using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Department;
using NS_Education.Models.APIItems.Department.GetList;
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
    public class DepartmentController : PublicClass
        , IGetListPaged<D_Department, Department_GetList_Input_APIItem, Department_GetList_Output_Row_APIItem>
        , IDeleteItem<D_Department>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Department_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        public DepartmentController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<DepartmentController, D_Department, Department_GetList_Input_APIItem,
                    Department_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new DeleteItemHelper<DepartmentController, D_Department>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(Department_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Department_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DCID.IsValidIdOrZero(), () => AddError(WrongFormat("所屬公司 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Department> GetListPagedOrderedQuery(Department_GetList_Input_APIItem input)
        {
            var query = DC.D_Department
                .Include(d => d.DC)
                .Include(d => d.D_Hall)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(d =>
                    d.TitleC.Contains(input.Keyword) || d.TitleE.Contains(input.Keyword) ||
                    d.Code.Contains(input.Keyword));

            if (input.DCID.IsValidId())
                query = query.Where(d => d.DCID == input.DCID);

            return query.OrderBy(d => d.DDID);
        }

        public async Task<Department_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Department entity)
        {
            return await Task.FromResult(new Department_GetList_Output_Row_APIItem
            {
                DDID = entity.DDID,
                DCID = entity.DCID,
                DC_TitleC = entity.DC.TitleC ?? "",
                DC_TitleE = entity.DC.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                PeopleCt = entity.PeopleCt,
                HallCt = entity.D_Hall?.Count ?? 0
            });
        }

        #endregion

        #region GetInfoByID

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Department.Include(q => q.DC).FirstOrDefaultAsync(q => q.DDID == ID && !q.DeleteFlag);
            D_Department_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Coms = DC.D_Company.Where(q => !q.DeleteFlag).OrderBy(q => q.TitleC);
                foreach (var Com in await Coms.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Com.DCID, Title = Com.TitleC, SelectFlag = N.DCID == Com.DCID });
                Item = new D_Department_APIItem
                {
                    DDID = N.DDID,
                    DCID = N.DCID,
                    DC_TitleC = N.DC.TitleC,
                    DC_TitleE = N.DC.TitleE,
                    DC_List = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    PeopleCt = N.PeopleCt,
                    HallCt = N.D_Hall.Count,
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
            var N_ = await DC.D_Department.FirstOrDefaultAsync(q => q.DDID == ID && !q.DeleteFlag);
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

        public IQueryable<D_Department> DeleteItemQuery(int id)
        {
            return DC.D_Department.Where(d => d.DDID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_Department.DDID))]
        public async Task<string> Submit(D_Department N)
        {
            Error = "";
            if (N.DDID == 0)
            {
                if (N.DCID <= 0)
                    Error += "請選擇部門所屬公司;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Department.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Department.FirstOrDefaultAsync(q => q.DDID == N.DDID && !q.DeleteFlag);
                if (N.DCID <= 0)
                    Error += "請選擇部門所屬公司;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.DCID = N.DCID;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
                    N_.PeopleCt = N.PeopleCt;
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