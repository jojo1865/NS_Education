using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Device.GetInfoById;
using NS_Education.Models.APIItems.Device.GetList;
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
        IGetInfoById<B_Device, Device_GetInfoById_Output_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Device_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        
        public DeviceController()
        {
            _getListPagedHelper = new GetListPagedHelper<DeviceController, B_Device, Device_GetList_Input_APIItem, Device_GetList_Output_Row_APIItem>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<DeviceController, B_Device, Device_GetInfoById_Output_APIItem>(this);
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
                SupplierPhone = entity.SupplierPhone ??"",
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
            return DC.B_Device.Where(d => d.BSCID == id);
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
    }
}