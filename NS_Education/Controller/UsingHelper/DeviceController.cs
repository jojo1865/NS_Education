using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Device.GetInfoById;
using NS_Education.Models.APIItems.Controller.Device.GetList;
using NS_Education.Models.APIItems.Controller.Device.Submit;
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
    public class DeviceController : PublicClass,
        IGetListPaged<B_Device, Device_GetList_Input_APIItem, Device_GetList_Output_Row_APIItem>,
        IGetInfoById<B_Device, Device_GetInfoById_Output_APIItem>,
        IDeleteItem<B_Device>,
        IChangeActive<B_Device>,
        ISubmit<B_Device, Device_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Device_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly ISubmitHelper<Device_Submit_Input_APIItem> _submitHelper;

        public DeviceController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<DeviceController, B_Device, Device_GetList_Input_APIItem,
                    Device_GetList_Output_Row_APIItem>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<DeviceController, B_Device, Device_GetInfoById_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<DeviceController, B_Device>(this);
            _changeActiveHelper = new ChangeActiveHelper<DeviceController, B_Device>(this);
            _submitHelper = new SubmitHelper<DeviceController, B_Device, Device_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Device_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Device_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之所屬分類 ID")))
                .Validate(i => i.DHID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之所屬廳別 ID")))
                .Validate(i => i.BOCID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之所屬入帳代號 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_Device> GetListPagedOrderedQuery(Device_GetList_Input_APIItem input)
        {
            var query = DC.B_Device
                .Include(d => d.B_Category)
                .Include(d => d.B_StaticCode)
                .Include(d => d.B_OrderCode)
                .Include(d => d.D_Hall)
                .Include(d => d.M_Site_Device)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(d => d.Title.Contains(input.Keyword) || d.Code.Contains(input.Keyword));

            if (input.BCID.IsAboveZero())
                query = query.Where(d => d.BCID == input.BCID);

            if (input.DHID.IsAboveZero())
                query = query.Where(d => d.DHID == input.DHID);

            if (input.BOCID.IsAboveZero())
                query = query.Where(d => d.BOCID == input.BOCID);

            return query.OrderBy(d => d.BCID)
                .ThenBy(d => d.DHID)
                .ThenBy(d => d.BOCID)
                .ThenBy(d => d.BDID);
        }

        public async Task<Device_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_Device entity)
        {
            return await Task.FromResult(new Device_GetList_Output_Row_APIItem
            {
                BDID = entity.BDID,
                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode?.Title ?? "",
                BOCID = entity.BOCID,
                BOC_Title = entity.B_OrderCode?.Title ?? "",
                DHID = entity.DHID,
                DH_Title = entity.D_Hall?.TitleC ?? entity.D_Hall?.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.M_Site_Device.Sum(msd => msd.Ct),
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                SupplierTitle = entity.SupplierTitle ?? "",
                SupplierName = entity.SupplierName ?? "",
                SupplierPhone = entity.SupplierPhone ?? "",
                Repair = entity.Repair ?? "",
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

        public IQueryable<B_Device> GetInfoByIdQuery(int id)
        {
            return DC.B_Device
                .Include(d => d.B_Category)
                .Include(d => d.B_StaticCode)
                .Include(d => d.B_OrderCode)
                .Include(d => d.D_Hall)
                .Include(d => d.M_Site_Device)
                .Where(d => d.BDID == id);
        }

        public async Task<Device_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_Device entity)
        {
            return new Device_GetInfoById_Output_APIItem
            {
                BDID = entity.BDID,
                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                BC_List = await DC.B_Category.GetCategorySelectable(entity.B_Category?.CategoryType, entity.BCID),
                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode?.CodeType, entity.BSCID),
                BOCID = entity.BOCID,
                BOC_Title = entity.B_OrderCode?.Title ?? "",
                BOC_List = await DC.B_OrderCode.GetOrderCodeSelectable(entity.B_OrderCode?.CodeType, entity.BOCID),
                DHID = entity.DHID,
                DH_Title = entity.D_Hall?.TitleC ?? entity.D_Hall?.TitleE ?? "",
                DH_List = await DC.D_Hall.GetHallSelectable(entity.DHID),
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.M_Site_Device.Sum(msd => msd.Ct),
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                SupplierTitle = entity.SupplierTitle ?? "",
                SupplierName = entity.SupplierName ?? "",
                SupplierPhone = entity.SupplierPhone ?? "",
                Repair = entity.Repair ?? "",
                Note = entity.Note ?? ""
            };
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            if (!await DeleteItemValidateReservation(input))
                return GetResponseJson();

            return await _deleteItemHelper.DeleteItem(input);
        }

        private async Task<bool> DeleteItemValidateReservation(DeleteItem_Input_APIItem input)
        {
            var uniqueDeleteId = input.Items.Where(i => i.DeleteFlag == true && i.Id != null)
                .Select(i => i.Id.Value)
                .Distinct()
                .ToHashSet();

            // 欲刪除的設備不能有任何進行中預約單
            var cantDeleteData = await DC.Resver_Head
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.Resver_Site)))
                .Where(ResverHeadExpression.IsOngoingExpression)
                .SelectMany(rh => rh.Resver_Site)
                .Where(rs => !rs.DeleteFlag)
                .SelectMany(rs => rs.Resver_Device)
                .Where(rd => !rd.DeleteFlag)
                .Where(rd => uniqueDeleteId.Contains(rd.BDID))
                .ToArrayAsync();

            foreach (Resver_Device resverDevice in cantDeleteData)
            {
                AddError(UnsupportedValue(
                    $"欲刪除的設備（ID {resverDevice.BDID} {resverDevice.B_Device.Code ?? ""}{resverDevice.B_Device.Title ?? ""}）",
                    $"已有進行中預約單（單號 {resverDevice.Resver_Site.RHID}）"));
            }

            return !HasError();
        }

        public IQueryable<B_Device> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.B_Device.Where(d => ids.Contains(d.BDID));
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            if (activeFlag == false && !await ChangeActiveValidateReservation(id))
                return GetResponseJson();

            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        private async Task<bool> ChangeActiveValidateReservation(int id)
        {
            var cantDisableData = await DC.Resver_Head
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.Resver_Site)))
                .Where(ResverHeadExpression.IsOngoingExpression)
                .SelectMany(rh => rh.Resver_Site)
                .Where(rs => !rs.DeleteFlag)
                .SelectMany(rs => rs.Resver_Device)
                .Where(rd => !rd.DeleteFlag)
                .Where(rd => rd.BDID == id)
                .ToArrayAsync();

            foreach (Resver_Device resverDevice in cantDisableData)
            {
                AddError(UnsupportedValue(
                    $"欲停用的設備（ID {resverDevice.BDID} {resverDevice.B_Device.Code ?? ""}{resverDevice.B_Device.Title ?? ""}）",
                    $"已有進行中預約單（單號 {resverDevice.Resver_Site.RHID}）"));
            }

            return !HasError();
        }

        public IQueryable<B_Device> ChangeActiveQuery(int id)
        {
            return DC.B_Device.Where(d => d.BDID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Device_Submit_Input_APIItem.BDID))]
        public async Task<string> Submit(Device_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Device_Submit_Input_APIItem input)
        {
            return input.BDID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Device_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.BDID == 0, () => AddError(WrongFormat("設備 ID")))
                .Validate(i => i.Code.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("編碼", 0, 10)))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("標題")))
                .Validate(i => i.Title.HasLengthBetween(1, 60), () => AddError(LengthOutOfRange("標題", 1, 60)))
                .Validate(i => i.SupplierTitle.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("供應商名稱", 0, 100)))
                .Validate(i => i.SupplierTitle.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("供應商聯絡人", 0, 100)))
                .Validate(i => i.SupplierPhone.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("供應商服務電話", 0, 30)))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Device),
                    () => AddError(NotFound("類別 ID")))
                .ValidateAsync(async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Unit),
                    () => AddError(NotFound("單位 ID")))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Device),
                    () => AddError(NotFound("入帳代號 ID")))
                .ValidateAsync(async i => await DC.D_Hall.ValidateHallExists(i.DHID),
                    () => AddError(NotFound("廳別 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<B_Device> SubmitCreateData(Device_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new B_Device
            {
                BCID = input.BCID,
                BSCID = input.BSCID,
                BOCID = input.BOCID,
                DHID = input.DHID,
                Code = input.Code,
                Title = input.Title,
                UnitPrice = input.UnitPrice,
                InPrice = input.InPrice,
                OutPrice = input.OutPrice,
                SupplierTitle = input.SupplierTitle,
                SupplierName = input.SupplierName,
                SupplierPhone = input.SupplierPhone,
                Repair = input.Repair,
                Note = input.Note
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Device_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.BDID.IsAboveZero(), () => AddError(EmptyNotAllowed("設備 ID")))
                .Validate(i => i.Code.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("編碼", 0, 10)))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("標題")))
                .Validate(i => i.Title.HasLengthBetween(1, 60), () => AddError(LengthOutOfRange("標題", 1, 60)))
                .Validate(i => i.SupplierTitle.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("供應商名稱", 0, 100)))
                .Validate(i => i.SupplierTitle.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("供應商聯絡人", 0, 100)))
                .Validate(i => i.SupplierPhone.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("供應商服務電話", 0, 30)))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Device),
                    () => AddError(NotFound("類別 ID")))
                .ValidateAsync(async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Unit),
                    () => AddError(NotFound("單位 ID")))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Device),
                    () => AddError(NotFound("入帳代號 ID")))
                .ValidateAsync(async i => await DC.D_Hall.ValidateHallExists(i.DHID),
                    () => AddError(NotFound("廳別 ID")))
                .IsValid();

            if (!input.ActiveFlag)
                isValid = isValid && await ChangeActiveValidateReservation(input.BDID);

            return isValid;
        }

        public IQueryable<B_Device> SubmitEditQuery(Device_Submit_Input_APIItem input)
        {
            return DC.B_Device.Where(d => d.BDID == input.BDID);
        }

        public void SubmitEditUpdateDataFields(B_Device data, Device_Submit_Input_APIItem input)
        {
            data.BCID = input.BCID;
            data.BSCID = input.BSCID;
            data.BOCID = input.BOCID;
            data.DHID = input.DHID;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.UnitPrice = input.UnitPrice;
            data.InPrice = input.InPrice;
            data.OutPrice = input.OutPrice;
            data.SupplierTitle = input.SupplierTitle ?? data.SupplierTitle;
            data.SupplierName = input.SupplierName ?? data.SupplierName;
            data.SupplierPhone = input.SupplierPhone ?? data.SupplierPhone;
            data.Repair = input.Repair ?? data.Repair;
            data.Note = input.Note ?? data.Note;
        }

        #endregion

        #endregion
    }
}