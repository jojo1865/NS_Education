using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Company;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.Legacy
{
    public class CompanyController : PublicClass,
        IGetListPaged<D_Company, Company_GetList_Input_APIItem, Company_GetList_Output_Row_APIItem>,
        IDeleteItem<D_Company>
    {
        #region Intialization

        private readonly IGetListPagedHelper<Company_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        public CompanyController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CompanyController, D_Company, Company_GetList_Input_APIItem,
                    Company_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<CompanyController, D_Company>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(Company_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Company_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsValidIdOrZero(), () => AddError(EmptyNotAllowed("資料所屬分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Company> GetListPagedOrderedQuery(Company_GetList_Input_APIItem input)
        {
            var query = DC.D_Company.Include(c => c.BC).AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(c =>
                    c.TitleC.Contains(input.Keyword) || c.TitleE.Contains(input.Keyword) ||
                    c.Code.Contains(input.Keyword));

            if (input.BCID.IsValidId())
                query = query.Where(c => c.BCID == input.BCID);

            return query.OrderBy(c => c.DCID);
        }

        public async Task<Company_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Company entity)
        {
            return await Task.FromResult(new Company_GetList_Output_Row_APIItem
            {
                DCID = entity.DCID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                DepartmentCt = 0
            });
        }

        #endregion

        #region GetInfoByID

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Company.Include(q => q.BC).FirstOrDefaultAsync(q => q.DCID == ID && !q.DeleteFlag);
            D_Company_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Cats = DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == 1).OrderBy(q => q.SortNo);
                foreach (var Cat in await Cats.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Cat.BCID, Title = Cat.TitleC, SelectFlag = N.BCID == Cat.BCID });
                Item = new D_Company_APIItem
                {
                    DCID = N.DCID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    BC_List = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    DepartmentCt = N.D_Department.Count,
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }

            return ChangeJson(Item);
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.D_Company.FirstOrDefaultAsync(q => q.DCID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_Company> DeleteItemQuery(int id)
        {
            return DC.D_Company.Where(c => c.DCID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_Company.DCID))]
        public async Task<string> Submit(D_Company N)
        {
            Error = "";
            if (N.DCID == 0)
            {
                if (N.BCID <= 0)
                    Error += "請選擇公司所屬分類;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Company.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Company.FirstOrDefaultAsync(q => q.DCID == N.DCID && !q.DeleteFlag);
                if (N.BCID <= 0)
                    Error += "請選擇公司所屬分類;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.BCID = N.BCID;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
                    N_.ActiveFlag = N.ActiveFlag;
                    N_.DeleteFlag = N.DeleteFlag;
                    N_.UpdUID = GetUid();
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion
    }
}