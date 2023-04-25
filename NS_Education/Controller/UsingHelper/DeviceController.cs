using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Device.GetInfoById;
using NS_Education.Models.APIItems.Device.GetList;
using NS_Education.Models.APIItems.Device.Submit;
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(Device_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Device_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之所屬分類 ID")))
                .Validate(i => i.DHID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之所屬廳別 ID")))
                .Validate(i => i.BOCID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之所屬入帳代號 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_Device> GetListPagedOrderedQuery(Device_GetList_Input_APIItem input)
        {
            var query = DC.B_Device
                .Include(d => d.BC)
                .Include(d => d.BSC)
                .Include(d => d.BOC)
                .Include(d => d.DH)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(d => d.Title.Contains(input.Keyword) || d.Code.Contains(input.Keyword));

            if (input.BCID.IsValidId())
                query = query.Where(d => d.BCID == input.BCID);

            if (input.DHID.IsValidId())
                query = query.Where(d => d.DHID == input.DHID);

            if (input.BOCID.IsValidId())
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
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                BOCID = entity.BOCID,
                BOC_Title = entity.BOC?.Title ?? "",
                DHID = entity.DHID,
                DH_Title = entity.DH?.TitleC ?? entity.DH?.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public async Task<bool> GetInfoByIdValidateInput(int id)
        {
            bool isValid = id.StartValidate()
                .Validate(i => i.IsValidId(), () => AddError(EmptyNotAllowed("設備 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_Device> GetInfoByIdQuery(int id)
        {
            return DC.B_Device
                .Include(d => d.BC)
                .Include(d => d.BSC)
                .Include(d => d.BOC)
                .Include(d => d.DH)
                .Where(d => d.BSCID == id);
        }

        public async Task<Device_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_Device entity)
        {
            return new Device_GetInfoById_Output_APIItem
            {
                BDID = entity.BDID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                BC_List = await DC.B_Category.GetCategorySelectable(entity.BC?.CategoryType, entity.BCID),
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSC?.CodeType, entity.BSCID),
                BOCID = entity.BOCID,
                BOC_Title = entity.BOC?.Title ?? "",
                BOC_List = await DC.B_OrderCode.GetOrderCodeSelectable(entity.BOC?.CodeType, entity.BOCID),
                DHID = entity.DHID,
                DH_Title = entity.DH?.TitleC ?? entity.DH?.TitleE ?? "",
                DH_List = await DC.D_Hall.GetHallSelectable(entity.DHID),
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<B_Device> DeleteItemQuery(int id)
        {
            return DC.B_Device.Where(d => d.BDID == id);
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<B_Device> ChangeActiveQuery(int id)
        {
            return DC.B_Device.Where(d => d.BDID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
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
            bool isValid = input.StartValidate()
                .Validate(i => i.BDID == 0, () => AddError(WrongFormat("設備 ID")))
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("類別 ID")))
                .Validate(i => i.BSCID.IsValidId(), () => AddError(EmptyNotAllowed("單位 ID")))
                .Validate(i => i.BOCID.IsValidId(), () => AddError(EmptyNotAllowed("入帳代號 ID")))
                .Validate(i => i.DHID.IsValidId(), () => AddError(EmptyNotAllowed("廳別 ID")))
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
                Ct = input.Ct,
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
            bool isValid = input.StartValidate()
                .Validate(i => i.BDID.IsValidId(), () => AddError(EmptyNotAllowed("設備 ID")))
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("類別 ID")))
                .Validate(i => i.BSCID.IsValidId(), () => AddError(EmptyNotAllowed("單位 ID")))
                .Validate(i => i.BOCID.IsValidId(), () => AddError(EmptyNotAllowed("入帳代號 ID")))
                .Validate(i => i.DHID.IsValidId(), () => AddError(EmptyNotAllowed("廳別 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_Device> SubmitEditQuery(Device_Submit_Input_APIItem input)
        {
            return DC.B_Device.Where(d => d.BCID == input.BCID);
        }

        public void SubmitEditUpdateDataFields(B_Device data, Device_Submit_Input_APIItem input)
        {
            data.BCID = input.BCID;
            data.BSCID = input.BSCID;
            data.BOCID = input.BOCID;
            data.DHID = input.DHID;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.Ct = input.Ct;
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