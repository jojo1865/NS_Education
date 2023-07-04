using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.SiteData.GetListForCalendar;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.SiteDataController
{
    /// <summary>
    /// 處理行事曆用的取得場地列表的 API。<br/>
    /// 雖然是場地列表，但實際上查詢主體是「預約單」而非「場地」。<br/>
    /// 但因為目前開的 Route 為 SiteData，因此還是歸類在 SiteDataController，<br/>
    /// 但處理的是預約單的 Entity。
    /// </summary>
    public class SiteDataCalendarController : PublicClass
    {
        #region GetList - For calendar

        // Route 為 /SiteData/GetCalendarList
        // 詳見 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(SiteData_GetListForCalendar_Input_APIItem input)
        {
            // 特例：最終輸出資料不以單筆資料為單位，而是重新 group 過，因此不使用 Helper

            if (!await GetListPagedValidateInput(input))
                return GetResponseJson();

            Resver_Site[] results = await GetListPagedOrderedQuery(input).ToArrayAsync();

            CommonResponseForList<SiteData_GetListForCalendar_Output_Row_APIItem> response =
                await GetListResponse(input, results);

            return GetResponseJson(response);
        }

        private async Task<CommonResponseForList<SiteData_GetListForCalendar_Output_Row_APIItem>> GetListResponse(
            SiteData_GetListForCalendar_Input_APIItem input,
            Resver_Site[] results)
        {
            // 查出所有有效的時段資料
            var timeSpans = await DC.D_TimeSpan
                .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                .ToArrayAsync();

            // 建立 RSID -> RS 的字典，方便在建立 DTSID -> RS 對照表時快速對照
            Dictionary<int, Resver_Site> rsIdToRs = results
                .ToDictionary(rs => rs.RSID, rs => rs);

            // 查詢 dtsToRts 的先備參數
            IEnumerable<int> resultIds = results.Select(rs => rs.RSID);
            string tableName = DC.GetTableName<Resver_Site>();

            // 查詢所有有關的 RTS
            M_Resver_TimeSpan[] relatedRts = await DC.M_Resver_TimeSpan
                .Include(rts => rts.D_TimeSpan)
                .Where(rts => rts.TargetTable == tableName)
                .Where(rts => resultIds.Contains(rts.TargetID))
                .ToArrayAsync();

            // 建立每個 DTSID 與哪些 RS 有關的對照辭典
            IDictionary<int, IEnumerable<Resver_Site>> dtsIdToRs = timeSpans
                .ToDictionary(dts => dts.DTSID, dts => relatedRts
                    .Where(rts => rts.D_TimeSpan.IsCrossingWith(dts))
                    .Select(rts => rsIdToRs[rts.TargetID]));

            CommonResponseForList<SiteData_GetListForCalendar_Output_Row_APIItem>
                response = GetListInitializeResponse();

            // 因為回傳結果是 startDate ~ endDate 每天一個 row，所以不能從不一定每天都有的 resverSites.TargetDate 來建立

            DateTime startDate = input.StartDate.ParseDateTime().Date;
            DateTime endDate = input.EndDate.ParseDateTime().Date;

            for (DateTime d = startDate; d <= endDate; d = d.AddDays(1))
            {
                SiteData_GetListForCalendar_Output_Row_APIItem row = GetListMakeNewRow(d);
                List<SiteData_GetListForCalendar_TimeSpan_APIItem> timeSpanItems =
                    GetListMakeRowTimeSpanItems(d, timeSpans, dtsIdToRs);

                row.TimeSpans = timeSpanItems;
                response.Items.Add(row);
            }

            return response;
        }

        private static CommonResponseForList<SiteData_GetListForCalendar_Output_Row_APIItem> GetListInitializeResponse()
        {
            return new CommonResponseForList<SiteData_GetListForCalendar_Output_Row_APIItem>
            {
                Items = new List<SiteData_GetListForCalendar_Output_Row_APIItem>()
            };
        }

        private List<SiteData_GetListForCalendar_TimeSpan_APIItem> GetListMakeRowTimeSpanItems(DateTime d,
            D_TimeSpan[] timeSpans,
            IDictionary<int, IEnumerable<Resver_Site>> dtsToRs)
        {
            List<SiteData_GetListForCalendar_TimeSpan_APIItem> timeSpanItems = timeSpans
                .Select((dts, tsIndex) => new SiteData_GetListForCalendar_TimeSpan_APIItem
                {
                    Title = dts.Title ?? "",
                    SortNo = tsIndex,
                    ReservedSites = !dtsToRs.ContainsKey(dts.DTSID)
                        ? new List<SiteData_GetListForCalendar_ReservedSite_APIItem>()
                        : dtsToRs[dts.DTSID]
                            .Where(rs => rs.TargetDate.Date == d.Date)
                            .Select((rs, rsIndex) => new SiteData_GetListForCalendar_ReservedSite_APIItem
                            {
                                BS_Code = rs.B_SiteData?.Code ?? "",
                                BS_Title = rs.B_SiteData?.Title ?? "",
                                RHID = rs.RHID,
                                RH_Code = rs.Resver_Head?.Code ?? "",
                                RH_Title = rs.Resver_Head?.Title ?? "",
                                SortNo = rsIndex
                            }).OrderBy(i => i.SortNo).ToList()
                }).OrderBy(i => i.SortNo).ToList();
            return timeSpanItems;
        }

        private static SiteData_GetListForCalendar_Output_Row_APIItem GetListMakeNewRow(DateTime d)
        {
            SiteData_GetListForCalendar_Output_Row_APIItem row = new SiteData_GetListForCalendar_Output_Row_APIItem
            {
                Date = d.ToFormattedStringDate(),
                Weekday = (int)d.DayOfWeek == 0 ? 7 : (int)d.DayOfWeek,
            };
            return row;
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetListForCalendar_Input_APIItem input)
        {
            DateTime startDate = default;
            DateTime endDate = default;

            bool isValid = await input.StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(i => i.StartDate.TryParseDateTime(out startDate),
                    () => AddError(WrongFormat("起始日期", nameof(input.StartDate))))
                .Validate(i => i.EndDate.TryParseDateTime(out endDate),
                    () => AddError(WrongFormat("結束日期", nameof(input.EndDate))))
                .Validate(i => endDate >= startDate,
                    () => AddError(MinLargerThanMax("起始日期", nameof(input.StartDate), "結束日期", nameof(input.EndDate))))
                .Validate(i => ((endDate.Date - startDate.Date).Days + 1).IsInBetween(1, 21),
                    () => AddError(OutOfRange("查詢的日期區間天數", nameof(input.StartDate) + ", " + nameof(input.EndDate), 1,
                        21)))
                .Validate(i => i.BSID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之場地 ID", nameof(input.BSID))))
                .Validate(i => i.RHID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之預約單 ID", nameof(input.RHID))))
                .SkipIfAlreadyInvalid()
                .ForceSkipIf(i => i.BSID <= 0)
                .ValidateAsync(async i => await DC.B_SiteData.ValidateIdExists(i.BSID, nameof(B_SiteData.BSID)),
                    () => AddError(NotFound("欲篩選之場地 ID", nameof(input.BSID))))
                .StopForceSkipping()
                .ForceSkipIf(i => i.RHID <= 0)
                .ValidateAsync(async i => await DC.Resver_Head.ValidateIdExists(i.RHID, nameof(Resver_Head.RHID)),
                    () => AddError(NotFound("欲篩選之預約單 ID", nameof(input.RHID))))
                .StopForceSkipping()
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Site> GetListPagedOrderedQuery(SiteData_GetListForCalendar_Input_APIItem input)
        {
            var query = DC.Resver_Site
                .Include(rs => rs.Resver_Head)
                .Include(rs => rs.Resver_Head.Customer)
                .Include(rs => rs.Resver_Head.M_Resver_TimeSpan)
                .Include(rs => rs.Resver_Head.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Include(rs => rs.B_SiteData)
                .Include(rs => rs.B_SiteData.M_SiteGroup)
                .Include(rs => rs.B_SiteData.M_SiteGroup1)
                .Include(rs => rs.B_SiteData.M_SiteGroup.Select(sg => sg.B_SiteData1))
                .Include(rs => rs.B_SiteData.M_SiteGroup1.Select(sg => sg.B_SiteData))
                .AsQueryable();

            DateTime startDate = input.StartDate.ParseDateTime().Date;
            DateTime endDate = input.EndDate.ParseDateTime().Date;

            // 日期範圍
            query = query.Where(
                rs => DbFunctions.TruncateTime(rs.TargetDate) >= startDate &&
                      DbFunctions.TruncateTime(rs.TargetDate) <= endDate);

            // 預約單號
            if (input.RHID.IsAboveZero())
                query = query.Where(rs => rs.RHID == input.RHID);

            // 場地 ID
            // 包含父場地與子場地
            if (input.BSID.IsAboveZero())
                query = query.Where(rs => rs.BSID == input.BSID
                                          || rs.B_SiteData.M_SiteGroup.Any(sg => sg.B_SiteData1.BSID == input.BSID)
                                          || rs.B_SiteData.M_SiteGroup1.Any(sg => sg.B_SiteData.BSID == input.BSID));

            // 客戶名稱
            if (input.CustomerTitleC.HasContent())
                query = query.Where(rs =>
                    rs.Resver_Head.CustomerTitle.Contains(input.CustomerTitleC) ||
                    rs.Resver_Head.Customer.TitleC.Contains(input.CustomerTitleC));

            if (input.SiteTitle.HasContent())
                query = query.Where(rs => rs.B_SiteData.Title.Contains(input.SiteTitle));

            query = query.Where(rs => !rs.DeleteFlag);

            query = query.Where(rs => !rs.Resver_Head.DeleteFlag);

            return query.OrderBy(rs => rs.TargetDate);
        }

        #endregion
    }
}