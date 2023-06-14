using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.CustomerVisit.GetInfoById;
using NS_Education.Models.APIItems.Controller.CustomerVisit.GetList;
using NS_Education.Models.APIItems.Controller.CustomerVisit.Submit;
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
    public class CustomerVisitController : PublicClass
        , IGetListPaged<CustomerVisit, CustomerVisit_GetList_Input_APIItem, CustomerVisit_GetList_Output_Row_APIItem>
        , IGetInfoById<CustomerVisit, CustomerVisit_GetInfoById_Output_APIItem>
        , IDeleteItem<CustomerVisit>
        , ISubmit<CustomerVisit, CustomerVisit_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<CustomerVisit_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<CustomerVisit_Submit_Input_APIItem> _submitHelper;

        public CustomerVisitController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CustomerVisitController, CustomerVisit, CustomerVisit_GetList_Input_APIItem,
                    CustomerVisit_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<CustomerVisitController, CustomerVisit, CustomerVisit_GetInfoById_Output_APIItem>(
                    this);

            _deleteItemHelper =
                new DeleteItemHelper<CustomerVisitController, CustomerVisit>(this);
            _submitHelper =
                new SubmitHelper<CustomerVisitController, CustomerVisit, CustomerVisit_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        private const string GetListDateRangeIncorrect = "欲篩選之拜訪期間起始日期不得大於最後日期！";

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CustomerVisit_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(CustomerVisit_GetList_Input_APIItem input)
        {
            DateTime sDate = default;
            DateTime eDate = default;

            bool isValid = input.StartValidate()
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.BUID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之業務員 ID")))
                .Validate(i => i.BSCID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之拜訪方式 ID")))
                .Validate(i => i.SDate.IsNullOrWhiteSpace() || i.SDate.TryParseDateTime(out sDate),
                    () => AddError(WrongFormat("欲篩選之拜訪期間起始日期")))
                .Validate(i => i.EDate.IsNullOrWhiteSpace() || i.EDate.TryParseDateTime(out eDate),
                    () => AddError(WrongFormat("欲篩選之拜訪期間最後日期")))
                .Validate(i => sDate.Date <= eDate.Date, () => AddError(GetListDateRangeIncorrect))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<CustomerVisit> GetListPagedOrderedQuery(CustomerVisit_GetList_Input_APIItem input)
        {
            var query = DC.CustomerVisit
                .Include(cv => cv.Customer)
                .Include(cv => cv.Customer.Resver_Head)
                .Include(cv => cv.B_StaticCode1)
                .Include(cv => cv.BusinessUser)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(cv => cv.Title.Contains(input.Keyword) || cv.TargetTitle.Contains(input.Keyword));

            if (input.CID.IsAboveZero())
                query = query.Where(cv => cv.CID == input.CID);

            if (input.BUID.IsAboveZero())
                query = query.Where(cv => cv.BUID == input.BUID);

            if (input.BSCID.IsAboveZero())
                query = query.Where(cv => cv.BSCID == input.BSCID);

            if (input.SDate.TryParseDateTime(out DateTime sDate))
                query = query.Where(cv => DbFunctions.TruncateTime(cv.VisitDate) >= sDate.Date);

            if (input.EDate.TryParseDateTime(out DateTime eDate))
                query = query.Where(cv => DbFunctions.TruncateTime(cv.VisitDate) <= eDate.Date);

            if (input.HasReservation != null)
                query = query.Where(cv => cv.Customer.Resver_Head.Any(rh => !rh.DeleteFlag) == input.HasReservation);

            return query.OrderByDescending(cv => cv.VisitDate)
                .ThenBy(cv => cv.CID)
                .ThenBy(cv => cv.BUID)
                .ThenBy(cv => cv.CVID);
        }

        public async Task<CustomerVisit_GetList_Output_Row_APIItem> GetListPagedEntityToRow(CustomerVisit entity)
        {
            return await Task.FromResult(new CustomerVisit_GetList_Output_Row_APIItem
            {
                CVID = entity.CVID,
                CID = entity.CID,
                C_TitleC = entity.Customer?.TitleC ?? "",
                C_TitleE = entity.Customer?.TitleE ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode1?.Title ?? "",
                BUID = entity.BUID,
                BU_Name = entity.BusinessUser?.Name ?? "",
                TargetTitle = entity.TargetTitle ?? "",
                Title = entity.Title ?? "",
                VisitDate = entity.VisitDate.ToFormattedStringDate(),
                Description = entity.Description ?? "",
                AfterNote = entity.AfterNote ?? "",
                HasReservation = entity.Customer?.Resver_Head.Any(rh => !rh.DeleteFlag) ?? false
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

        public IQueryable<CustomerVisit> GetInfoByIdQuery(int id)
        {
            return DC.CustomerVisit
                .Include(cv => cv.Customer)
                .Include(cv => cv.B_StaticCode)
                .Include(cv => cv.B_StaticCode1)
                .Include(cv => cv.BusinessUser)
                .Where(cv => cv.CVID == id);
        }

        public async Task<CustomerVisit_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            CustomerVisit entity)
        {
            return new CustomerVisit_GetInfoById_Output_APIItem
            {
                CVID = entity.CVID,
                CID = entity.CID,
                C_TitleC = entity.Customer?.TitleC ?? "",
                C_TitleE = entity.Customer?.TitleE ?? "",
                C_List = await DC.Customer.GetCustomerSelectable(entity.CID),
                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode1?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode1?.CodeType, entity.BSCID),
                BUID = entity.BUID,
                BU_Name = entity.BusinessUser?.Name ?? "",
                BU_List = await GetSelectedBusinessUserList(entity.BUID),
                TargetTitle = entity.TargetTitle ?? "",
                Title = entity.Title ?? "",
                VisitDate = entity.VisitDate.ToFormattedStringDate(),
                Description = entity.Description ?? "",
                AfterNote = entity.AfterNote ?? "",
                NoDealReason = entity.B_StaticCode?.Title ?? "",
                NoDealReasons_List =
                    await DC.B_StaticCode.GetStaticCodeSelectable((int)StaticCodeType.NoDealReason, entity.BSCID15 ?? 0)
            };
        }

        private async Task<List<BaseResponseRowForSelectable>> GetSelectedBusinessUserList(int businessUserId)
        {
            return await DC.BusinessUser
                .Where(bu => bu.ActiveFlag && !bu.DeleteFlag)
                .Select(bu => new BaseResponseRowForSelectable
                {
                    ID = bu.BUID,
                    Title = bu.Name ?? "",
                    SelectFlag = bu.BUID == businessUserId
                })
                .ToListAsync();
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<CustomerVisit> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.CustomerVisit.Where(cv => ids.Contains(cv.CVID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null,
            nameof(CustomerVisit_Submit_Input_APIItem.CVID))]
        public async Task<string> Submit(CustomerVisit_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(CustomerVisit_Submit_Input_APIItem input)
        {
            return input.CVID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(CustomerVisit_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.CVID == 0, () => AddError(WrongFormat("拜訪紀錄 ID")))
                .ValidateAsync(async i => await DC.Customer.ValidateIdExists(i.CID, nameof(Customer.CID)),
                    () => AddError(NotFound("客戶 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.VisitMethod),
                    () => AddError(NotFound("客戶拜訪方式 ID")))
                .ValidateAsync(async i => await DC.BusinessUser.ValidateIdExists(i.BUID, nameof(BusinessUser.BUID)),
                    () => AddError(NotFound("拜訪業務 ID")))
                .Validate(i => i.TargetTitle.HasContent(), () => AddError(EmptyNotAllowed("拜訪對象")))
                .Validate(i => i.TargetTitle.HasLengthBetween(1, 100), () => AddError(LengthOutOfRange("拜訪對象", 1, 100)))
                .Validate(i => i.Title.HasLengthBetween(0, 100), () => AddError(LengthOutOfRange("主旨", 0, 100)))
                .Validate(i => i.VisitDate.TryParseDateTime(out _), () => AddError(WrongFormat("拜訪日期")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<CustomerVisit> SubmitCreateData(CustomerVisit_Submit_Input_APIItem input)
        {
            input.VisitDate.TryParseDateTime(out DateTime visitDate);
            return await Task.FromResult(new CustomerVisit
            {
                CID = input.CID,
                BSCID = input.BSCID,
                BUID = input.BUID,
                TargetTitle = input.TargetTitle,
                VisitDate = visitDate,
                Title = input.Title,
                Description = input.Description,
                AfterNote = input.AfterNote
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(CustomerVisit_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.CVID.IsAboveZero(), () => AddError(EmptyNotAllowed("拜訪紀錄 ID")))
                .ValidateAsync(async i => await DC.Customer.ValidateIdExists(i.CID, nameof(Customer.CID)),
                    () => AddError(NotFound("客戶 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.VisitMethod),
                    () => AddError(NotFound("客戶拜訪方式 ID")))
                .ValidateAsync(async i => await DC.BusinessUser.ValidateIdExists(i.BUID, nameof(BusinessUser.BUID)),
                    () => AddError(NotFound("拜訪業務 ID")))
                .Validate(i => i.TargetTitle.HasContent(), () => AddError(EmptyNotAllowed("拜訪對象")))
                .Validate(i => i.TargetTitle.HasLengthBetween(1, 100), () => AddError(LengthOutOfRange("拜訪對象", 1, 100)))
                .Validate(i => i.Title.HasLengthBetween(0, 100), () => AddError(LengthOutOfRange("主旨", 0, 100)))
                .Validate(i => i.VisitDate.TryParseDateTime(out _), () => AddError(WrongFormat("拜訪日期")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<CustomerVisit> SubmitEditQuery(CustomerVisit_Submit_Input_APIItem input)
        {
            return DC.CustomerVisit.Where(cv => cv.CVID == input.CVID);
        }

        public void SubmitEditUpdateDataFields(CustomerVisit data, CustomerVisit_Submit_Input_APIItem input)
        {
            data.CID = input.CID;
            data.BSCID = input.BSCID;
            data.BUID = input.BUID;
            data.TargetTitle = input.TargetTitle;

            input.VisitDate.TryParseDateTime(out DateTime visitDate);
            data.VisitDate = visitDate;

            data.Title = input.Title;
            data.Description = input.Description;
            data.AfterNote = input.AfterNote;
        }

        #endregion

        #endregion
    }
}