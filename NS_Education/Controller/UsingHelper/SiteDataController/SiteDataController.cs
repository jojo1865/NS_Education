using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.SiteData.GetInfoById;
using NS_Education.Models.APIItems.SiteData.GetList;
using NS_Education.Models.APIItems.SiteData.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using WebGrease.Css.Extensions;

namespace NS_Education.Controller.UsingHelper.SiteDataController
{
    public class SiteDataController : PublicClass,
        IGetListPaged<B_SiteData, SiteData_GetList_Input_APIItem, SiteData_GetList_Output_Row_APIItem>,
        IGetInfoById<B_SiteData, SiteData_GetInfoById_Output_APIItem>,
        IChangeActive<B_SiteData>,
        IDeleteItem<B_SiteData>,
        ISubmit<B_SiteData, SiteData_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<SiteData_GetList_Input_APIItem> _getListHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<SiteData_Submit_Input_APIItem> _submitHelper;

        public SiteDataController()
        {
            _getListHelper = new GetListPagedHelper<SiteDataController, B_SiteData, SiteData_GetList_Input_APIItem,
                SiteData_GetList_Output_Row_APIItem>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<SiteDataController, B_SiteData, SiteData_GetInfoById_Output_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<SiteDataController, B_SiteData>(this);
            _deleteItemHelper = new DeleteItemHelper<SiteDataController, B_SiteData>(this);
            _submitHelper = new SubmitHelper<SiteDataController, B_SiteData, SiteData_Submit_Input_APIItem>(this);
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

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
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
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<B_SiteData> ChangeActiveQuery(int id)
        {
            return DC.B_SiteData.Where(sd => sd.BSID == id);
        }
        
        #endregion

        #region DeleteItem
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<B_SiteData> DeleteItemQuery(int id)
        {
            return DC.B_SiteData.Where(sd => sd.BSID == id);
        }
        
        #endregion

        #region Submit
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(SiteData_Submit_Input_APIItem.BSID))]
        public async Task<string> Submit(SiteData_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(SiteData_Submit_Input_APIItem input)
        {
            return input.BSID == 0;
        }
        
        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(SiteData_Submit_Input_APIItem input)
        {
            return await Task.FromResult(input.StartValidate()
                .Validate(i => i.BSID == 0, () => AddError("場地 ID 只允許為 0！"))
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("類別 ID")))
                .Validate(i => !i.Code.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("編碼")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數")))
                .Validate(i => i.MaxSize >= i.BasicSize, () => AddError("最大容納人數須大於等於一般容納人數！"))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用")))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價")))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價")))
                .Validate(i => i.BSCID1.IsValidId(), () => AddError(EmptyNotAllowed("樓別 ID")))
                .Validate(i => i.BSCID5.IsValidId(), () => AddError(EmptyNotAllowed("桌型 ID")))
                .Validate(i => i.DHID.IsValidId(), () => AddError(EmptyNotAllowed("廳別 ID")))
                .Validate(i => i.BOCID.IsValidId(), () => AddError(EmptyNotAllowed("入帳代號 ID")))
                .IsValid());
        }

        public async Task<B_SiteData> SubmitCreateData(SiteData_Submit_Input_APIItem input)
        {
            B_SiteData newEntry = new B_SiteData
            {
                BCID = input.BCID,
                Code = input.Code,
                Title = input.Title,
                BasicSize = input.BasicSize,
                MaxSize = input.MaxSize,
                UnitPrice = input.UnitPrice,
                InPrice = input.InPrice,
                OutPrice = input.OutPrice,
                CubicleFlag = input.CubicleFlag,
                BSCID1 = input.BSCID1,
                BSCID5 = input.BSCID5,
                DHID = input.DHID,
                BOCID = input.BOCID,
                PhoneExt1 = input.PhoneExt1,
                PhoneExt2 = input.PhoneExt2,
                PhoneExt3 = input.PhoneExt3,
                Note = input.Note,
                ActiveFlag = input.ActiveFlag,
                M_SiteGroupGroup = input.GroupList.Select(sg => new M_SiteGroup
                {
                    GroupID = sg.BSID,
                    SortNo = sg.SortNo,
                    ActiveFlag = true,
                    DeleteFlag = false,
                    CreDate = DateTime.Now,
                    CreUID = GetUid(),
                    UpdDate = DateTime.Now,
                    UpdUID = 0
                }).ToArray()
            };

            return await Task.FromResult(newEntry);
        }
        #endregion

        #region Submit - Edit
        
        public async Task<bool> SubmitEditValidateInput(SiteData_Submit_Input_APIItem input)
        {
            return await Task.FromResult(input.StartValidate()
                .Validate(i => i.BSID.IsValidId(), () => AddError(WrongFormat("場地 ID")))
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("類別 ID")))
                .Validate(i => !i.Code.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("編碼")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數")))
                .Validate(i => i.MaxSize >= i.BasicSize, () => AddError("最大容納人數須大於等於一般容納人數！"))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用")))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價")))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價")))
                .Validate(i => i.BSCID1.IsValidId(), () => AddError(EmptyNotAllowed("樓別 ID")))
                .Validate(i => i.BSCID5.IsValidId(), () => AddError(EmptyNotAllowed("桌型 ID")))
                .Validate(i => i.DHID.IsValidId(), () => AddError(EmptyNotAllowed("廳別 ID")))
                .Validate(i => i.BOCID.IsValidId(), () => AddError(EmptyNotAllowed("入帳代號 ID")))
                .IsValid());
        }

        public IQueryable<B_SiteData> SubmitEditQuery(SiteData_Submit_Input_APIItem input)
        {
            return DC.B_SiteData.Where(sd => sd.BSID == input.BSID);
        }

        public void SubmitEditUpdateDataFields(B_SiteData data, SiteData_Submit_Input_APIItem input)
        {
            // 1. 刪除這個場地原本有的所有組合
            data.M_SiteGroupGroup.ForEach(sg =>
            {
                sg.ActiveFlag = false;
                sg.DeleteFlag = true;
            });
            // 2. 修改資料
            data.BCID = input.BCID;
            data.Code = input.Code;
            data.Title = input.Title;
            data.BasicSize = input.BasicSize;
            data.MaxSize = input.MaxSize;
            data.UnitPrice = input.UnitPrice;
            data.InPrice = input.InPrice;
            data.OutPrice = input.OutPrice;
            data.CubicleFlag = input.CubicleFlag;
            data.BSCID1 = input.BSCID1;
            data.BSCID5 = input.BSCID5;
            data.DHID = input.DHID;
            data.BOCID = input.BOCID;
            data.PhoneExt1 = input.PhoneExt1;
            data.PhoneExt2 = input.PhoneExt2;
            data.PhoneExt3 = input.PhoneExt3;
            data.Note = input.Note;
            data.ActiveFlag = input.ActiveFlag;
            data.M_SiteGroupGroup = input.GroupList.Select(sg => new M_SiteGroup
            {
                GroupID = sg.BSID,
                SortNo = sg.SortNo,
                ActiveFlag = true,
                DeleteFlag = false,
                CreDate = DateTime.Now,
                CreUID = GetUid(),
                UpdDate = DateTime.Now,
                UpdUID = 0
            }).ToArray();
        }
        
        #endregion
        
        #endregion
    }
}