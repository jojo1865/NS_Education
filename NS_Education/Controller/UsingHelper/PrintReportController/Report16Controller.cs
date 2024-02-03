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

                query = query.OrderBy(e => e.rs.RSID);

                var results = await query.ToArrayAsync();

                Report16_Output_APIItem response = new Report16_Output_APIItem();
                response.SetByInput(input);

                response.Items = results
                    .Where(e => e.rts != null)
                    .GroupBy(e => new { e.rs.TargetDate, e.rs.BSID, e.rts.DTSID, e.rs.RHID, e.rs.QuotedPrice })
                    .Select(e => new Report16_Output_Row_APIItem
                    {
                        Date = e.Max(grouping => grouping.rs.TargetDate).ToString("yy/MM/dd"),
                        Site = e.Max(grouping => grouping.rs.B_SiteData.Title),
                        TimeSpan = e.Max(grouping => grouping.rts.D_TimeSpan.Title),
                        StartDate = e.Max(grouping => grouping.rs.Resver_Head.SDate).ToString("yy/MM/dd"),
                        EndDate = e.Max(grouping => grouping.rs.Resver_Head.EDate).ToString("yy/MM/dd"),
                        RHID = e.Max(grouping => grouping.rs.RHID),
                        CustomerCode = e.Max(grouping => grouping.rs.Resver_Head.Customer?.Code ?? ""),
                        Host = e.Max(grouping => grouping.rs.Resver_Head.Customer?.TitleC ?? ""),
                        HostType = e.Max(grouping
                            => grouping.rs.Resver_Head.Customer.TypeFlag == (int)CustomerType.Internal
                                ? "內部單位"
                                : grouping.rs.Resver_Head.Customer.TypeFlag == (int)CustomerType.CommDept
                                    ? "通訊處"
                                    : "外部單位"),
                        MKSales = e.Max(grouping => grouping.rs.Resver_Head.BusinessUser.Name),
                        OPSales = e.Max(grouping => grouping.rs.Resver_Head.BusinessUser1.Name),
                        EventName = e.Max(grouping => grouping.rs.Resver_Head.Title),
                        UnitPrice = e.Max(grouping => grouping.rs.B_SiteData.UnitPrice),
                        QuotedPrice = e.Max(grouping => grouping.rs.QuotedPrice)
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