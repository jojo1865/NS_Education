using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.SiteData.GetInfoById;
using NS_Education.Models.APIItems.Controller.SiteData.GetList;
using NS_Education.Models.APIItems.Controller.SiteData.Submit;
using NS_Education.Models.Entities;
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

            CommonResponseForPagedList<SiteData_GetList_Output_Row_APIItem> responseForPagedList =
                new CommonResponseForPagedList<SiteData_GetList_Output_Row_APIItem>
                {
                    Items = filteredResult
                        .Select(sd => Task.Run(() => GetListPagedEntityToRow(sd, skip++)).Result)
                        .SortWithInput(input.Sorting)
                        .Skip(skip)
                        .Take(take)
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
                    () => AddError(EmptyNotAllowed("分類 ID", nameof(input.BCID))))
                .ForceSkipIf(i => i.BCID <= 0)
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Site),
                    () => AddError(NotFound("分類 ID", nameof(input.BCID))))
                .StopForceSkipping()
                .Validate(i => i.BSCID1.IsZeroOrAbove(),
                    () => AddError(WrongFormat("樓層別", nameof(input.BSCID1))))
                .ForceSkipIf(i => i.BSCID1 <= 0)
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    () => AddError(NotFound("樓層別 ID", nameof(input.BSCID1))))
                .StopForceSkipping()
                .Validate(i => i.BasicCapacity.IsZeroOrAbove(),
                    () => AddError(WrongFormat("一般容納人數", nameof(input.BasicCapacity))))
                .Validate(i => i.MaxCapacity.IsZeroOrAbove(),
                    () => AddError(WrongFormat("最大容納人數", nameof(input.MaxCapacity))))
                .ForceSkipIf(i => i.TargetDate.IsNullOrWhiteSpace())
                .Validate(i => i.TargetDate.TryParseDateTime(out _),
                    () => AddError(WrongFormat("可租借日期", nameof(input.TargetDate))))
                .StopForceSkipping()
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetList_Input_APIItem input)
        {
            IQueryable<B_SiteData> query = DC.B_SiteData
                .Include(sd => sd.B_Category)
                .Include(sd => sd.Resver_Site)
                .Include(sd => sd.M_SiteGroup)
                .Include(sd =>
                    sd.M_SiteGroup.Select(asMaster => asMaster.B_SiteData1).Select(child => child.Resver_Site))
                .Include(sd => sd.M_SiteGroup.Select(msg => msg.B_SiteData1))
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

            if (input.BasicCapacity.IsAboveZero())
                query = query.Where(sd => sd.BasicSize >= input.BasicCapacity);

            if (input.MaxCapacity.IsAboveZero())
                query = query.Where(sd => sd.BasicSize >= input.MaxCapacity);

            if (input.IsCombinedSiteMaster.HasValue)
                query = query.Where(sd =>
                    sd.M_SiteGroup.Any(msg =>
                        msg.ActiveFlag && !msg.DeleteFlag && msg.B_SiteData1.ActiveFlag &&
                        !msg.B_SiteData1.DeleteFlag) == input.IsCombinedSiteMaster);

            if (input.ActiveFlag.IsInBetween(0, 1))
                query = query.Where(sd => sd.ActiveFlag == (input.ActiveFlag == 1));

            query = query.Where(sd => sd.DeleteFlag == (input.DeleteFlag == 1));

            return query.OrderBy(sd => sd.Code.Length)
                .ThenBy(sd => sd.Code)
                .ThenBy(sd => sd.BSID);
        }

        public async Task<SiteData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_SiteData entity, int index)
        {
            SiteData_GetList_Output_Row_APIItem output = new SiteData_GetList_Output_Row_APIItem
            {
                BSID = entity.BSID,
                IsCombinedSiteMaster = entity.M_SiteGroup.Any(msg =>
                    msg.ActiveFlag && !msg.DeleteFlag && msg.B_SiteData1.ActiveFlag && !msg.B_SiteData1.DeleteFlag),
                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                BasicSize = entity.BasicSize,
                MaxSize = entity.MaxSize,
                AreaSize = entity.AreaSize,
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
            await output.SetInfoFromEntity(entity, this);

            return output;
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
                .Include(sd => sd.M_SiteGroup1)
                .Include(sd => sd.M_SiteGroup1.Select(msg => msg.B_SiteData))
                .Where(sd => sd.BSID == id);
        }

        public async Task<SiteData_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_SiteData entity)
        {
            return new SiteData_GetInfoById_Output_APIItem
            {
                BSID = entity.BSID,
                IsCombinedSiteChild = entity.M_SiteGroup1.Any(msg =>
                    msg.ActiveFlag && !msg.DeleteFlag && msg.B_SiteData.ActiveFlag && !msg.B_SiteData.DeleteFlag),
                BCID = entity.BCID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                BasicSize = entity.BasicSize,
                MaxSize = entity.MaxSize,
                UnitPrice = entity.UnitPrice,
                AreaSize = entity.AreaSize,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                CubicleFlag = entity.CubicleFlag,
                PhoneExt1 = entity.PhoneExt1 ?? "",
                PhoneExt2 = entity.PhoneExt2 ?? "",
                PhoneExt3 = entity.PhoneExt3 ?? "",
                Note = entity.Note ?? "",
                BSCID1 = entity.BSCID1,
                FloorList = await DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.Floor,
                    entity.BSCID1),
                BSCID5 = entity.BSCID5,
                TableList = await DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.SiteTable,
                    entity.BSCID5),
                DHID = entity.DHID,
                HallList = await DC.D_Hall.GetHallSelectable(entity.DHID),
                BOCID = entity.BOCID,
                GroupList = entity.M_SiteGroup
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
                Devices = entity.GetDevicesFromSiteNotes()
                    .Devices
                    .Select(d => new SiteData_GetInfoById_Output_Device_Row_APIItem
                    {
                        BSID = entity.BSID,
                        BS_Code = entity.Code,
                        BS_Title = entity.Title,
                        DeviceName = d.DeviceName,
                        Ct = d.Count ?? 0
                    })
                    .ToList()
            };
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            // 若場地本身或其父場地有正在進行的預約，不予停用
            if (activeFlag == false && !await ChangeActiveValidateResverSite(id))
                return GetResponseJson();

            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        private async Task<bool> ChangeActiveValidateResverSite(int id)
        {
            Resver_Site[] ongoingResverSites = await DC.Resver_Head
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData.M_SiteGroup))
                .Where(ResverHeadExpression.IsOngoingExpression)
                .SelectMany(rh => rh.Resver_Site)
                .Where(rs => !rs.DeleteFlag)
                // 考慮父場地
                .Where(rs => rs.BSID == id ||
                             rs.B_SiteData.ActiveFlag && !rs.B_SiteData.DeleteFlag &&
                             rs.B_SiteData.M_SiteGroup.Any(msg => msg.GroupID == id))
                .Distinct()
                .ToArrayAsync();

            foreach (Resver_Site ongoing in ongoingResverSites)
            {
                AddError(ongoing.BSID == id
                    ? UnsupportedValue($"指定的場地 ID {id}", nameof(id), $"有進行中預約單（預約單號：{ongoing.RHID}）")
                    : UnsupportedValue($"指定的場地 ID {id}", nameof(id),
                        $"其上層場地（ID {ongoing.BSID} {ongoing.B_SiteData.Code ?? ""}{ongoing.B_SiteData.Title ?? ""}）有進行中預約單（預約單號：{ongoing.RHID}）"));
            }

            return !HasError();
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
            // 驗證刪除或復活時，是否會造成三層以上場地關係
            if (!await DeleteItemValidateParentAndChild(input))
                return GetResponseJson();

            // 若場地有正在進行的預約，不予刪除

            if (!await DeleteItemValidateResverSite(input))
                return GetResponseJson();

            return await _deleteItemHelper.DeleteItem(input);
        }

        private async Task<bool> DeleteItemValidateResverSite(DeleteItem_Input_APIItem input)
        {
            // 特例：需考慮上層場地，所以不使用 DeleteItemHelper 的驗證方法

            HashSet<int> uniqueDeleteId = input.GetUniqueDeleteId();

            // 須同時驗證這個場地的父場地的預約單
            Resver_Site[] cantDeleteData = await DC.Resver_Head
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData.M_SiteGroup1))
                .Where(ResverHeadExpression.IsOngoingExpression)
                .SelectMany(rh => rh.Resver_Site)
                .Where(rs => !rs.DeleteFlag)
                .Where(rs => uniqueDeleteId.Contains(rs.BSID) ||
                             // 找到這張場地預約單的 BS
                             // BS -> 作為父場地的 M_SiteGroup -> 任何包含在刪除項中的 GroupID
                             rs.B_SiteData.M_SiteGroup
                                 .Where(msg => msg.ActiveFlag && !msg.DeleteFlag)
                                 .Any(msg => uniqueDeleteId.Contains(msg.GroupID)))
                .ToArrayAsync();

            foreach (Resver_Site resverSite in cantDeleteData)
            {
                if (uniqueDeleteId.Contains(resverSite.BSID))
                    AddError(UnsupportedValue(
                        $"欲刪除的場地（ID {resverSite.BSID} {resverSite.B_SiteData.Code ?? ""}{resverSite.B_SiteData.Title ?? ""}）",
                        nameof(DeleteItem_Input_Row_APIItem.Id),
                        $"有進行中的預約（預約單號：{resverSite.RHID}）"));
                else
                    AddError(UnsupportedValue("欲刪除的場地",
                        nameof(DeleteItem_Input_Row_APIItem.Id),
                        $"其中一筆場地的上層場地（ID {resverSite.BSID} {resverSite.B_SiteData.Code ?? ""}{resverSite.B_SiteData.Title ?? ""}）有進行中的預約（預約單號：{resverSite.RHID}）"));
            }

            return !HasError();
        }

        private async Task<bool> DeleteItemValidateParentAndChild(DeleteItem_Input_APIItem input)
        {
            // 會造成三層以上場地關係的情況有四種
            // +-----+           +-----+          +-----+
            // |  A  |    --->   |  B  |   --->   |  C  |
            // +-----+           +-----+          +-----+
            // A: 成為父場地之父
            // B: (1) 帶父增子、 (2) 帶子增父
            // C: 成為子場地之子
            // 
            // A: 成為父場地之父
            // |- 刪除: 不會發生
            // +- 復活: 需確認原有的子場地是否已經當爸爸了
            //
            // B(1): 帶父增子
            // |- 刪除: 不會發生
            // +- 復活: 檢查是否同時有父場地和子場地指向自己
            //
            // B(2): 帶子增父
            // |- 刪除: 不會發生
            // +- 復活: 檢查是否同時有父場地和子場地指向自己 (B(1))
            //
            // C: 成為子場地之子
            // |- 刪除: 不會發生
            // +- 復活: 需確認原有的父場地是否已經是別人的兒子
            //
            // 所以需要檢查的為復活時，並且是以下其中一種情況：
            // |- a. 原有子場地是否已有子場地
            // |- b. 原有父場地是否已有父場地
            // +- c. 復活資料是否同時有子場地和父場地的關係資料

            HashSet<int> uniqueReviveIds = input.GetUniqueReviveId();

            HashSet<int> uniqueDeleteIds = input.GetUniqueDeleteId();

            // 找出復活對象的資料
            Dictionary<int, B_SiteData> dataToRevive = await DeleteItemGetReviveData(uniqueReviveIds);

            // 取得原有子場地與父場地，檢查狀態

            foreach (KeyValuePair<int, B_SiteData> kvp in dataToRevive)
            {
                // 檢查其本身的子場地與父場地
                // |- 如果子場地或父場地在 dataToRevive 的行列中，視為活著的資料做判斷...
                // +- 如果子場地或父場地在 dataToDelete 的行列中，從判斷中排除...

                // 原有子場地是否已有子場地
                DeleteItemCheckIfChildIsParentNow(kvp, uniqueDeleteIds, uniqueReviveIds);

                // 原有父場地是否已有父場地
                DeleteItemCheckIfParentIsChildNow(kvp, uniqueDeleteIds, uniqueReviveIds);

                // 復活資料本身，復活後是否同時會有活著的子場地和父場地
                DeleteItemCheckIfHasBothParentAndChild(kvp, uniqueDeleteIds, uniqueReviveIds);
            }

            return !HasError();
        }

        private void DeleteItemCheckIfHasBothParentAndChild(KeyValuePair<int, B_SiteData> kvp,
            HashSet<int> uniqueDeleteIds, HashSet<int> uniqueReviveIds)
        {
            if (kvp.Value.M_SiteGroup
                    .Where(msg => !uniqueDeleteIds.Contains(msg.GroupID) && msg.ActiveFlag && !msg.DeleteFlag)
                    .Any(msg => !msg.B_SiteData1.DeleteFlag || uniqueReviveIds.Contains(msg.B_SiteData1.BSID))
                &&
                kvp.Value.M_SiteGroup1
                    .Where(msg => !uniqueDeleteIds.Contains(msg.MasterID) && msg.ActiveFlag && !msg.DeleteFlag)
                    .Any(msg => !msg.B_SiteData.DeleteFlag || uniqueReviveIds.Contains(msg.B_SiteData.BSID)))
            {
                AddError(UnsupportedValue($"欲復活的場地 ID（{kvp.Key}）", nameof(DeleteItem_Input_Row_APIItem.Id),
                    "此場地復活後會同時有父場地和子場地"));
            }
        }

        private void DeleteItemCheckIfParentIsChildNow(KeyValuePair<int, B_SiteData> kvp, HashSet<int> uniqueDeleteIds,
            HashSet<int> uniqueReviveIds)
        {
            if (kvp.Value.M_SiteGroup1
                .Where(msg => !uniqueDeleteIds.Contains(msg.MasterID) && msg.ActiveFlag && !msg.DeleteFlag)
                .SelectMany(msg => msg.B_SiteData.M_SiteGroup1)
                .Where(parentMsg => parentMsg.ActiveFlag && !parentMsg.DeleteFlag)
                .Any(parentMsg =>
                    !parentMsg.B_SiteData.DeleteFlag || uniqueReviveIds.Contains(parentMsg.B_SiteData.BSID)))
            {
                AddError(UnsupportedValue($"欲復活的場地 ID（{kvp.Key}）", nameof(DeleteItem_Input_Row_APIItem.Id),
                    "此場地在刪除前的原有父場地，現已經是其他組合場地的子場地"));
            }
        }

        private void DeleteItemCheckIfChildIsParentNow(KeyValuePair<int, B_SiteData> kvp, HashSet<int> uniqueDeleteIds,
            HashSet<int> uniqueReviveIds)
        {
            if (kvp.Value.M_SiteGroup
                .Where(msg => !uniqueDeleteIds.Contains(msg.GroupID) && msg.ActiveFlag && !msg.DeleteFlag)
                .SelectMany(msg => msg.B_SiteData1.M_SiteGroup)
                .Where(childMsg => childMsg.ActiveFlag && !childMsg.DeleteFlag)
                .Any(childMsg =>
                    !childMsg.B_SiteData1.DeleteFlag || uniqueReviveIds.Contains(childMsg.B_SiteData1.BSID)))
            {
                AddError(UnsupportedValue($"欲復活的場地 ID（{kvp.Key}）", nameof(DeleteItem_Input_Row_APIItem.Id),
                    "此場地在刪除前的原有子場地，現已經是組合場地"));
            }
        }

        private async Task<Dictionary<int, B_SiteData>> DeleteItemGetReviveData(HashSet<int> uniqueReviveIds)
        {
            return await DC.B_SiteData
                .Include(sd => sd.M_SiteGroup)
                .Include(sd => sd.M_SiteGroup.Select(msg => msg.B_SiteData1))
                .Include(sd => sd.M_SiteGroup.Select(msg => msg.B_SiteData1.M_SiteGroup))
                .Include(sd =>
                    sd.M_SiteGroup.Select(msg => msg.B_SiteData1.M_SiteGroup.Select(childMsg => childMsg.B_SiteData1)))
                .Include(sd => sd.M_SiteGroup1)
                .Include(sd => sd.M_SiteGroup1.Select(msg => msg.B_SiteData))
                .Include(sd => sd.M_SiteGroup1.Select(msg => msg.B_SiteData.M_SiteGroup1))
                .Include(sd =>
                    sd.M_SiteGroup1.Select(msg =>
                        msg.B_SiteData.M_SiteGroup1.Select(parentMsg => parentMsg.B_SiteData)))
                .Where(sd => sd.DeleteFlag)
                .Where(sd => uniqueReviveIds.Contains(sd.BSID))
                .ToDictionaryAsync(sd => sd.BSID, sd => sd);
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
            // 篩掉子場地的輸入，避免誤傳誤改
            input.Devices = input.Devices.Where(d => d.IsImplicit != true).ToList();

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
                .Validate(i => i.BSID == 0, () => AddError(WrongFormat("場地 ID", nameof(input.BSID))))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Site),
                    () => AddError(NotFound("所屬分類 ID", nameof(input.BCID))))
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("編碼", nameof(input.Code), 0, 10)))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱", nameof(input.Title))))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    () => AddError(LengthOutOfRange("中文名稱", nameof(input.Title), 0, 60)))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數", nameof(input.BasicSize))))
                .Validate(i => i.AreaSize.IsAboveZero(),
                    () => AddError(OutOfRange("面積坪數", nameof(input.AreaSize), 1)))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用", nameof(input.UnitPrice))))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價", nameof(input.InPrice))))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價", nameof(input.OutPrice))))
                .Validate(i => i.PhoneExt1.HasLengthBetween(0, 6),
                    () => AddError(LengthOutOfRange("分機 1", nameof(input.PhoneExt1), 0, 6)))
                .Validate(i => i.PhoneExt2.HasLengthBetween(0, 6),
                    () => AddError(LengthOutOfRange("分機 2", nameof(input.PhoneExt2), 0, 6)))
                .Validate(i => i.PhoneExt3.HasLengthBetween(0, 6),
                    () => AddError(LengthOutOfRange("分機 3", nameof(input.PhoneExt3), 0, 6)))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    () => AddError(NotFound("樓別 ID", nameof(input.BSCID1))))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID5, StaticCodeType.SiteTable),
                    () => AddError(NotFound("桌型 ID", nameof(input.BSCID5))))
                .ValidateAsync(async i => await DC.D_Hall.ValidateHallExists(i.DHID),
                    () => AddError(NotFound("廳別 ID", nameof(input.DHID))))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Site),
                    () => AddError(NotFound("入帳代號 ID", nameof(input.BOCID))))
                .IsValid();

            bool isGroupListValid = await SubmitValidateGroupList(input);

            isValid = isValid && isGroupListValid;

            return isValid;
        }

        private async Task<bool> SubmitValidateGroupList(SiteData_Submit_Input_APIItem input)
        {
            // 驗證場地組合列表
            // |- a. 檢查是否與主場地 ID 相同
            // |- b. 檢查是否所有 id 都非重複
            // |- c. 檢查是否所有 id 都有有效資料
            // +- d. 檢查是否所有子場地都沒有下一層子場地

            // 主場地 ID 檢查
            if (input.GroupList.Any(gl => gl.BSID == input.BSID))
            {
                AddError(UnsupportedValue("子場地 ID", nameof(input.BSID), "不可將主場地設為自己的子場地"));
                return false;
            }

            // id 非重複
            HashSet<int> uniqueSiteIds = input.GroupList.Select(gl => gl.BSID).Distinct().ToHashSet();

            if (uniqueSiteIds.Count != input.GroupList.Count)
            {
                AddError(CopyNotAllowed("場地組合子場地 ID", nameof(SiteData_Submit_Input_GroupList_Row_APIItem.BSID)));
                return false;
            }

            // id 皆有資料
            Dictionary<int, B_SiteData> idToData = await DC.B_SiteData
                .Include(sd => sd.M_SiteGroup)
                .Where(sd => sd.ActiveFlag && !sd.DeleteFlag)
                .Where(sd => uniqueSiteIds.Contains(sd.BSID))
                .ToDictionaryAsync(sd => sd.BSID, sd => sd);

            bool allInputSiteExists = uniqueSiteIds.StartValidateElements()
                .Validate(id => idToData.ContainsKey(id),
                    id => AddError(NotFound($"場地組合子場地（ID {id}）",
                        nameof(SiteData_Submit_Input_GroupList_Row_APIItem.BSID))))
                .IsValid();

            if (!allInputSiteExists)
                return false;

            // 都不會造成三層以上場地關係

            // 會造成三層以上場地關係的情況有四種
            // +-----+           +-----+          +-----+
            // |  A  |    --->   |  B  |   --->   |  C  |
            // +-----+           +-----+          +-----+
            // A: 成為父場地之父
            // B: (1) 帶父增子、 (2) 帶子增父
            // C: 成為子場地之子

            // 因為 SiteData.Submit 只有「指定子場地」
            // 所以只需要處理情況 A 和 B(1)

            // A. 不允許輸入的子場地為任何人的父場地
            bool allInputSiteLeaf = idToData.Values.StartValidateElements()
                .Validate(sd => !sd.M_SiteGroup.Any(msg => msg.ActiveFlag && !msg.DeleteFlag),
                    sd => AddError(UnsupportedValue($"子場地（ID {sd.BSID}）", nameof(sd.BSID), "已為組合場地")))
                .IsValid();

            if (!allInputSiteLeaf)
                return false;

            // B. 不允許目前的場地已經是子場地了，還增加子場地
            bool isAGroup = await DC.M_SiteGroup
                .Include(msg => msg.B_SiteData)
                .AnyAsync(msg =>
                    msg.ActiveFlag && !msg.DeleteFlag && msg.GroupID == input.BSID && !msg.B_SiteData.DeleteFlag);

            if (isAGroup && input.GroupList.Any())
            {
                AddError(UnsupportedValue("場地 ID", nameof(input.BSID), "已為組合場地之子場地，不得設置子場地"));
                return false;
            }

            return true;
        }

        public async Task<B_SiteData> SubmitCreateData(SiteData_Submit_Input_APIItem input)
        {
            B_SiteData newEntry = new B_SiteData
            {
                BCID = input.BCID,
                Code = input.Code,
                Title = input.Title,
                BasicSize = input.BasicSize,
                MaxSize = input.BasicSize, // 最大可容納人數已取消，保留純為向後相容用
                AreaSize = input.AreaSize,
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
                M_SiteGroup = input.GroupList.Select((item, index) => new M_SiteGroup
                {
                    GroupID = item.BSID,
                    SortNo = index,
                    ActiveFlag = true
                }).ToArray()
            };

            SetSiteDataNoteFromSubmitInput(input, newEntry);

            return await Task.FromResult(newEntry);
        }

        private static void SetSiteDataNoteFromSubmitInput(SiteData_Submit_Input_APIItem input, B_SiteData siteData)
        {
            IEnumerable<SiteDeviceDto> newSiteDevices = input.Devices
                .Select(d => new SiteDeviceDto
                {
                    DeviceName = d.DeviceName,
                    Count = d.Ct
                });

            SiteDevicesDto dto = new SiteDevicesDto
            {
                Devices = newSiteDevices
            };

            siteData.SetDevicesToSiteNotes(dto);
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(SiteData_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.BSID.IsAboveZero(), () => AddError(EmptyNotAllowed("場地 ID", nameof(input.BSID))))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Site),
                    () => AddError(NotFound("所屬分類 ID", nameof(input.BCID))))
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("編碼", nameof(input.Code), 0, 10)))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱", nameof(input.Title))))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    () => AddError(LengthOutOfRange("中文名稱", nameof(input.Title), 0, 60)))
                .Validate(i => i.BasicSize >= 0, () => AddError(WrongFormat("一般容納人數", nameof(input.BasicSize))))
                .Validate(i => i.AreaSize.IsAboveZero(),
                    () => AddError(OutOfRange("面積坪數", nameof(input.AreaSize), 1)))
                .Validate(i => i.UnitPrice >= 0, () => AddError(WrongFormat("成本費用", nameof(input.UnitPrice))))
                .Validate(i => i.InPrice >= 0, () => AddError(WrongFormat("內部單位定價", nameof(input.InPrice))))
                .Validate(i => i.OutPrice >= 0, () => AddError(WrongFormat("外部單位定價", nameof(input.OutPrice))))
                .Validate(i => i.PhoneExt1.HasLengthBetween(0, 6),
                    () => AddError(LengthOutOfRange("分機 1", nameof(input.PhoneExt1), 0, 6)))
                .Validate(i => i.PhoneExt2.HasLengthBetween(0, 6),
                    () => AddError(LengthOutOfRange("分機 2", nameof(input.PhoneExt2), 0, 6)))
                .Validate(i => i.PhoneExt3.HasLengthBetween(0, 6),
                    () => AddError(LengthOutOfRange("分機 3", nameof(input.PhoneExt3), 0, 6)))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    () => AddError(NotFound("樓別 ID", nameof(input.BSCID1))))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID5, StaticCodeType.SiteTable),
                    () => AddError(NotFound("桌型 ID", nameof(input.BSCID5))))
                .ValidateAsync(async i => await DC.D_Hall.ValidateHallExists(i.DHID),
                    () => AddError(NotFound("廳別 ID", nameof(input.DHID))))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Site),
                    () => AddError(NotFound("入帳代號 ID", nameof(input.BOCID))))
                .IsValid();

            bool isReservationClear = input.ActiveFlag || isValid && await ChangeActiveValidateResverSite(input.BSID);

            isValid = isValid && isReservationClear;

            bool isGroupListValid = await SubmitValidateGroupList(input);

            isValid = isValid && isGroupListValid;

            if (!isValid)
                return false;

            // 判定新的 MaxSize 不會造成任何預約單人數溢出
            isValid = await SubmitEditValidatePeopleCtEnough(input);

            return isValid;
        }

        private async Task<bool> SubmitEditValidatePeopleCtEnough(SiteData_Submit_Input_APIItem input)
        {
            int neededSize = await DC.Resver_Site
                .Include(rs => rs.Resver_Head)
                .Where(rs => !rs.DeleteFlag)
                .Where(rs => rs.BSID == input.BSID)
                .Select(rs => rs.Resver_Head)
                .Where(ResverHeadExpression.IsOngoingExpression)
                .Select(rh => rh.PeopleCt)
                .OrderByDescending(ct => ct)
                .FirstOrDefaultAsync();

            if (input.BasicSize >= neededSize || input.MaxSize >= neededSize) return true;

            AddError(OutOfRange("容納人數", nameof(input.BasicSize) + ", " + nameof(input.MaxSize), neededSize));

            return false;
        }

        public IQueryable<B_SiteData> SubmitEditQuery(SiteData_Submit_Input_APIItem input)
        {
            return DC.B_SiteData
                .Include(sd => sd.M_SiteGroup)
                .Where(sd => sd.BSID == input.BSID);
        }

        public void SubmitEditUpdateDataFields(B_SiteData data, SiteData_Submit_Input_APIItem input)
        {
            // 先為每筆 input.GroupList 資料設 Index 值
            // 方便後面寫入 SortNo

            for (int i = 0; i < input.GroupList.Count; i++)
            {
                input.GroupList[i].Index = i;
            }

            // 1. 將所有 SiteGroup 的舊資料刪除

            // 先找出已存在關係資料的 BSID
            Dictionary<int, SiteData_Submit_Input_GroupList_Row_APIItem> inputBySiteIds =
                input.GroupList.ToDictionary(item => item.BSID, item => item);
            M_SiteGroup[] alreadyExistingInputs = data.M_SiteGroup
                .Where(sg => inputBySiteIds.ContainsKey(sg.GroupID))
                .ToArray();

            // 刪除所有 alreadyExistingInputs 以外的資料 - 這些舊資料沒有出現在新輸入中，表示要刪除
            DC.M_SiteGroup.RemoveRange(data.M_SiteGroup.Except(alreadyExistingInputs));

            IEnumerable<int> alreadyExistingInputsIds = alreadyExistingInputs.Select(aei => aei.GroupID);
            IEnumerable<SiteData_Submit_Input_GroupList_Row_APIItem> newSiteGroupInputs =
                input.GroupList.Where(item => !alreadyExistingInputsIds.Contains(item.BSID));

            // 2. 修改資料
            data.BCID = input.BCID;
            data.Code = input.Code;
            data.Title = input.Title;
            data.BasicSize = input.BasicSize;
            data.MaxSize = input.BasicSize; // 最大可容納人數已取消，保留純為向後相容用
            data.AreaSize = input.AreaSize;
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
            SetSiteDataNoteFromSubmitInput(input, data);

            // 把 M_SiteGroup 的舊資料改好
            foreach (M_SiteGroup siteGroup in data.M_SiteGroup)
            {
                if (!inputBySiteIds.ContainsKey(siteGroup.GroupID))
                    continue;

                SiteData_Submit_Input_GroupList_Row_APIItem item = inputBySiteIds[siteGroup.GroupID];

                siteGroup.SortNo = item.Index + 1;
                siteGroup.ActiveFlag = true;
                siteGroup.DeleteFlag = false;
            }

            // 加入 M_SiteGroup 的新資料
            data.M_SiteGroup = data.M_SiteGroup.Concat(newSiteGroupInputs
                    .Select(item => new M_SiteGroup
                    {
                        GroupID = item.BSID,
                        SortNo = item.Index + 1,
                        ActiveFlag = true
                    }))
                .ToArray();
        }

        #endregion

        #endregion
    }
}