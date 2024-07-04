using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report13;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地預估銷售月報表的處理。
    /// </summary>
    public class Report13Controller : PublicClass, IPrintReport<Report13_Input_APIItem, Report13_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report13_Output_Row_APIItem>> GetResultAsync(
            Report13_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();

                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime().Date ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .Include(rs => rs.Resver_Head)
                    .Where(rs => !rs.DeleteFlag)
                    .Where(rs => !rs.Resver_Head.DeleteFlag)
                    .Where(rs => startTime <= rs.TargetDate && rs.TargetDate <= endTime)
                    .GroupJoin(dbContext.M_Resver_TimeSpan
                            .Include(rts => rts.D_TimeSpan)
                            .Where(rts => rts.TargetTable == tableName),
                        rs => rs.RSID,
                        rts => rts.TargetID,
                        (rs, rts) => new { resverSite = rs, timeSpans = rts }
                    )
                    .AsQueryable();

                if (input.TargetMonth.HasContent())
                {
                    // yyyy/MM
                    string[] splits = input.TargetMonth.Split('/');
                    int year = splits.Take(1).Select(s => (int?)Convert.ToInt32(s)).FirstOrDefault() ?? 0;
                    int month = splits.Skip(1).Take(1).Select(s => (int?)Convert.ToInt32(s)).FirstOrDefault() ?? 0;

                    DateTime start = new DateTime(year, month, 1);
                    DateTime end = start.AddMonths(1);

                    query = query.Where(x => start <= x.resverSite.TargetDate && x.resverSite.TargetDate < end);
                }

                if (input.SiteName.HasContent())
                    query = query.Where(x => x.resverSite.B_SiteData.Title.Contains(input.SiteName));

                if (input.BCID.IsAboveZero())
                    query = query.Where(x => x.resverSite.B_SiteData.BCID == input.BCID);

                if (input.IsActive.HasValue)
                    query = query.Where(x => x.resverSite.B_SiteData.ActiveFlag == input.IsActive);

                if (input.BSCID1.IsAboveZero())
                    query = query.Where(x => x.resverSite.B_SiteData.BSCID1 == input.BSCID1);

                if (input.BasicSize.IsAboveZero())
                    query = query.Where(x => x.resverSite.B_SiteData.BasicSize >= input.BasicSize);

                query = query.OrderBy(e => e.resverSite.RSID);

                var results = await query.ToArrayAsync();

                Report13_Output_APIItem response = new Report13_Output_APIItem();
                response.SetByInput(input);

                response.Items = results
                    .Where(e => e.timeSpans.Any())
                    // 先 flatten by 預約時段後做成單筆單筆的 row
                    .SelectMany(x =>
                    {
                        D_TimeSpan[] timeSpans = x.timeSpans.Select(ts => ts.D_TimeSpan).ToArray();

                        IEnumerable<decimal> fixedPrices =
                            x.resverSite.GetFixedPriceByTimeSpan(timeSpans);
                        IEnumerable<decimal> quotedPrices =
                            x.resverSite.GetQuotedPriceByTimeSpan(timeSpans);

                        return x.timeSpans.Select((ts, idx) => new Report13_Output_Row_APIItem
                        {
                            SiteCode = x.resverSite.B_SiteData.Code,
                            SiteName = x.resverSite.B_SiteData.Title,
                            SiteQuotedPrice = Convert.ToInt32(quotedPrices.ElementAtOrDefault(idx)),
                            SiteUnitPrice = Convert.ToInt32(fixedPrices.ElementAtOrDefault(idx)),
                            TimeSpan = ts.D_TimeSpan.Title,
                            Quantity = 1
                        });
                    })
                    // 然後再 group by 場地,報價,時段 達成最後要呈現的報表內容
                    .GroupBy(row => new { row.SiteName, row.SiteQuotedPrice, row.TimeSpan })
                    .Select(grouping =>
                    {
                        Report13_Output_Row_APIItem first = grouping.First();

                        return new Report13_Output_Row_APIItem
                        {
                            SiteCode = first.SiteCode,
                            SiteName = first.SiteName,
                            SiteUnitPrice = first.SiteUnitPrice,
                            SiteQuotedPrice = grouping.Key.SiteQuotedPrice,
                            TimeSpan = grouping.Key.TimeSpan,
                            Quantity = grouping.Sum(g => g.Quantity)
                        };
                    })
                    .SortWithInput(input)
                    .ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);

                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();

                return response;
            }
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report13_Input_APIItem input)
        {
            // 匯出時忽略頁數篩選
            input.NowPage = 0;

            CommonResponseForPagedList<Report13_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "場地預估銷售月報表",
                Columns = 7
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo();

            excelBuilder.CreateHeader(info);

            IDictionary<string, string> conditions = new (string name, string value)[]
                {
                    ("查詢月份", input.TargetMonth),
                    ("查詢區間",
                        new[] { input.StartDate, input.EndDate }.Distinct().Where(s => s.HasContent()).StringJoin("~")),
                    ("場地名稱", input.SiteName),
                    ("啟用", input.IsActive.HasValue
                        ? input.IsActive.Value ? "是" : "否"
                        : null)
                }
                .Where(p => p.value.HasContent())
                .ToDictionary(p => p.name, p => p.value);

            excelBuilder.CreateRow();

            if (conditions.Keys.Any())
            {
                excelBuilder.NowRow()
                    .SetValue(0, "查詢條件:");

                foreach (KeyValuePair<string, string> kvp in conditions)
                {
                    excelBuilder.NowRow()
                        .SetValue(1, kvp.Key)
                        .SetValue(2, kvp.Value);

                    excelBuilder.CreateRow();
                }
            }

            excelBuilder.NowRow()
                .CombineCells(4, 6)
                .SetValue(4, "*總金額=場地報價*數量");

            excelBuilder.StartDefineTable<Report13_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "場地代號", i => i.SiteCode,
                    (l, c) => l.SiteCode == c.SiteCode)
                .StringColumn(1, "場地名稱", i => i.SiteName)
                .NumberColumn(2, "場地定價", i => i.SiteUnitPrice)
                .NumberColumn(3, "場地報價", i => i.SiteQuotedPrice)
                .StringColumn(4, "時段", i => i.TimeSpan)
                .NumberColumn(5, "數量", i => i.Quantity)
                .NumberColumn(6, "總金額", i => i.SiteQuotedPrice * i.Quantity, true)
                .AddToBuilder(excelBuilder);

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report13_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}