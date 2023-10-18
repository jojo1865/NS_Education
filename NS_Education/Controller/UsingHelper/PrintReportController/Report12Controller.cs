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
using NS_Education.Models.Utilities.PrintReport;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using QuestPDF.Helpers;

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
                    .OrderByDescending(sd => Convert.ToInt32(sd.PeopleCt))
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                int totalAreaSize = sites.Sum(s => (int?)s.AreaSize) ?? 0;
                HashSet<B_SiteData> uniqueSites = sites.DistinctBy(s => s.BSID).ToHashSet();

                // Total 行
                var allRs = groupedResverSites.Values.SelectMany(v => v).Select(v => v.rs).ToArray();
                var allRts = groupedResverSites.Values.SelectMany(v => v).Select(v => v.rts).ToArray();
                response.Items.Add(new Report12_Output_Row_APIItem
                {
                    PeopleCt = null,
                    SiteName = null,
                    SiteCode = null,
                    AreaSize = totalAreaSize,
                    AllUsage = GetUsage(allRs, allRts, input, uniqueSites),
                    InternalUsage = GetUsage(allRs.Where(rs => rs.Resver_Head.Customer.InFlag), allRts, input,
                        uniqueSites),
                    ExternalUsage = GetUsage(allRs.Where(rs => !rs.Resver_Head.Customer.InFlag), allRts, input,
                        uniqueSites)
                });

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = resverSites.Count();
                return response;
            }
        }

        private static Report12_Output_Row_MonthlyUsage_APIItem GetUsage(IEnumerable<Resver_Site> resverSites,
            IEnumerable<M_Resver_TimeSpan> resverTimeSpans, Report12_Input_APIItem input,
            ISet<B_SiteData> allUniqueSites = null)
        {
            resverSites = resverSites.ToArray();
            resverTimeSpans = resverTimeSpans.ToArray();

            return new Report12_Output_Row_MonthlyUsage_APIItem
            {
                MonthlyUsageDecimal = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }
                    .Select(month => GetUsageInMonth(resverSites, resverTimeSpans, month, input, allUniqueSites))
                    .ToArray()
            };
        }

        private static decimal? GetUsageInMonth(IEnumerable<Resver_Site> resverSites,
            IEnumerable<M_Resver_TimeSpan> resverTimeSpans, int month, Report12_Input_APIItem input,
            ISet<B_SiteData> allUniqueSites = null)
        {
            int hours = input.Hours[month - 1] ?? 0;

            if (!hours.IsAboveZero())
                return null;

            // 教室使用率 = (使用時段數 * 教室坪數) / (每月可供租用時段數 * 教室坪數)
            //
            // 總使用率時，resverSites 可能包含多種 B_SiteDate
            // 每月總使用率 = (每間教室使用時段數 * 每間教室坪數) / (每月可供租用時段數 * 教室總坪數)
            //
            // ( Σ (各教室的租用小時數 * 各教室坪數) ) / ( hour * Σ 所有獨特場地的坪數 )
            resverSites = resverSites.Where(rs => rs.TargetDate.Month == month).ToArray();
            HashSet<int> ids = resverSites.Select(rs => rs.RSID).Distinct().ToHashSet();
            resverTimeSpans = resverTimeSpans.Where(rts => ids.Contains(rts.TargetID));

            int totalRentedMinutesPerArea = resverTimeSpans
                .Sum(rts =>
                {
                    D_TimeSpan dts = rts.D_TimeSpan;
                    int minutes = (dts.HourS, dts.MinuteS).GetMinutesUntil((dts.HourE, dts.MinuteE));
                    int area = resverSites.First(rs => rs.RSID == rts.TargetID)
                        .B_SiteData
                        .AreaSize;

                    return (int?)minutes * area;
                }) ?? 0;
            int totalRentedHoursPerArea = totalRentedMinutesPerArea / 60;

            // 如果沒有任何租用，回傳 null
            if (!totalRentedHoursPerArea.IsAboveZero())
                return null;

            // 如果有提供 allUniqueSites（如 total 行），用 allUniqueSites 的場地坪數為分母元素
            // 因為 resverSites 不一定包含所有場地

            ISet<B_SiteData> uniqueSites = allUniqueSites ??
                                           resverSites.Select(rs => rs.B_SiteData).DistinctBy(bs => bs.BSID)
                                               .ToHashSet();

            int totalAreaSize = uniqueSites.Sum(sd => (int?)sd.AreaSize) ?? 0;

            totalAreaSize *= hours;

            // 分母不正確時，回傳 null
            if (!totalAreaSize.IsAboveZero())
                return null;

            return Decimal.Divide(totalRentedHoursPerArea, totalAreaSize);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report12_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<FileContentResult> GetPdf(Report12_Input_APIItem input)
        {
            CommonResponseForPagedList<Report12_Output_Row_APIItem> data = await GetResultAsync(input);

            Report12_Output_Row_APIItem totalRow = data.Items
                .Last(i => i.PeopleCt == null);

            data.Items = data.Items
                .Where(i => i != totalRow)
                .ToArray();

            IEnumerable<PdfColumn<Report12_Output_Row_APIItem>> usageColumns = new[] { "全部", "內部", "外部" }
                .SelectMany(i => new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Select(j =>
                    new PdfColumn<Report12_Output_Row_APIItem>
                    {
                        Name = $"{j}月-{i} ({input.Hours[j - 1]})",
                        LengthWeight = 4,
                        Selector = row => GetUsageByType(i).Invoke(row).MonthlyUsageDecimal[j - 1],
                        Formatter = usage => ((decimal?)usage)?.ToString("P") ?? "-",
                        OutputTotal = true
                    }));
            byte[] pdf = data.MakePdf(input,
                GetUid(),
                await GetUserNameByID(GetUid()),
                "場地使用率分析表",
                new[]
                    {
                        new PdfColumn<Report12_Output_Row_APIItem>
                        {
                            Name = "人數",
                            LengthWeight = 2,
                            Formatter = o => o?.ToString() ?? "Total",
                            Selector = r => r.PeopleCt
                        },
                        new PdfColumn<Report12_Output_Row_APIItem>
                        {
                            Name = "場地",
                            LengthWeight = 5,
                            Selector = r => r.SiteName
                        },
                        new PdfColumn<Report12_Output_Row_APIItem>
                        {
                            Name = "場地代號",
                            LengthWeight = 4,
                            Selector = r => r.SiteCode
                        },
                        new PdfColumn<Report12_Output_Row_APIItem>
                        {
                            Name = "面積(坪)",
                            LengthWeight = 3,
                            Selector = r => r.AreaSize,
                            OutputTotal = true
                        },
                    }
                    .Concat(usageColumns)
                    .ToArray(),
                $"年份 = {input.Year}",
                new PageSize(PageSizes.A4.Width * 3.5f, PageSizes.A4.Height),
                totalRow
            );

            return new FileContentResult(pdf, "application/pdf");
        }

        private static Func<Report12_Output_Row_APIItem, Report12_Output_Row_MonthlyUsage_APIItem> GetUsageByType(
            string type)
        {
            switch (type)
            {
                case "內部":
                    return row => row.InternalUsage;
                case "外部":
                    return row => row.ExternalUsage;
                default:
                    return row => row.AllUsage;
            }
        }
    }
}