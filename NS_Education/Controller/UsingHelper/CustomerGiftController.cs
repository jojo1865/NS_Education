using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.CustomerGift.GetInfoById;
using NS_Education.Models.APIItems.CustomerGift.GetList;
using NS_Education.Models.APIItems.CustomerGift.Submit;
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
    public class CustomerGiftController : PublicClass,
        IGetListPaged<CustomerGift, CustomerGift_GetList_Input_APIItem, CustomerGift_GetList_Output_Row_APIItem>,
        IGetInfoById<CustomerGift, CustomerGift_GetInfoById_Output_APIItem>,
        IDeleteItem<CustomerGift>,
        ISubmit<CustomerGift, CustomerGift_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<CustomerGift_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<CustomerGift_Submit_Input_APIItem> _submitHelper;

        public CustomerGiftController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CustomerGiftController, CustomerGift, CustomerGift_GetList_Input_APIItem,
                    CustomerGift_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<CustomerGiftController, CustomerGift, CustomerGift_GetInfoById_Output_APIItem>(
                    this);

            _deleteItemHelper =
                new DeleteItemHelper<CustomerGiftController, CustomerGift>(this);

            _submitHelper =
                new SubmitHelper<CustomerGiftController, CustomerGift, CustomerGift_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CustomerGift_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(CustomerGift_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate(true)
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.SendYear.IsInBetween(1911, 9999), () => AddError(WrongFormat("欲篩選之贈送年分")))
                .Validate(i =>
                        !i.SDate.TryParseDateTime(out DateTime startDate)
                        || !i.EDate.TryParseDateTime(out DateTime endDate)
                        || endDate >= startDate
                    , () => AddError("GetDateRangeIncorrect"))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<CustomerGift> GetListPagedOrderedQuery(CustomerGift_GetList_Input_APIItem input)
        {
            var query = DC.CustomerGift
                .Include(cg => cg.C)
                .Include(cg => cg.BSC)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(cg => cg.Title.Contains(input.Keyword));

            if (input.CID.IsAboveZero())
                query = query.Where(cg => cg.CID == input.CID);

            query = query.Where(cg => cg.Year == input.SendYear);

            if (input.SDate.TryParseDateTime(out DateTime startDate))
                query = query.Where(cg => cg.SendDate.Date >= startDate.Date);

            if (input.EDate.TryParseDateTime(out DateTime endDate))
                query = query.Where(cg => cg.SendDate.Date <= endDate.Date);

            return query.OrderByDescending(cg => cg.SendDate)
                .ThenBy(cg => cg.CID)
                .ThenBy(cg => cg.CGID);
        }

        public async Task<CustomerGift_GetList_Output_Row_APIItem> GetListPagedEntityToRow(CustomerGift entity)
        {
            return await Task.FromResult(new CustomerGift_GetList_Output_Row_APIItem
            {
                CGID = entity.CGID,
                CID = entity.CID,
                C_TitleC = entity.C?.TitleC ?? "",
                C_TitleE = entity.C?.TitleE ?? "",
                Year = entity.Year,
                SendDate = entity.SendDate.ToFormattedStringDateTime(),
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
                Note = entity.Note ?? ""
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

        public IQueryable<CustomerGift> GetInfoByIdQuery(int id)
        {
            return DC.CustomerGift
                .Include(cg => cg.C)
                .Include(cg => cg.BSC)
                .Where(cg => cg.CGID == id);
        }

        public async Task<CustomerGift_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            CustomerGift entity)
        {
            return await Task.FromResult(new CustomerGift_GetInfoById_Output_APIItem
            {
                CGID = entity.CGID,
                CID = entity.CID,
                C_TitleC = entity.C?.TitleC ?? "",
                C_TitleE = entity.C?.TitleE ?? "",
                C_List = await DC.Customer.GetCustomerSelectable(entity.CID),
                Year = entity.Year,
                SendDate = entity.SendDate.ToFormattedStringDateTime(),
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSC?.CodeType, entity.BSCID),
                Title = entity.Title ?? "",
                Ct = entity.Ct,
                Note = entity.Note ?? ""
            });
        }

        #endregion

        #region DeleteItem
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<CustomerGift> DeleteItemQuery(int id)
        {
            return DC.CustomerGift.Where(cg => cg.CGID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(CustomerGift_Submit_Input_APIItem.CGID))]
        public async Task<string> Submit(CustomerGift_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(CustomerGift_Submit_Input_APIItem input)
        {
            return input.CGID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(CustomerGift_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate(true)
                .Validate(i => i.CGID == 0, () => AddError(WrongFormat("禮品贈與紀錄 ID")))
                .Validate(i => i.CID.IsAboveZero(), () => AddError(EmptyNotAllowed("客戶 ID")))
                .Validate(i => i.Year.IsInBetween(1911, 9999), () => AddError(WrongFormat("禮品贈送代表年分")))
                .Validate(i => i.SendDate.TryParseDateTime(out _), () => AddError(WrongFormat("禮品贈與時間")))
                .Validate(i => i.BSCID.IsAboveZero(), () => AddError(EmptyNotAllowed("禮品 ID")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("禮品實際名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<CustomerGift> SubmitCreateData(CustomerGift_Submit_Input_APIItem input)
        {
            input.SendDate.TryParseDateTime(out DateTime sendDate);
            
            return await Task.FromResult(new CustomerGift
            {
                CID = input.CID,
                Year = input.Year,
                SendDate = sendDate,
                BSCID = input.BSCID,
                Title = input.Title,
                Ct = input.Ct,
                Note = input.Note
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(CustomerGift_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate(true)
                .Validate(i => i.CGID.IsAboveZero(), () => AddError(EmptyNotAllowed("禮品贈與紀錄 ID")))
                .Validate(i => i.CID.IsAboveZero(), () => AddError(EmptyNotAllowed("客戶 ID")))
                .Validate(i => i.Year.IsInBetween(1911, 9999), () => AddError(WrongFormat("禮品贈送代表年分")))
                .Validate(i => i.SendDate.TryParseDateTime(out _), () => AddError(WrongFormat("禮品贈與時間")))
                .Validate(i => i.BSCID.IsAboveZero(), () => AddError(EmptyNotAllowed("禮品 ID")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("禮品實際名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<CustomerGift> SubmitEditQuery(CustomerGift_Submit_Input_APIItem input)
        {
            return DC.CustomerGift.Where(cg => cg.CGID == input.CGID);
        }

        public void SubmitEditUpdateDataFields(CustomerGift data, CustomerGift_Submit_Input_APIItem input)
        {
            input.SendDate.TryParseDateTime(out DateTime sendDate);
            
            data.CID = input.CID;
            data.Year = input.Year;
            data.SendDate = sendDate;
            data.BSCID = input.BSCID;
            data.Title = input.Title;
            data.Ct = input.Ct;
            data.Note = input.Note;
        }

        #endregion

        #endregion
    }
}