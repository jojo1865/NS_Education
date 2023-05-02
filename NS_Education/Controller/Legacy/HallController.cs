using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Hall;
using NS_Education.Models.APIItems.Hall.GetList;
using NS_Education.Models.APIItems.Hall.Submit;
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
        IDeleteItem<D_Hall>,
        ISubmit<D_Hall, Hall_Submit_Input_APIItem>,
        IChangeActive<D_Hall>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Hall_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<Hall_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;

        public HallController()
        {
            _getListPagedHelper = new GetListPagedHelper<HallController, D_Hall, Hall_GetList_Input_APIItem, Hall_GetList_Output_Row_APIItem>(
                this);
            _deleteItemHelper = new DeleteItemHelper<HallController, D_Hall>(this);
            _submitHelper = new SubmitHelper<HallController, D_Hall, Hall_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<HallController, D_Hall>(this);
        }

        #endregion
        
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Hall_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Hall_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DDID.IsZeroOrAbove(), () => AddError(WrongFormat("所屬部門 ID")))
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
                BusinessTaxRate = entity.BusinessTaxRatePercentage / 100m,
                DeviceCt = entity.B_Device?.Count ?? 0,
                SiteCt = entity.B_SiteData?.Count ?? 0,
                PartnerItemCt = entity.B_PartnerItem?.Count ?? 0
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Hall.Include(q => q.DD).FirstOrDefaultAsync(q => q.DHID == ID && !q.DeleteFlag);
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
                    BusinessTaxRate = N.BusinessTaxRatePercentage / 100m,

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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<D_Hall> ChangeActiveQuery(int id)
        {
            return DC.D_Hall.Where(dh => dh.DHID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Hall_Submit_Input_APIItem.DHID))]
        public async Task<string> Submit(Hall_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Hall_Submit_Input_APIItem input)
        {
            return input.DHID == 0;
        }
        
        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Hall_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DHID == 0, () => AddError(WrongFormat("廳別 ID")))
                .Validate(i => i.DDID.IsAboveZero(), () => AddError(EmptyNotAllowed("部門 ID")))
                .Validate(i => i.CheckType == 0 || i.CheckType == 1, () => AddError(WrongFormat("開立憑證種類")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_Hall> SubmitCreateData(Hall_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_Hall
            {
                Code = input.Code,
                TitleC = input.TitleC,
                TitleE = input.TitleE,
                DDID = input.DDID,
                DiscountFlag = input.DiscountFlag,
                CheckoutNowFlag = input.CheckoutNowFlag,
                PrintCheckFlag = input.PrintCheckFlag,
                Invoice3Flag = input.Invoice3Flag,
                CheckType = input.CheckType,
                BusinessTaxRatePercentage = (int)(input.BusinessTaxRate * 100)
            });
        }
        
        #endregion
        
        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Hall_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DHID.IsAboveZero(), () => AddError(EmptyNotAllowed("廳別 ID")))
                .Validate(i => i.DDID.IsAboveZero(), () => AddError(EmptyNotAllowed("部門 ID")))
                .Validate(i => i.CheckType == 0 || i.CheckType == 1, () => AddError(WrongFormat("開立憑證種類")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_Hall> SubmitEditQuery(Hall_Submit_Input_APIItem input)
        {
            return DC.D_Hall.Where(h => h.DHID == input.DHID);
        }

        public void SubmitEditUpdateDataFields(D_Hall data, Hall_Submit_Input_APIItem input)
        {
            data.DDID = input.DDID;
            data.Code = input.Code ?? data.Code;
            data.TitleC = input.TitleC ?? data.TitleC;
            data.TitleE = input.TitleE ?? data.TitleE;
            data.DiscountFlag = input.DiscountFlag;
            data.CheckoutNowFlag = input.CheckoutNowFlag;
            data.PrintCheckFlag = input.PrintCheckFlag;
            data.Invoice3Flag = input.Invoice3Flag;
            data.CheckType = input.CheckType;
            data.BusinessTaxRatePercentage = (int)(input.BusinessTaxRate * 100);
        }
        
        #endregion

        #endregion
    }
}