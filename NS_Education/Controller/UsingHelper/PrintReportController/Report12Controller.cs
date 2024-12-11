using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using Microsoft.Ajax.Utilities;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report12;
using NS_Education.Models.Entities;
using NS_Education.Models.Errors;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

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
            DateTime startYearMonth = default;
            DateTime endYearMonth = default;

            bool isValid = input.StartValidate()
                .Validate(i => i.StartYearMonth.HasContent(),
                    () => AddError(EmptyNotAllowed("年月區間起", nameof(input.StartYearMonth))))
                .Validate(i => i.EndYearMonth.HasContent(),
                    () => AddError(EmptyNotAllowed("年月區間迄", nameof(input.EndYearMonth))))
                .Validate(i => i.StartYearMonth.TryParseDateTime(out startYearMonth, DateTimeParseType.YearMonth),
                    () => AddError(WrongFormat("年月區間起", nameof(input.StartYearMonth))))
                .Validate(i => i.EndYearMonth.TryParseDateTime(out endYearMonth, DateTimeParseType.YearMonth),
                    () => AddError(WrongFormat("年月區間迄", nameof(input.EndYearMonth))))
                .Validate(
                    i => startYearMonth.TotalMonths(endYearMonth) == input.MonthHours.Length,
                    () => AddError(new BusinessError(1, "時段數的值不符合輸入的年月區間！")))
                .IsValid();

            if (!isValid)
                return null;

            // 如果 CommDept, Internal, External 有任何一者為 true 時
            // null 視為 false

            if (input.ShowCommDept is true || input.ShowExternal is true || input.ShowInternal is true)
            {
                input.ShowCommDept = input.ShowCommDept ?? false;
                input.ShowExternal = input.ShowExternal ?? false;
                input.ShowInternal = input.ShowInternal ?? false;
            }
            else
            {
                input.ShowCommDept = input.ShowCommDept ?? true;
                input.ShowExternal = input.ShowExternal ?? true;
                input.ShowInternal = input.ShowInternal ?? true;
            }

            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = startYearMonth;
                DateTime endTime = endYearMonth.AddMonths(1).AddSeconds(-1);
                string tableName = dbContext.GetTableName<Resver_Site>();

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .Include(rs => rs.B_SiteData.B_Category)
                    .Include(rs => rs.Resver_Head)
                    .Include(rs => rs.Resver_Head.Customer)
                    .Where(rs => !rs.DeleteFlag)
                    .Where(rs => !rs.Resver_Head.DeleteFlag)
                    .Where(rs => startTime <= rs.TargetDate && rs.TargetDate <= endTime)
                    .Where(rs => input.BC_Title == null || rs.B_SiteData.B_Category.TitleC == input.BC_Title)
                    .Where(rs => input.SiteName == null || rs.B_SiteData.Title.Contains(input.SiteName))
                    .Where(rs =>
                        input.ShowInternal.Value || rs.Resver_Head.Customer.TypeFlag != (int)CustomerType.Internal)
                    .Where(rs =>
                        input.ShowExternal.Value || rs.Resver_Head.Customer.TypeFlag != (int)CustomerType.External)
                    .Where(rs =>
                        input.ShowCommDept.Value || rs.Resver_Head.Customer.TypeFlag != (int)CustomerType.CommDept)
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

                // populate 表頭
                // 第一行：人數、場地、編號、面積（坪）、全館使用率...、內部使用率...、外部使用率...、通訊處使用率...
                // 第二行：時段數、　、　、　、小時...

                IEnumerable<string> firstLine = new[]
                {
                    "人數", "場地", "編號", "面積（坪）"
                };

                IEnumerable<string> firstLineUsages = new[]
                    {
                        startTime.MonthRange(endYearMonth).Select(d => d.ToString("yyyy/MM全館使用率")),
                        input.ShowInternal != null && input.ShowInternal.Value
                            ? startTime.MonthRange(endYearMonth).Select(d => d.ToString("yyyy/MM內部使用率"))
                            : null,
                        input.ShowExternal != null && input.ShowExternal.Value
                            ? startTime.MonthRange(endYearMonth).Select(d => d.ToString("yyyy/MM外部使用率"))
                            : null,
                        input.ShowCommDept != null && input.ShowCommDept.Value
                            ? startTime.MonthRange(endYearMonth).Select(d => d.ToString("yyyy/MM通訊處使用率"))
                            : null
                    }
                    .Where(e => e != null)
                    .SelectMany(s => s);

                firstLine = firstLine.Concat(firstLineUsages);

                response.Items = new List<Report12_Output_Row_APIItem>()
                {
                    new Report12_Output_Row_APIItem
                    {
                        Cells = firstLine
                    },
                    new Report12_Output_Row_APIItem()
                    {
                        Cells = new[] { "時段數", "", "", "" }.Concat(input.MonthHours.Select(h => h.ToString()))
                    }
                };


                List<Report12_Output_Row_APIItem> actualRows = sites
                    .Select(sd =>
                    {
                        var resver = groupedResverSites.GetValueOrDefault(sd.BSID)?.ToArray();
                        IEnumerable<Resver_Site> rs = resver?.Select(x => x.rs) ?? Array.Empty<Resver_Site>();
                        IEnumerable<M_Resver_TimeSpan> rts = resver?.Select(x => x.rts).ToArray() ??
                                                             Array.Empty<M_Resver_TimeSpan>();
                        rs = rs.ToArray();

                        List<string> results = new List<string>
                        {
                            sd.Title,
                            sd.Code,
                            sd.BasicSize.ToString(),
                            sd.AreaSize.ToString(),
                        };

                        Report12_Output_Row_MonthlyUsage_APIItem totalUsages = GetUsage(rs, rts, input);
                        Report12_Output_Row_MonthlyUsage_APIItem internalUsages = GetUsage(
                            rs.Where(r => r.Resver_Head.Customer.TypeFlag == (int)CustomerType.Internal),
                            rts, input);
                        Report12_Output_Row_MonthlyUsage_APIItem externalUsages = GetUsage(
                            rs.Where(r => r.Resver_Head.Customer.TypeFlag == (int)CustomerType.External),
                            rts, input);
                        Report12_Output_Row_MonthlyUsage_APIItem commDeptUsages = GetUsage(
                            rs.Where(r => r.Resver_Head.Customer.TypeFlag == (int)CustomerType.CommDept),
                            rts, input);

                        IEnumerable<string> usages = new[]
                            {
                                totalUsages,
                                input.ShowInternal != null && input.ShowInternal.Value ? internalUsages : null,
                                input.ShowExternal != null && input.ShowExternal.Value ? externalUsages : null,
                                input.ShowCommDept != null && input.ShowCommDept.Value ? commDeptUsages : null
                            }
                            .Where(u => u != null)
                            .SelectMany(u => u.MonthlyUsage);

                        results.AddRange(usages);

                        Report12_Output_Row_APIItem row = new Report12_Output_Row_APIItem
                        {
                            Cells = results
                        };

                        return row;
                    })
                    .SortWithInput(input)
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                response.Items = response.Items
                    .Concat(actualRows)
                    .ToList();

                int totalAreaSize = sites.Sum(s => (int?)s.AreaSize) ?? 0;
                HashSet<B_SiteData> uniqueSites = sites.DistinctBy(s => s.BSID).ToHashSet();

                // Total 行
                Resver_Site[] allRs = groupedResverSites.Values.SelectMany(v => v).Select(v => v.rs).ToArray();
                M_Resver_TimeSpan[] allRts = groupedResverSites.Values.SelectMany(v => v).Select(v => v.rts).ToArray();

                Report12_Output_Row_MonthlyUsage_APIItem allTotalUsages = GetUsage(allRs, allRts, input, uniqueSites);
                Report12_Output_Row_MonthlyUsage_APIItem allInternalUsages = GetUsage(
                    allRs.Where(r => r.Resver_Head.Customer.TypeFlag == (int)CustomerType.Internal),
                    allRts, input, uniqueSites);
                Report12_Output_Row_MonthlyUsage_APIItem allExternalUsages = GetUsage(
                    allRs.Where(r => r.Resver_Head.Customer.TypeFlag == (int)CustomerType.External),
                    allRts, input, uniqueSites);
                Report12_Output_Row_MonthlyUsage_APIItem allCommDeptUsages = GetUsage(
                    allRs.Where(r => r.Resver_Head.Customer.TypeFlag == (int)CustomerType.CommDept),
                    allRts, input, uniqueSites);

                IEnumerable<string> allUsages = new[]
                    {
                        allTotalUsages,
                        input.ShowInternal != null && input.ShowInternal.Value ? allInternalUsages : null,
                        input.ShowExternal != null && input.ShowExternal.Value ? allExternalUsages : null,
                        input.ShowCommDept != null && input.ShowCommDept.Value ? allCommDeptUsages : null
                    }
                    .Where(u => u != null)
                    .SelectMany(u => u.MonthlyUsage);

                response.Items.Add(new Report12_Output_Row_APIItem
                {
                    Cells = new[]
                        {
                            null,
                            null,
                            null,
                            totalAreaSize.ToString()
                        }
                        .Concat(allUsages)
                });

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = sites.Length;
                return response;
            }
        }

        private static Report12_Output_Row_MonthlyUsage_APIItem GetUsage(IEnumerable<Resver_Site> resverSites,
            IEnumerable<M_Resver_TimeSpan> resverTimeSpans, Report12_Input_APIItem input,
            ISet<B_SiteData> allUniqueSites = null)
        {
            resverSites = resverSites.ToArray();
            resverTimeSpans = resverTimeSpans.ToArray();

            DateTime start = input.StartYearMonth.ParseDateTime(DateTimeParseType.YearMonth).Date;
            DateTime end = input.EndYearMonth.ParseDateTime(DateTimeParseType.YearMonth).Date;

            return new Report12_Output_Row_MonthlyUsage_APIItem
            {
                MonthlyUsageDecimal = start.MonthRange(end)
                    .Select(month => GetUsageInMonth(resverSites, resverTimeSpans, month, input, allUniqueSites))
                    .ToArray()
            };
        }

        private static decimal? GetUsageInMonth(IEnumerable<Resver_Site> resverSites,
            IEnumerable<M_Resver_TimeSpan> resverTimeSpans, DateTime yearMonth, Report12_Input_APIItem input,
            ISet<B_SiteData> allUniqueSites = null)
        {
            DateTime start = input.StartYearMonth.ParseDateTime(DateTimeParseType.YearMonth);

            int hours = input.MonthHours[start.TotalMonths(yearMonth) - 1];

            if (!hours.IsAboveZero())
                return null;

            // 教室使用率 = (使用時段數 * 教室坪數) / (每月可供租用時段數 * 教室坪數)
            //
            // 總使用率時，resverSites 可能包含多種 B_SiteDate
            // 每月總使用率 = (每間教室使用時段數 * 每間教室坪數) / (每月可供租用時段數 * 教室總坪數)
            //
            // ( Σ (各教室的租用小時數 * 各教室坪數) ) / ( hour * Σ 所有獨特場地的坪數 )
            resverSites = resverSites
                .Where(rs => rs.TargetDate.Year == yearMonth.Year && rs.TargetDate.Month == yearMonth.Month).ToArray();
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
            input.NowPage = 0; // 報表時強制全查
            
            var details = await GetResultAsync(input);
            details.Items = details.Items.Take(details.Items.Count - 1).ToList();

            return GetResponseJson(details);
        }
    }
}