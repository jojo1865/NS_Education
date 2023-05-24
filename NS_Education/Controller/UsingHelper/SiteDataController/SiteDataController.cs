using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.SiteData.GetInfoById;
using NS_Education.Models.APIItems.Controller.SiteData.GetList;
using NS_Education.Models.APIItems.Controller.SiteData.Submit;
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
            _getInfoByIdHelper =
                new GetInfoByIdHelper<SiteDataController, B_SiteData, SiteData_GetInfoById_Output_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<SiteDataController, B_SiteData>(this);
            _deleteItemHelper = new DeleteItemHelper<SiteDataController, B_SiteData>(this);
            _submitHelper = new SubmitHelper<SiteDataController, B_SiteData, SiteData_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(SiteData_GetList_Input_APIItem input)
        {
            return await _getListHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetList_Input_APIItem input)
        {
            bool isValid = input
                .StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(),
                    () => AddError(EmptyNotAllowed("分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetList_Input_APIItem input)
        {
            var query = DC.B_SiteData.Include(sd => sd.B_Category).AsQueryable();

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
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<B_SiteData> GetInfoByIdQuery(int id)
        {
            return DC.B_SiteData
                .Include(sd => sd.M_SiteGroup)
                .Include(sd => sd.M_SiteGroup.Select(sg => sg.B_SiteData1))
                .Where(sd => sd.BSID == id);
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
                FloorList = await DC.B_StaticCode.GetStaticCodeSelectable(1,
                    entity.BSCID1),
                BSCID5 = entity.BSCID5,
                TableList = await DC.B_StaticCode.GetStaticCodeSelectable(5,
                    entity.BSCID5),
                DHID = entity.DHID,
                HallList = await DC.D_Hall.GetHallSelectable(entity.DHID),
                BOCID = entity.BOCID,
                Items = entity.M_SiteGroup
                    .Where(siteGroup => siteGroup.ActiveFlag && !siteGroup.DeleteFlag)
                    .Select(siteGroup => new SiteData_GetInfoById_Output_GroupList_Row_APIItem
                    {
                        BSID = siteGroup.B_SiteData1?.BSID ?? 0,
                        Code = siteGroup.B_SiteData1?.Code ?? "",
                        Title = siteGroup.B_SiteData1?.Title ?? "",
                        SortNo = siteGroup.SortNo
                    })
                    .OrderBy(siteGroup => siteGroup.SortNo)
                    .ToList()
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

        public IQueryable<B_SiteData> ChangeActiveQuery(int id)
        {
            return DC.B_SiteData.Where(sd => sd.BSID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<B_SiteData> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.B_SiteData.Where(sd => ids.Contains(sd.BSID));
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
            bool isValid = await input.StartValidate()
                .Validate(i => i.BSID == 0, () => AddError(WrongFormat("場地 ID")))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Site),
                    () => AddError(NotFound("所屬分類 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數")))
                .Validate(i => i.MaxSize >= i.BasicSize, () => AddError("最大容納人數須大於等於一般容納人數！"))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用")))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價")))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    () => AddError(NotFound("樓別 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID5, StaticCodeType.SiteTable),
                    () => AddError(NotFound("桌型 ID")))
                .ValidateAsync(async i => await DC.D_Hall.ValidateHallExists(i.DHID), () => AddError(NotFound("廳別 ID")))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Site),
                    () => AddError(NotFound("入帳代號 ID")))
                .IsValid();

            return isValid;
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
                M_SiteGroup = input.GroupList.Select(item => new M_SiteGroup
                {
                    GroupID = item.BSID,
                    SortNo = item.SortNo,
                    ActiveFlag = true
                }).ToArray()
            };

            return await Task.FromResult(newEntry);
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(SiteData_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.BSID.IsAboveZero(), () => AddError(EmptyNotAllowed("場地 ID")))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Site),
                    () => AddError(NotFound("所屬分類 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數")))
                .Validate(i => i.MaxSize >= i.BasicSize, () => AddError("最大容納人數須大於等於一般容納人數！"))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用")))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價")))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    () => AddError(NotFound("樓別 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID5, StaticCodeType.SiteTable),
                    () => AddError(NotFound("桌型 ID")))
                .ValidateAsync(async i => await DC.D_Hall.ValidateHallExists(i.DHID), () => AddError(NotFound("廳別 ID")))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Site),
                    () => AddError(NotFound("入帳代號 ID")))
                .IsValid();

            return isValid;
        }

        public IQueryable<B_SiteData> SubmitEditQuery(SiteData_Submit_Input_APIItem input)
        {
            return DC.B_SiteData
                .Include(sd => sd.M_SiteGroup)
                .Where(sd => sd.BSID == input.BSID);
        }

        public void SubmitEditUpdateDataFields(B_SiteData data, SiteData_Submit_Input_APIItem input)
        {
            // 1. 將所有舊資料設為刪除狀態
            // TODO: 找出有效率方法，只處理真的需要新增或刪除的資料
            foreach (var oldData in data.M_SiteGroup)
            {
                oldData.ActiveFlag = false;
                oldData.DeleteFlag = true;
            }

            // 3. 修改資料
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
            data.M_SiteGroup = data.M_SiteGroup.Concat(input.GroupList
                .Select(item => new M_SiteGroup
                {
                    GroupID = item.BSID,
                    SortNo = item.SortNo,
                    ActiveFlag = true
                }))
                .ToArray();
        }

        #endregion

        #endregion
    }
}