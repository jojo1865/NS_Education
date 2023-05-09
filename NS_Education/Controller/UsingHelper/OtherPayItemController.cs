using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.OtherPayItem.GetInfoById;
using NS_Education.Models.APIItems.OtherPayItem.GetList;
using NS_Education.Models.APIItems.OtherPayItem.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper
{
    public class OtherPayItemController : PublicClass,
        IGetListPaged<D_OtherPayItem, OtherPayItem_GetList_Input_APIItem, OtherPayItem_GetList_Output_Row_APIItem>,
        IGetInfoById<D_OtherPayItem, OtherPayItem_GetInfoById_Output_APIItem>,
        IDeleteItem<D_OtherPayItem>,
        ISubmit<D_OtherPayItem, OtherPayItem_Submit_Input_APIItem>, 
        IChangeActive<D_OtherPayItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<OtherPayItem_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<OtherPayItem_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public OtherPayItemController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<OtherPayItemController, D_OtherPayItem, OtherPayItem_GetList_Input_APIItem,
                    OtherPayItem_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new DeleteItemHelper<OtherPayItemController, D_OtherPayItem>(this);
            _submitHelper =
                new SubmitHelper<OtherPayItemController, D_OtherPayItem, OtherPayItem_Submit_Input_APIItem>(this);

            _changeActiveHelper = new ChangeActiveHelper<OtherPayItemController, D_OtherPayItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<OtherPayItemController, D_OtherPayItem, OtherPayItem_GetInfoById_Output_APIItem>(
                    this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(OtherPayItem_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(OtherPayItem_GetList_Input_APIItem input)
        {
            // 此功能無須驗證輸入
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<D_OtherPayItem> GetListPagedOrderedQuery(OtherPayItem_GetList_Input_APIItem input)
        {
            var query = DC.D_OtherPayItem.AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(opi => opi.Title.Contains(input.Keyword) || opi.Code.Contains(input.Keyword));

            return query.OrderBy(opi => opi.Code)
                .ThenBy(opi => opi.DOPIID);
        }

        public async Task<OtherPayItem_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_OtherPayItem entity)
        {
            return await Task.FromResult(new OtherPayItem_GetList_Output_Row_APIItem
            {
                DOPIID = entity.DOPIID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                PaidType = entity.PaidType,
                BSCID = entity.BSCID,
                BOCID = entity.BOCID
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<D_OtherPayItem> GetInfoByIdQuery(int id)
        {
            return DC.D_OtherPayItem.Where(opi => opi.DOPIID == id);
        }

        public async Task<OtherPayItem_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_OtherPayItem entity)
        {
            return await Task.FromResult(new OtherPayItem_GetInfoById_Output_APIItem
            {
                DOPIID = entity.DOPIID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                PaidType = entity.PaidType,
                BSCID = entity.BSCID,
                BOCID = entity.BOCID
            });
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<D_OtherPayItem> ChangeActiveQuery(int id)
        {
            return DC.D_OtherPayItem.Where(opi => opi.DOPIID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_OtherPayItem> DeleteItemQuery(int id)
        {
            return DC.D_OtherPayItem.Where(opi => opi.DOPIID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null,
            nameof(OtherPayItem_Submit_Input_APIItem.DOPIID))]
        public async Task<string> Submit(OtherPayItem_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(OtherPayItem_Submit_Input_APIItem input)
        {
            return input.DOPIID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(OtherPayItem_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DOPIID == 0, () => AddError(WrongFormat("項目 ID")))
                .Validate(i => i.PaidType.IsInBetween(0, 1), () => AddError(WrongFormat("計價方式")))
                .ValidateAsync(async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Unit), () => AddError(NotFound("單位 ID")))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID), () => AddError(NotFound("入帳代號 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_OtherPayItem> SubmitCreateData(OtherPayItem_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_OtherPayItem
            {
                DOPIID = 0,
                Code = input.Code,
                Title = input.Title,
                Ct = input.Ct,
                UnitPrice = input.UnitPrice,
                InPrice = input.InPrice,
                OutPrice = input.OutPrice,
                PaidType = input.PaidType,
                BSCID = input.BSCID,
                BOCID = input.BOCID
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(OtherPayItem_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DOPIID.IsAboveZero(), () => AddError(EmptyNotAllowed("項目 ID")))
                .Validate(i => i.PaidType.IsInBetween(0, 1), () => AddError(WrongFormat("計價方式")))
                .ValidateAsync(async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Unit), () => AddError(NotFound("單位 ID")))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID), () => AddError(NotFound("入帳代號 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_OtherPayItem> SubmitEditQuery(OtherPayItem_Submit_Input_APIItem input)
        {
            return DC.D_OtherPayItem.Where(opi => opi.DOPIID == input.DOPIID);
        }

        public void SubmitEditUpdateDataFields(D_OtherPayItem data, OtherPayItem_Submit_Input_APIItem input)
        {
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.Ct = input.Ct;
            data.UnitPrice = input.UnitPrice;
            data.InPrice = input.InPrice;
            data.OutPrice = input.OutPrice;
            data.PaidType = input.PaidType;
            data.BSCID = input.BSCID;
            data.BOCID = input.BOCID;
        }

        #endregion

        #endregion
    }
}