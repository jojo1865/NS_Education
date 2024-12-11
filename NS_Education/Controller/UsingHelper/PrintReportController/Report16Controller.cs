using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report16;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地使用一覽表的處理。
    /// </summary>
    public class Report16Controller : PublicClass, IPrintReport<Report16_Input_APIItem, Report16_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report16_Output_Row_APIItem>> GetResultAsync(
            Report16_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();

                DateTime startTime = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

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
                            .Where(rts => rts.TargetTable == tableName)
                            .OrderBy(rts => rts.DTSID), // 依據 DTSID 排序，這樣在稍後特殊計算每個時段價格時，才會有穩定的結果
                        rs => rs.RSID,
                        rts => rts.TargetID,
                        (rs, rts) => new { resverSite = rs, timeSpans = rts }
                    )
                    .AsQueryable();

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

                Report16_Output_APIItem response = new Report16_Output_APIItem();
                response.SetByInput(input);

                response.Items = results
                    .Where(x => x.timeSpans.Any())
                    // 每個 x 先計算並變形成單行
                    .SelectMany(x =>
                    {
                        // 這張報表要把報價依特殊計算方式拆回給每個時段
                        // 特殊計算的原因與內容請參照下列方法

                        D_TimeSpan[] timeSpans = x.timeSpans.Select(ts => ts.D_TimeSpan).ToArray();

                        decimal[] fixedPrices = x.resverSite
                            .GetFixedPriceByTimeSpan(timeSpans)
                            .ToArray();

                        decimal[] quotedPrices = x.resverSite
                            .GetQuotedPriceByTimeSpan(timeSpans)
                            .ToArray();

                        return x.timeSpans
                            .Select((ts, index) => new Report16_Output_Row_APIItem
                            {
                                Date = x.resverSite.TargetDate.ToString("yy/MM/dd"),
                                Site = x.resverSite.B_SiteData.Title,
                                TimeSpan = ts.D_TimeSpan.Title,
                                StartDate = x.resverSite.Resver_Head.SDate.ToString("yy/MM/dd"),
                                EndDate = x.resverSite.Resver_Head.EDate.ToString("yy/MM/dd"),
                                RHID = x.resverSite.RHID,
                                CustomerCode =
                                    x.resverSite.Resver_Head.Customer?.Code ?? "",
                                Host = x.resverSite.Resver_Head.Customer?.TitleC ?? "",
                                HostType = ((CustomerType?)x.resverSite.Resver_Head.Customer?.TypeFlag ??
                                            CustomerType.External)
                                    .GetTypeFlagName(),
                                MKSales = x.resverSite.Resver_Head.BusinessUser.Name,
                                OPSales = x.resverSite.Resver_Head.BusinessUser1.Name,
                                EventName = x.resverSite.Resver_Head.Title,
                                UnitPrice = Convert.ToInt32(fixedPrices.ElementAtOrDefault(index)),
                                QuotedPrice = Convert.ToInt32(quotedPrices.ElementAtOrDefault(index))
                            });
                    })
                    // 然後再依據日期、場地、時段、預約單彙整
                    .GroupBy(row => new { row.Date, row.Site, row.TimeSpan, row.RHID })
                    .Select(grouping =>
                    {
                        // 因為這邊 Group by RHID, 有些欄位其實都會相同。
                        // 這些欄位也可以放在 GroupBy 中，這裡找 first 來用純粹是為了讓 GroupBy 比較簡潔
                        Report16_Output_Row_APIItem first = grouping.First();
                        return new Report16_Output_Row_APIItem
                        {
                            Date = grouping.Key.Date,
                            Site = grouping.Key.Site,
                            TimeSpan = grouping.Key.TimeSpan,
                            StartDate = first.StartDate,
                            EndDate = first.EndDate,
                            RHID = grouping.Key.RHID,
                            CustomerCode = first.CustomerCode,
                            Host = first.Host,
                            HostType = first.HostType,
                            MKSales = first.MKSales,
                            OPSales = first.OPSales,
                            EventName = first.EventName,
                            UnitPrice = first.UnitPrice,
                            QuotedPrice = grouping.Sum(g => g.QuotedPrice)
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
        public async Task<ActionResult> GetExcel(Report16_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            
            CommonResponseForPagedList<Report16_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "場地使用一覽表",
                Columns = 13
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo();

            excelBuilder.CreateHeader(info);

            IDictionary<string, string> conditions = new (string name, string value)[]
                {
                    ("使用日:",
                        new[] { input.StartDate, input.EndDate }.Distinct().Where(s => s.HasContent()).StringJoin("~")),
                    ("場地名稱:", input.SiteName),
                    ("啟用:", input.IsActive.HasValue
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

            bool SameHead(Report16_Output_Row_APIItem last, Report16_Output_Row_APIItem current) =>
                last.RHID == current.RHID;

            bool SameSite(Report16_Output_Row_APIItem last, Report16_Output_Row_APIItem current) =>
                last.Site == current.Site;

            bool SameDate(Report16_Output_Row_APIItem last, Report16_Output_Row_APIItem current) =>
                last.Date == current.Date;

            bool SameDateRange(Report16_Output_Row_APIItem last, Report16_Output_Row_APIItem current) =>
                last.StartDate == current.StartDate && last.EndDate == current.EndDate;

            excelBuilder.StartDefineTable<Report16_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "使用日", i => i.Date, (l, c) => SameHead(l, c) && SameDate(l, c))
                .StringColumn(1, "起始日", i => i.StartDate, (l, c) => SameHead(l, c) && SameDateRange(l, c))
                .StringColumn(2, "結束日", i => i.EndDate, (l, c) => SameHead(l, c) && SameDateRange(l, c))
                .StringColumn(3, "場地", i => i.Site,
                    (l, c) => SameHead(l, c) && SameSite(l, c))
                .StringColumn(4, "使用時段", i => i.TimeSpan)
                .StringColumn(5, "預約單號", i => i.RHID.ToString(), SameHead)
                .StringColumn(6, "客戶代號", i => i.CustomerCode, SameHead)
                .StringColumn(7, "客戶名稱", i => i.Host, SameHead)
                .StringColumn(8, "活動名稱", i => i.EventName, SameHead)
                .StringColumn(9, "類別", i => i.HostType, SameHead)
                .StringColumn(10, "MK", i => i.MKSales, SameHead)
                .StringColumn(11, "OP", i => i.OPSales, SameHead)
                .NumberColumn(12, "場地報價", i => i.QuotedPrice, true,
                    (l, c) => l.QuotedPrice == c.QuotedPrice && SameHead(l, c))
                .AddToBuilder(excelBuilder);

            excelBuilder.NowRow()
                .Align(1, HorizontalAlignment.Right)
                .SetValue(1, data.Items.Count)
                .SetValue(2, "筆");

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report16_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}