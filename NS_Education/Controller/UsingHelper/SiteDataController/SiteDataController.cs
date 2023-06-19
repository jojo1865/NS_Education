using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
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
        IGetInfoById<B_SiteData, SiteData_GetInfoById_Output_APIItem>,
        IChangeActive<B_SiteData>,
        IDeleteItem<B_SiteData>,
        ISubmit<B_SiteData, SiteData_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<SiteData_Submit_Input_APIItem> _submitHelper;

        public SiteDataController()
        {
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
            // 因為要透過時段檢查可租借日期，無法使用 helper

            // 1. 輸入驗證
            if (!await GetListPagedValidateInput(input))
                return GetResponseJson();

            // 2. 篩選資料並查詢（不包括 TargetDate 的處理）
            B_SiteData[] queryResult = await GetListPagedOrderedQuery(input).ToArrayAsync();

            // 3. 依據 TargetDate 做篩選
            B_SiteData[] filteredResult = (await GetListFilterByTargetDate(input.TargetDate, queryResult)).ToArray();

            (int skip, int take) = input.CalculateSkipAndTake(filteredResult.Length);

            BaseResponseForPagedList<SiteData_GetList_Output_Row_APIItem> responseForPagedList =
                new BaseResponseForPagedList<SiteData_GetList_Output_Row_APIItem>
                {
                    Items = filteredResult
                        .Skip(skip)
                        .Take(take)
                        .Select(sd => Task.Run(() => GetListPagedEntityToRow(sd, skip++)).Result)
                        .ToArray(),
                    NowPage = input.NowPage,
                    CutPage = input.CutPage,
                    AllItemCt = filteredResult.Length
                };

            if (input.ReverseOrder)
                responseForPagedList.Items = responseForPagedList.Items.Reverse().ToArray();

            return GetResponseJson(responseForPagedList);
        }

        private async Task<IEnumerable<B_SiteData>> GetListFilterByTargetDate(string inputTargetDate,
            B_SiteData[] queryResult)
        {
            if (!inputTargetDate.TryParseDateTime(out DateTime targetDate))
                return queryResult;

            targetDate = targetDate.Date;
            IList<B_SiteData> result = new List<B_SiteData>();

            string resverSiteTableName = DC.GetTableName<Resver_Site>();

            // 查詢所有 DTS
            D_TimeSpan[] allDts = await DC.D_TimeSpan
                .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                .ToArrayAsync();

            foreach (B_SiteData data in queryResult)
            {
                // 取得指定日期當天所有跟 data 有關的 Resver_Site
                // 包含父場地和子場地
                Resver_Site[] sameDayResverSites = data
                    .Resver_Site
                    .Concat(data.M_SiteGroup
                        .Where(sg => sg.ActiveFlag && !sg.DeleteFlag)
                        .Select(asMaster => asMaster.B_SiteData1)
                        .SelectMany(child => child.Resver_Site))
                    .Concat(data.M_SiteGroup1
                        .Where(sg => sg.ActiveFlag && !sg.DeleteFlag)
                        .Select(asChild => asChild.B_SiteData)
                        .SelectMany(master => master.Resver_Site))
                    .Where(rs => !rs.DeleteFlag)
                    .Where(rs => rs.TargetDate.Date == targetDate)
                    .ToArray();

                // 如果沒有 resver site, 則已確認可用
                if (sameDayResverSites.Length == 0)
                {
                    result.Add(data);
                    continue;
                }

                // 查詢所有跟 Resver_Site 有關的 Resver_TimeSpan
                IEnumerable<int> resverSiteIds = sameDayResverSites.Select(rs => rs.RSID);

                D_TimeSpan[] resverTimeSpans = await DC.M_Resver_TimeSpan
                        .Include(rts => rts.D_TimeSpan)
                        .Where(rts => rts.TargetTable == resverSiteTableName)
                        .Where(rts => resverSiteIds.Contains(rts.TargetID))
                        .Select(rts => rts.D_TimeSpan)
                        .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                        .ToArrayAsync()
                    ;

                // 只要有任一 DTS 沒有被占用，就 Add
                if (!resverTimeSpans.All(rts => allDts.Any(dts => dts.IsCrossingWith(rts))))
                    result.Add(data);
            }

            return result;
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetList_Input_APIItem input)
        {
            bool isValid = await input
                .StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(),
                    () => AddError(EmptyNotAllowed("分類 ID")))
                .ForceSkipIf(i => i.BCID <= 0)
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Site),
                    () => AddError(NotFound("分類 ID")))
                .StopForceSkipping()
                .Validate(i => i.BSCID1.IsZeroOrAbove(),
                    () => AddError(WrongFormat("樓層別")))
                .ForceSkipIf(i => i.BSCID1 <= 0)
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    () => AddError(NotFound("樓層別 ID")))
                .StopForceSkipping()
                .Validate(i => i.Capacity.IsZeroOrAbove(),
                    () => AddError(WrongFormat("容納人數")))
                .ForceSkipIf(i => i.TargetDate.IsNullOrWhiteSpace())
                .Validate(i => i.TargetDate.TryParseDateTime(out _),
                    () => AddError(WrongFormat("可租借日期")))
                .StopForceSkipping()
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetList_Input_APIItem input)
        {
            var query = DC.B_SiteData
                .Include(sd => sd.B_Category)
                .Include(sd => sd.Resver_Site)
                .Include(sd => sd.M_SiteGroup)
                .Include(sd =>
                    sd.M_SiteGroup.Select(asMaster => asMaster.B_SiteData1).Select(child => child.Resver_Site))
                .Include(sd => sd.M_SiteGroup1)
                .Include(sd =>
                    sd.M_SiteGroup1.Select(asChild => asChild.B_SiteData).Select(master => master.Resver_Site))
                .AsQueryable();

            if (input.Keyword.HasContent())
                query = query.Where(sd => sd.Title.Contains(input.Keyword) || sd.Code.Contains(input.Keyword));

            if (input.BCID.IsAboveZero())
                query = query.Where(sd => sd.BCID == input.BCID);

            if (input.BSCID1.IsAboveZero())
                query = query.Where(sd => sd.BSCID1 == input.BSCID1);

            if (input.Capacity.IsAboveZero())
                query = query.Where(sd => sd.MaxSize >= input.Capacity);

            if (input.ActiveFlag.IsInBetween(0, 1))
                query = query.Where(sd => sd.ActiveFlag == (input.ActiveFlag == 1));

            query = query.Where(sd => sd.DeleteFlag == (input.DeleteFlag == 1));

            return query.OrderBy(sd => sd.Code.Length)
                .ThenBy(sd => sd.Code)
                .ThenBy(sd => sd.BSID);
        }

        public Task<SiteData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_SiteData entity, int index)
        {
            SiteData_GetList_Output_Row_APIItem output = new SiteData_GetList_Output_Row_APIItem
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
            };

            output.SetIndex(index);

            return Task.FromResult(output);
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
                .Include(sd => sd.M_SiteGroup.Select(sg => sg.B_SiteData1).Select(child => child.M_Site_Device))
                .Include(sd => sd.M_Site_Device)
                .Include(sd => sd.M_Site_Device.Select(msd => msd.B_Device))
                .Include(sd => sd.M_Site_Device.Select(msd => msd.B_SiteData))
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
                    .ToList(),
                Devices = entity.M_Site_Device
                    .Concat(entity.M_SiteGroup
                        .Select(msg => msg.B_SiteData1)
                        .Where(sd => sd.ActiveFlag && !sd.DeleteFlag)
                        .SelectMany(sd => sd.M_Site_Device))
                    .Where(msd => msd.B_SiteData.ActiveFlag && !msd.B_SiteData.DeleteFlag)
                    .Where(msd => msd.B_Device.ActiveFlag && !msd.B_Device.DeleteFlag)
                    .Select(msd => new SiteData_GetInfoById_Output_Device_Row_APIItem
                    {
                        BDID = msd.BDID,
                        BSID = msd.BSID,
                        BD_Code = msd.B_Device?.Code ?? "",
                        BD_Title = msd.B_Device?.Title ?? "",
                        BS_Code = msd.B_SiteData?.Code ?? "",
                        BS_Title = msd.B_SiteData?.Title ?? "",
                        Ct = msd.Ct,
                        IsImplicit = msd.BSID != entity.BSID
                    }).ToList(),
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
                .Validate(i => i.Code.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("編碼", 0, 10)))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.Title.HasLengthBetween(1, 60), () => AddError(LengthOutOfRange("中文名稱", 0, 60)))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數")))
                .Validate(i => i.MaxSize >= i.BasicSize, () => AddError("最大容納人數須大於等於一般容納人數！"))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用")))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價")))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價")))
                .Validate(i => i.PhoneExt1.HasLengthBetween(0, 6), () => AddError(LengthOutOfRange("分機 1", 0, 6)))
                .Validate(i => i.PhoneExt2.HasLengthBetween(0, 6), () => AddError(LengthOutOfRange("分機 2", 0, 6)))
                .Validate(i => i.PhoneExt3.HasLengthBetween(0, 6), () => AddError(LengthOutOfRange("分機 3", 0, 6)))
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
                .Validate(i => i.Code.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("編碼", 0, 10)))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.Title.HasLengthBetween(1, 60), () => AddError(LengthOutOfRange("中文名稱", 0, 60)))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數")))
                .Validate(i => i.MaxSize >= i.BasicSize, () => AddError("最大容納人數須大於等於一般容納人數！"))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用")))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價")))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價")))
                .Validate(i => i.PhoneExt1.HasLengthBetween(0, 6), () => AddError(LengthOutOfRange("分機 1", 0, 6)))
                .Validate(i => i.PhoneExt2.HasLengthBetween(0, 6), () => AddError(LengthOutOfRange("分機 2", 0, 6)))
                .Validate(i => i.PhoneExt3.HasLengthBetween(0, 6), () => AddError(LengthOutOfRange("分機 3", 0, 6)))
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
            // 1. 將所有舊資料刪除

            // 先找出已存在關係資料的 BSID
            var inputBySiteIds = input.GroupList.ToDictionary(item => item.BSID, item => item);
            var alreadyExistingInputs = data.M_SiteGroup
                .Where(sg => inputBySiteIds.ContainsKey(sg.GroupID))
                .ToArray();

            // 刪除所有 alreadyExistingInputs 以外的資料 - 這些舊資料沒有出現在新輸入中，表示要刪除
            DC.M_SiteGroup.RemoveRange(data.M_SiteGroup.Except(alreadyExistingInputs));

            var alreadyExistingInputsIds = alreadyExistingInputs.Select(aei => aei.GroupID);
            var newInputs = input.GroupList.Where(item => !alreadyExistingInputsIds.Contains(item.BSID));

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

            // 把舊資料改好
            foreach (M_SiteGroup siteGroup in data.M_SiteGroup)
            {
                if (!inputBySiteIds.ContainsKey(siteGroup.GroupID))
                    continue;

                SiteData_Submit_Input_GroupList_Row_APIItem item = inputBySiteIds[siteGroup.GroupID];

                siteGroup.SortNo = item.SortNo;
                siteGroup.ActiveFlag = true;
                siteGroup.DeleteFlag = false;
            }

            // 加入新資料
            data.M_SiteGroup = data.M_SiteGroup.Concat(newInputs
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