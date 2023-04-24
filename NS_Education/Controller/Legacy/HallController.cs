using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Hall;
using NS_Education.Models.APIItems.Hall.GetList;
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
    public class HallController : PublicClass,
        IGetListPaged<D_Hall, Hall_GetList_Input_APIItem, Hall_GetList_Output_Row_APIItem>,
        IDeleteItem<D_Hall>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Hall_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        public HallController()
        {
            _getListPagedHelper = new GetListPagedHelper<HallController, D_Hall, Hall_GetList_Input_APIItem, Hall_GetList_Output_Row_APIItem>(
                this);
            _deleteItemHelper = new DeleteItemHelper<HallController, D_Hall>(this);
        }

        #endregion
        
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(Hall_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Hall_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DDID.IsValidIdOrZero(), () => AddError(WrongFormat("所屬部門 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Hall> GetListPagedOrderedQuery(Hall_GetList_Input_APIItem input)
        {
            var query = DC.D_Hall
                .Include(h => h.DD)
                .Include(h => h.B_Device)
                .Include(h => h.B_SiteData)
                .Include(h => h.B_PartnerItem)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(h =>
                    h.TitleC.Contains(input.Keyword) || h.TitleE.Contains(input.Keyword) ||
                    h.Code.Contains(input.Keyword));

            if (input.DDID > 0)
                query = query.Where(h => h.DDID == input.DDID);

            return query.OrderBy(h => h.DHID);
        }

        public async Task<Hall_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Hall entity)
        {
            return await Task.FromResult(new Hall_GetList_Output_Row_APIItem
            {
                DDID = entity.DDID,
                DHID = entity.DHID,
                DD_TitleC = entity.DD?.TitleC ?? "",
                DD_TitleE = entity.DD?.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                DiscountFlag = entity.DiscountFlag,
                CheckoutNowFlag = entity.CheckoutNowFlag,
                PrintCheckFlag = entity.PrintCheckFlag,
                Invoice3Flag = entity.Invoice3Flag,
                CheckType = entity.CheckType,
                BusinessTaxRate = entity.BusinessTaxRate,
                DeviceCt = entity.B_Device?.Count ?? 0,
                SiteCt = entity.B_SiteData?.Count ?? 0,
                PartnerItemCt = entity.B_PartnerItem?.Count ?? 0
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Hall.Include(q => q.DD).FirstOrDefaultAsync(q => q.DDID == ID && !q.DeleteFlag);
            D_Hall_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Deps = DC.D_Department.Where(q => !q.DeleteFlag).OrderBy(q => q.TitleC);

                foreach (var Dep in await Deps.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Dep.DDID, Title = Dep.TitleC, SelectFlag = N.DDID == Dep.DDID });
                Item = new D_Hall_APIItem
                {
                    DDID = N.DDID,
                    DHID = N.DHID,
                    DD_TitleC = N.DD.TitleC,
                    DD_TitleE = N.DD.TitleE,
                    DD_List = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,

                    DiscountFlag = N.DiscountFlag,
                    CheckoutNowFlag = N.CheckoutNowFlag,
                    PrintCheckFlag = N.PrintCheckFlag,
                    Invoice3Flag = N.Invoice3Flag,
                    CheckType = N.CheckType,
                    BusinessTaxRate = N.BusinessTaxRate,

                    DeviceCt = N.B_Device.Count,
                    SiteCt = N.B_SiteData.Count,
                    PartnerItemCt = N.B_PartnerItem.Count,

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
            var N_ = await DC.D_Hall.FirstOrDefaultAsync(q => q.DHID == ID && !q.DeleteFlag);
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

        public IQueryable<D_Hall> DeleteItemQuery(int id)
        {
            return DC.D_Hall.Where(h => h.DHID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_Hall.DHID))]
        public async Task<string> Submit(D_Hall N)
        {
            Error = "";
            if (N.DHID == 0)
            {
                if (N.DDID <= 0)
                    Error += "請選擇廳別所屬部門;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Hall.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Hall.FirstOrDefaultAsync(q => q.DHID == N.DHID && !q.DeleteFlag);
                if (N.DDID <= 0)
                    Error += "請選擇廳別所屬部門;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.DDID = N.DDID;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
                    N_.DiscountFlag = N.DiscountFlag;
                    N_.CheckoutNowFlag = N.CheckoutNowFlag;
                    N_.PrintCheckFlag = N.PrintCheckFlag;
                    N_.Invoice3Flag = N.Invoice3Flag;
                    N_.CheckType = N.CheckType;
                    N_.BusinessTaxRate = N.BusinessTaxRate;
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