using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report12;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地使用率分析表的處理。
    /// </summary>
    public class Report12Controller : PublicClass, IPrintReport<Report12_Input_APIItem, Report12_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report12_Output_Row_APIItem>> GetResultAsync(
            Report12_Input_APIItem input)
        {
            if (!input.Year.IsAboveZero())
                AddError(EmptyNotAllowed("西元年", nameof(input.Year)));

            if (!input.Hours.Any(h => h.IsAboveZero()))
                AddError(EmptyNotAllowed("每月可租用時段數（須至少一個月有輸入）", nameof(input.Hours)));

            if (HasError())
                return null;

            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = new DateTime(input.Year, 1, 1).Date;
                DateTime endTime = startTime.AddYears(1).AddDays(-1).Date;

                string tableName = dbContext.GetTableName<Resver_Site>();

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .Include(rs => rs.Resver_Head)
                    .Include(rs => rs.Resver_Head.Customer)
                    .Where(rs => !rs.DeleteFlag)
                    .Where(rs => !rs.Resver_Head.DeleteFlag)
                    .Where(rs => startTime <= rs.TargetDate && rs.TargetDate <= endTime)
                    .GroupJoin(dbContext.M_Resver_TimeSpan
                            .Include(rts => rts.D_TimeSpan)
                            .Where(rts => rts.TargetTable == tableName),
                        rs => rs.RSID,
                        rts => rts.TargetID,
                        (rs, rts) => new { rs, rts }
                    )
                    .SelectMany(e => e.rts.DefaultIfEmpty(), (e, rts) => new { e.rs, rts })
                    .AsQueryable();

                var resverSites = await query.ToArrayAsync();

                resverSites = resverSites.Where(x => x.rts != null).ToArray();

                var sites = await dbContext.B_SiteData
                    .Where(sd => sd.ActiveFlag && !sd.DeleteFlag)
                    .ToArrayAsync();

                Report12_Output_APIItem response = new Report12_Output_APIItem();
                response.SetByInput(input);

                var groupedResverSites = resverSites
                    .GroupBy(g => g.rs.BSID)
                    .ToDictionary(g => g.Key, g => g);

                response.Items = sites
                    .OrderBy(sd => sd.BSID)
                    .Select(sd =>
                    {
                        var resver = groupedResverSites.GetValueOrDefault(sd.BSID)?.ToArray();
                        IEnumerable<Resver_Site> rs = resver?.Select(x => x.rs) ?? Array.Empty<Resver_Site>();
                        IEnumerable<M_Resver_TimeSpan> rts = resver?.Select(x => x.rts).ToArray() ??
                                                             Array.Empty<M_Resver_TimeSpan>();
                        rs = rs.ToArray();

                        return new Report12_Output_Row_APIItem
                        {
                            SiteName = sd.Title,
                            SiteCode = sd.Code,
                            PeopleCt = sd.MaxSize.ToString(),
                            AreaSize = sd.AreaSize,
                            AllUsage = GetUsage(rs, rts, input),
                            InternalUsage = GetUsage(rs.Where(r => r.Resver_Head.Customer.InFlag), rts, input),
                            ExternalUsage = GetUsage(rs.Where(r => !r.Resver_Head.Customer.InFlag), rts, input),
                        };
                    })
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                // 每月總使用率 = Σ (每間教室使用時段數 * 每間教室坪數) / (每月可供租用時段數 * 教室總坪數)
                int totalAreaSize = sites.Sum(s => (int?)s.AreaSize) ?? 0;

                // Total 行
                var allRs = groupedResverSites.Values.SelectMany(v => v).Select(v => v.rs).ToArray();
                var allRts = groupedResverSites.Values.SelectMany(v => v).Select(v => v.rts).ToArray();
                response.Items.Add(new Report12_Output_Row_APIItem
                {
                    PeopleCt = null,
                    SiteName = null,
                    SiteCode = null,
                    AreaSize = totalAreaSize,
                    AllUsage = GetUsage(allRs, allRts, input),
                    InternalUsage = GetUsage(allRs.Where(rs => rs.Resver_Head.Customer.InFlag), allRts, input),
                    ExternalUsage = GetUsage(allRs.Where(rs => !rs.Resver_Head.Customer.InFlag), allRts, input)
                });

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = resverSites.Count();
                return response;
            }
        }

        private static Report12_Output_Row_MonthlyUsage_APIItem GetUsage(IEnumerable<Resver_Site> resverSites,
            IEnumerable<M_Resver_TimeSpan> resverTimeSpans, Report12_Input_APIItem input)
        {
            resverSites = resverSites.ToArray();
            resverTimeSpans = resverTimeSpans.ToArray();

            return new Report12_Output_Row_MonthlyUsage_APIItem
            {
                Jan = GetUsageInMonth(resverSites, resverTimeSpans, 1, input),
                Feb = GetUsageInMonth(resverSites, resverTimeSpans, 2, input),
                Mar = GetUsageInMonth(resverSites, resverTimeSpans, 3, input),
                Apr = GetUsageInMonth(resverSites, resverTimeSpans, 4, input),
                May = GetUsageInMonth(resverSites, resverTimeSpans, 5, input),
                Jun = GetUsageInMonth(resverSites, resverTimeSpans, 6, input),
                Jul = GetUsageInMonth(resverSites, resverTimeSpans, 7, input),
                Aug = GetUsageInMonth(resverSites, resverTimeSpans, 8, input),
                Sep = GetUsageInMonth(resverSites, resverTimeSpans, 9, input),
                Oct = GetUsageInMonth(resverSites, resverTimeSpans, 10, input),
                Nov = GetUsageInMonth(resverSites, resverTimeSpans, 11, input),
                Dec = GetUsageInMonth(resverSites, resverTimeSpans, 12, input)
            };
        }

        private static string GetUsageInMonth(IEnumerable<Resver_Site> resverSites,
            IEnumerable<M_Resver_TimeSpan> resverTimeSpans, int month, Report12_Input_APIItem input)
        {
            int? hours = input.Hours[month - 1];

            if (!hours.IsAboveZero())
                return null;

            // 教室使用率 = (使用時段數 * 教室坪數) / (每月可供租用時段數 * 教室坪數)
            //
            // 總使用率時，resverSites 可能包含多種 B_SiteDate
            // 每月總使用率 = (每間教室使用時段數 * 每間教室坪數) / (每月可供租用時段數 * 教室總坪數)

            resverSites = resverSites.Where(rs => rs.TargetDate.Month == month).ToArray();
            HashSet<int> ids = resverSites.Select(rs => rs.RSID).Distinct().ToHashSet();

            resverTimeSpans = resverTimeSpans.Where(rts => ids.Contains(rts.TargetID));

            // 時段數在這邊就考慮坪數加權
            int usedPeriods = resverTimeSpans
                .Select(rts => resverSites.First(rs => rs.RSID == rts.TargetID).B_SiteData.AreaSize).Sum();

            int totalAreaSize = resverSites.Select(rs => rs.B_SiteData)
                .DistinctBy(bs => bs.BSID)
                .Sum(bs => (int?)bs.AreaSize) ?? 0;

            int divider = (hours ?? 40) * totalAreaSize;

            if (!divider.IsAboveZero())
                return null;

            // 40: 來自報表樣張，預設的每個月可租用時段數
            return Decimal.Divide(usedPeriods, divider).ToString("P");
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report12_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}