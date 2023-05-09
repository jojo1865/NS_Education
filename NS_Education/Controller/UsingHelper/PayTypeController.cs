using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.PayType.GetInfoById;
using NS_Education.Models.APIItems.PayType.GetList;
using NS_Education.Models.APIItems.PayType.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    public class PayTypeController : PublicClass,
        IGetListPaged<D_PayType, PayType_GetList_Input_APIItem, PayType_GetList_Output_APIItem>,
        IGetInfoById<D_PayType, PayType_GetInfoById_Output_APIItem>,
        IDeleteItem<D_PayType>,
        ISubmit<D_PayType, PayType_Submit_Input_APIItem>,
        IChangeActive<D_PayType>
    {
        #region Initialization

        private readonly IGetListPagedHelper<PayType_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<PayType_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public PayTypeController()
        {
            _getListPagedHelper = new GetListPagedHelper<PayTypeController, D_PayType, PayType_GetList_Input_APIItem,
                PayType_GetList_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<PayTypeController, D_PayType>(this);
            _submitHelper = new SubmitHelper<PayTypeController, D_PayType, PayType_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<PayTypeController, D_PayType>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<PayTypeController, D_PayType, PayType_GetInfoById_Output_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(PayType_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(PayType_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(), () => AddError(EmptyNotAllowed("所屬分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_PayType> GetListPagedOrderedQuery(PayType_GetList_Input_APIItem input)
        {
            var query = DC.D_PayType
                .Include(pt => pt.BC)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(pt => pt.Title.Contains(input.Keyword) || pt.Code.Contains(input.Keyword));

            if (input.BCID > 0)
                query = query.Where(pt => pt.BCID == input.BCID);

            return query.OrderBy(pt => pt.DPTID);
        }

        public async Task<PayType_GetList_Output_APIItem> GetListPagedEntityToRow(D_PayType entity)
        {
            return await Task.FromResult(new PayType_GetList_Output_APIItem
            {
                DPTID = entity.DPTID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                AccountingNo = entity.AccountingNo,
                CustomerNo = entity.CustormerNo,
                InvoiceFlag = entity.InvoiceFlag,
                DepositFlag = entity.DepositFlag,
                RestaurantFlag = entity.RestaurantFlag,
                SimpleCheckoutFlag = entity.SimpleCheckoutFlag,
                SimpleDepositFlag = entity.DepositFlag
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

        public IQueryable<D_PayType> GetInfoByIdQuery(int id)
        {
            return DC.D_PayType
                .Include(pt => pt.BC)
                .Where(pt => pt.DPTID == id);
        }

        public async Task<PayType_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_PayType entity)
        {
            return new PayType_GetInfoById_Output_APIItem
            {
                DPTID = entity.DPTID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                BC_List = await DC.B_Category.GetCategorySelectable(entity.BC?.CategoryType, entity.BCID),
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                AccountingNo = entity.AccountingNo ?? "",
                CustomerNo = entity.CustormerNo ?? "",
                InvoiceFlag = entity.InvoiceFlag,
                DepositFlag = entity.DepositFlag,
                RestaurantFlag = entity.RestaurantFlag,
                SimpleCheckoutFlag = entity.SimpleCheckoutFlag,
                SimpleDepositFlag = entity.SimpleDepositFlag
            };
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<D_PayType> ChangeActiveQuery(int id)
        {
            return DC.D_PayType.Where(pt => pt.DPTID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_PayType> DeleteItemQuery(int id)
        {
            return DC.D_PayType.Where(pt => pt.DPTID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(PayType_Submit_Input_APIItem.DPTID))]
        public async Task<string> Submit(PayType_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(PayType_Submit_Input_APIItem input)
        {
            return input.DPTID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(PayType_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DPTID == 0, () => AddError(WrongFormat("付款方式 ID")))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, 8), () => AddError(NotFound("分類 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("編碼")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_PayType> SubmitCreateData(PayType_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_PayType
            {
                BCID = input.BCID,
                Code = input.Code,
                Title = input.Title,
                AccountingNo = input.AccountingNo,
                CustormerNo = input.CustomerNo,
                InvoiceFlag = input.InvoiceFlag,
                DepositFlag = input.DepositFlag,
                RestaurantFlag = input.RestaurantFlag,
                SimpleCheckoutFlag = input.SimpleCheckoutFlag,
                SimpleDepositFlag = input.SimpleDepositFlag
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(PayType_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DPTID.IsAboveZero(), () => AddError(WrongFormat("付款方式 ID")))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, 8), () => AddError(NotFound("分類 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("編碼")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_PayType> SubmitEditQuery(PayType_Submit_Input_APIItem input)
        {
            return DC.D_PayType.Where(pt => pt.DPTID == input.DPTID);
        }

        public void SubmitEditUpdateDataFields(D_PayType data, PayType_Submit_Input_APIItem input)
        {
            data.BCID = input.BCID;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.AccountingNo = input.AccountingNo ?? data.AccountingNo;
            data.CustormerNo = input.CustomerNo ?? data.CustormerNo;
            data.InvoiceFlag = input.InvoiceFlag;
            data.DepositFlag = input.DepositFlag;
            data.RestaurantFlag = input.RestaurantFlag;
            data.SimpleCheckoutFlag = input.SimpleCheckoutFlag;
            data.SimpleDepositFlag = input.SimpleDepositFlag;
        }

        #endregion

        #endregion
    }
}