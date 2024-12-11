using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report14;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地實際使用統計表的處理。
    /// </summary>
    public class Report14Controller : PublicClass, IPrintReport<Report14_Input_APIItem, Report14_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report14_Output_Row_APIItem>> GetResultAsync(
            Report14_Input_APIItem input)
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

                query = query.OrderBy(e => e.rs.B_SiteData.Code)
                    .ThenBy(e => e.rs.BSID)
                    .ThenBy(e => e.rts.D_TimeSpan.HourS)
                    .ThenBy(e => e.rts.D_TimeSpan.MinuteS);

                var results = await query.ToArrayAsync();

                Report14_Output_APIItem response = new Report14_Output_APIItem();
                response.SetByInput(input);

                // 使用率：使用時段數 / 查詢期間所有時段數

                int minMonth = startTime.Year * 12 + startTime.Month;
                int maxMonth = endTime.Year * 12 + endTime.Month;

                int total = await DC.Monthly_TimeSpans
                    .Where(mts => minMonth <= mts.Year * 12 + mts.Month)
                    .Where(mts => mts.Year * 12 + mts.Month <= maxMonth)
                    .Select(mts => (int?)mts.TimeSpanCt)
                    .SumAsync() ?? 0;

                response.Items = results
                    .Where(e => e.rts != null)
                    .GroupBy(e => new { e.rs.BSID })
                    .Select(e =>
                    {
                        return new Report14_Output_Row_APIItem
                        {
                            SiteType = e.Max(grouping => grouping.rs.B_SiteData.B_Category.TitleC),
                            SiteCode = e.Max(grouping => grouping.rs.B_SiteData.Code),
                            SiteName = e.Max(grouping => grouping.rs.B_SiteData.Title),
                            TimeSpan = "", // 實際報表改為無此欄位，留作向後相容
                            UseCount = e.Count(),
                            UseRate = total > 0
                                ? Decimal.Divide(e.Count(), total).ToString("P")
                                : "未設定時段數"
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
        public async Task<ActionResult> GetExcel(Report14_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            
            CommonResponseForPagedList<Report14_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "場地實際使用統計表",
                Columns = 6
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo();

            excelBuilder.CreateHeader(info);

            IDictionary<string, string> conditions = new (string name, string value)[]
                {
                    ("查詢區間",
                        new[] { input.StartDate, input.EndDate }.Distinct().Where(s => s.HasContent()).StringJoin("~")),
                    ("場地名稱", input.SiteName),
                    ("最小容納人數", input.BasicSize.ToString()),
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
                .CombineCells()
                .Align(0, HorizontalAlignment.Right)
                .SetValue(0, "*使用率=查詢期間各教室實際使用時段總數/查詢期間所有月份可用時段數加總");

            excelBuilder.StartDefineTable<Report14_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "場地類別", i => i.SiteType)
                .StringColumn(1, "場地代號", i => i.SiteCode)
                .StringColumn(2, "場地名稱", i => i.SiteName)
                .NumberColumn(3, "使用時段數", i => i.UseCount)
                .StringColumn(4, "使用率", i => i.UseRate)
                .AddToBuilder(excelBuilder);

            excelBuilder.CreateRow()
                .DrawBorder(BorderDirection.Top)
                .SetValue(0, "合計:")
                .Align(0, HorizontalAlignment.Right)
                .SetValue(1, data.AllItemCt)
                .SetValue(2, "筆");

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report14_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}