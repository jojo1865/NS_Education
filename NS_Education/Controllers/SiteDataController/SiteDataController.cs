using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.SiteData.GetInfoById;
using NS_Education.Models.APIItems.SiteData.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers.SiteDataController
{
    public class SiteDataController : PublicClass,
        IGetListPaged<B_SiteData, SiteData_GetList_Input_APIItem, SiteData_GetList_Output_Row_APIItem>,
        IGetInfoById<B_SiteData, SiteData_GetInfoById_Output_APIItem>,
        IChangeActive<B_SiteData>
    {
        #region Initialization

        private readonly IGetListPagedHelper<SiteData_GetList_Input_APIItem> _getListHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        private readonly IChangeActiveHelper _changeActiveHelper;

        public SiteDataController()
        {
            _getListHelper = new GetListPagedHelper<SiteDataController, B_SiteData, SiteData_GetList_Input_APIItem,
                SiteData_GetList_Output_Row_APIItem>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<SiteDataController, B_SiteData, SiteData_GetInfoById_Output_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<SiteDataController, B_SiteData>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(SiteData_GetList_Input_APIItem input)
        {
            return await _getListHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetList_Input_APIItem input)
        {
            bool isValid = input
                .StartValidate()
                .Validate(i => i.BCID.IsValidIdOrZero(),
                    () => AddError(EmptyNotAllowed("分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetList_Input_APIItem input)
        {
            var query = DC.B_SiteData.Include(sd => sd.BC).AsQueryable();

            if (input.ActiveFlag != null)
                query = query.Where(sd => sd.ActiveFlag == input.ActiveFlag);

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(sd => sd.Title.Contains(input.Keyword) || sd.Code.Contains(input.Keyword));

            if (input.BCID > 0)
                query = query.Where(sd => sd.BCID == input.BCID);

            return query.OrderBy(sd => sd.BSID);
        }

        public async Task<SiteData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_SiteData entity)
        {
            return await Task.FromResult(new SiteData_GetList_Output_Row_APIItem
            {
                BSID = entity.BSID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC.TitleC ?? "",
                BC_TitleE = entity.BC.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                BasicSize = entity.BasicSize,
                MaxSize = entity.MaxSize,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                CubicleFlag = entity.CubicleFlag,
                PhoneExt1 = entity.PhoneExt1 ?? "",
                PhoneExt2 = entity.PhoneExt2 ?? "",
                PhoneExt3 = entity.PhoneExt3 ?? "",
                Note = entity.Note
            });
        }

        #endregion

        #region GetInfoById

        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public async Task<bool> GetInfoByIdValidateInput(int id)
        {
            bool isValid = id.StartValidate()
                .Validate(i => i.IsValidId()
                    , () => AddError(EmptyNotAllowed("場地 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_SiteData> GetInfoByIdQuery(int id)
        {
            return DC.B_SiteData.Where(sd => sd.BSID == id);
        }

        public async Task<SiteData_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_SiteData entity)
        {
            return new SiteData_GetInfoById_Output_APIItem
            {
                BSID = entity.BSID,
                BCID = entity.BCID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                BasicSize = entity.BasicSize,
                MaxSize = entity.MaxSize,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                CubicleFlag = entity.CubicleFlag,
                PhoneExt1 = entity.PhoneExt1 ?? "",
                PhoneExt2 = entity.PhoneExt2 ?? "",
                PhoneExt3 = entity.PhoneExt3 ?? "",
                Note = entity.Note ?? "",
                BSCID1 = entity.BSCID1,
                FloorList = await DC.B_StaticCode
                    .Where(sc => sc.ActiveFlag && !sc.DeleteFlag && sc.CodeType == 1)
                    .Select(sc => new BaseResponseRowForSelectable
                    {
                        ID = sc.BSCID,
                        Title = sc.Title ?? "",
                        SelectFlag = sc.BSCID == entity.BSCID1
                    })
                    .ToListAsync(),
                BSCID5 = entity.BSCID5,
                TableList = await DC.B_StaticCode
                    .Where(sc => sc.ActiveFlag && !sc.DeleteFlag && sc.CodeType == 5)
                    .Select(sc => new BaseResponseRowForSelectable
                    {
                        ID = sc.BSCID,
                        Title = sc.Title ?? "",
                        SelectFlag = sc.BSCID == entity.BSCID5
                    })
                    .ToListAsync(),
                DHID = entity.DHID,
                HallList = await DC.D_Hall
                    .Where(dh => dh.ActiveFlag && !dh.DeleteFlag)
                    .Select(dh => new BaseResponseRowForSelectable
                    {
                        ID = dh.DHID,
                        Title = dh.TitleC ?? "",
                        SelectFlag = dh.DHID == entity.DHID
                    })
                    .ToListAsync(),
                BOCID = entity.BOCID
            };
        }

        #endregion

        #region ChangeActive
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<B_SiteData> ChangeActiveQuery(int id)
        {
            return DC.B_SiteData.Where(sd => sd.BSID == id);
        }
        
        #endregion
    }
}