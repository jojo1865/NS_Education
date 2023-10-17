using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
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
    /// 場地使用率一覽表的處理。
    /// </summary>
    public class Report12Controller : PublicClass, IPrintReport<Report12_Input_APIItem, Report12_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report12_Output_Row_APIItem>> GetResultAsync(
            Report12_Input_APIItem input)
        {
            if (input.PeriodTotal <= 0)
            {
                AddError(OutOfRange("總時段數", nameof(Report12_Input_APIItem.PeriodTotal), 1));
                return null;
            }

            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

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

                if (input.SiteName.HasContent())
                    query = query.Where(x => x.rs.B_SiteData.Title.Contains(input.SiteName));

                if (input.BCID.IsAboveZero())
                    query = query.Where(x => x.rs.B_SiteData.BCID == input.BCID);

                if (input.IsActive.HasValue)
                    query = query.Where(x => x.rs.B_SiteData.ActiveFlag == input.IsActive);

                if (input.BSCID1.IsAboveZero())
                    query = query.Where(x => x.rs.B_SiteData.BSCID1 == input.BSCID1);

                if (input.BasicSize.IsAboveZero())
                    query = query.Where(x => x.rs.B_SiteData.BasicSize >= input.BasicSize);

                var resverSites = await query.ToArrayAsync();

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

                        int usedDays = resver?.Select(g => g.rs.TargetDate.Date).Distinct().Count() ?? 0;
                        int usedPeriods = resver?.Select(g => g.rts).Count() ?? 0;
                        return new Report12_Output_Row_APIItem
                        {
                            SiteName = sd.Title,
                            SiteCode = sd.Code,
                            PeopleCt = sd.MaxSize,
                            Days = usedDays,
                            Periods = usedPeriods,
                            // 教室使用率 = (使用時段數 * 教室坪數) / (每月可供租用時段數 * 教室坪數)
                            Usage = Decimal.Divide(usedPeriods, input.PeriodTotal).ToString("P"),
                            AreaSize = sd.AreaSize
                        };
                    })
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                // 每月總使用率 = Σ (每間教室使用時段數 * 每間教室坪數) / (每月可供租用時段數 * 教室總坪數)
                int totalUsageNumerator = response.Items.Sum(i => (int?)i.Periods * i.AreaSize) ?? 0;
                int totalAreaSize = sites.Sum(s => (int?)s.AreaSize) ?? 0;
                int totalUsageDenominator = input.PeriodTotal * totalAreaSize;

                response.TotalAreaSize = totalAreaSize;
                decimal totalUsage = totalUsageDenominator == 0
                    ? 0m
                    : Decimal.Divide(totalUsageNumerator, totalUsageDenominator);
                response.TotalUsage = totalUsage.ToString("P");

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = resverSites.Count();
                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report12_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}