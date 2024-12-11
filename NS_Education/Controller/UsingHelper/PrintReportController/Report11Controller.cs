using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report11;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.ExcelBuild.ExcelBuilderTable;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地庫存狀況表的處理。
    /// </summary>
    public class Report11Controller : PublicClass,
        IPrintReport<Report11_Input_APIItem, IDictionary<string, string>>
    {
        private const string Type = "Type";
        private const string SiteName = "SiteName";
        private const string Time = "Time";

        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<IDictionary<string, string>>> GetResultAsync(
            Report11_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();

                // startDate 跟 endDate 不得差距超過 31 天
                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;

                // endDate 未輸入時，自動帶入今天
                DateTime endTime = input.EndDate?.ParseDateTime().Date ?? DateTime.Now.AddDays(1).Date;
                DateTime earliestTimeAllowed = endTime.AddDays(-31);

                if (startTime < earliestTimeAllowed)
                    startTime = earliestTimeAllowed;

                input.StartDate = startTime.ToFormattedStringDate();
                input.EndDate = endTime.ToFormattedStringDate();

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

                // 這邊一起處理 siteDataQuery

                var siteDataQuery = dbContext.B_SiteData
                    .Include(sd => sd.B_Category)
                    .Where(sd => !sd.DeleteFlag);

                if (input.SiteName.HasContent())
                {
                    query = query.Where(x => x.rs.B_SiteData.Title.Contains(input.SiteName));
                    siteDataQuery = siteDataQuery.Where(x => x.Title.Contains(input.SiteName));
                }

                if (input.BCID.IsAboveZero())
                {
                    query = query.Where(x => x.rs.B_SiteData.BCID == input.BCID);
                    siteDataQuery = siteDataQuery.Where(x => x.BCID == input.BCID);
                }

                if (input.IsActive.HasValue)
                {
                    query = query.Where(x => x.rs.B_SiteData.ActiveFlag == input.IsActive);
                    siteDataQuery = siteDataQuery.Where(x => x.ActiveFlag == input.IsActive);
                }

                if (input.BSCID1.IsAboveZero())
                {
                    query = query.Where(x => x.rs.B_SiteData.BSCID1 == input.BSCID1);
                    siteDataQuery = siteDataQuery.Where(x => x.BSCID1 == input.BSCID1);
                }

                if (input.BasicSize.IsAboveZero())
                {
                    query = query.Where(x => x.rs.B_SiteData.BasicSize >= input.BasicSize);
                    siteDataQuery = siteDataQuery.Where(x => x.BasicSize >= input.BasicSize);
                }

                var results = await query
                    .OrderBy(e => e.rs.RSID)
                    .ToArrayAsync();

                B_SiteData[] siteData = await siteDataQuery
                    .OrderBy(sd => sd.BSID)
                    .ToArrayAsync();

                D_TimeSpan[] timeSpans = await dbContext.D_TimeSpan
                    .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                    .ToArrayAsync();

                Report11_Output_APIItem response = new Report11_Output_APIItem();
                response.SetByInput(input);

                // 欄位：
                // Type
                // SiteName
                // Time
                // yyyy-MM-dd
                // yyyy-MM-dd
                foreach (B_SiteData sd in siteData)
                {
                    foreach (D_TimeSpan dts in timeSpans)
                    {
                        IDictionary<string, string> newRow = new Dictionary<string, string>();
                        response.Items.Add(newRow);

                        newRow.Add(Type, sd.B_Category.TitleC);
                        newRow.Add(SiteName, sd.Title);
                        newRow.Add(Time, dts.Title);

                        foreach (DateTime dt in startTime.DayRange(endTime))
                        {
                            newRow.Add(dt.ToFormattedStringDate(),
                                results
                                    .Where(g => g.rs.BSID == sd.BSID)
                                    .Where(g => g.rts.DTSID == dts.DTSID)
                                    .Where(g => g.rs.TargetDate.Date == dt.Date)
                                    .Select(g => g.rs.Resver_Head.Customer?.TitleC ?? "")
                                    .FirstOrDefault());
                        }
                    }
                }

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                // 這個端點的回傳物件是用 dictionary 實作的類似 dynamic 的東西
                // 所以用 sortWithInput 會需要特殊處理

                if (input.Sorting != null)
                {
                    IOrderedEnumerable<IDictionary<string, string>> orderedEnumerable =
                        response.Items.AsOrderedEnumerable();

                    foreach (ListSorting sorting in input.Sorting)
                    {
                        orderedEnumerable = sorting.IsAscending
                            ? orderedEnumerable.ThenBy(e => e.GetValueOrDefault(sorting.PropertyName))
                            : orderedEnumerable.ThenByDescending(e => e.GetValueOrDefault(sorting.PropertyName));
                    }

                    response.Items = orderedEnumerable.ToList();
                }


                response.Items = response.Items
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                return response;
            }
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report11_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            
            CommonResponseForPagedList<IDictionary<string, string>> data = await GetResultAsync(input);

            // 轉換 items, 確保有正確排序
            IEnumerable<KeyValuePair<string, string>>[] items = data.Items
                .Select(dict => dict.Select(kvp => kvp))
                .OrderBy(dict => dict.FirstOrDefault(kvp => kvp.Key == Type).Value)
                .ThenBy(dict => dict.FirstOrDefault(kvp => kvp.Key == SiteName).Value)
                .ThenBy(dict => dict.FirstOrDefault(kvp => kvp.Key == Time).Value)
                .ToArray();

            // 日期是動態長的
            IEnumerable<string> dateKeys = items
                                               .FirstOrDefault()?
                                               .Select(kvp => kvp.Key)
                                               .Where(k => k.All(c => Char.IsDigit(c) || c == '/'))
                                               .ToArray()
                                           ?? Array.Empty<string>();

            ExcelBuilderInfo info = await GetExcelBuilderInfo();
            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "場地庫存狀況表",
                // 這個表會隨著天數動態長, 所以要特殊計算
                Columns = Math.Max(3 + dateKeys.Count(), 6)
            };

            excelBuilder.CreateHeader(info);

            string[] conditions = new[]
                {
                    new[] { input.StartDate, input.EndDate }
                        .Distinct()
                        .Where(s => s.HasContent())
                        .StringJoin("~"),
                    input.SiteName.HasContent() ? $"場地名稱={input.SiteName}" : null,
                    input.IsActive.HasValue ? $"是否啟用={input.IsActive}" : null,
                    input.BasicSize.HasValue ? $"容納人數>={input.BasicSize}" : null
                }
                .Where(s => s.HasContent())
                .ToArray();

            excelBuilder.CreateRow();

            if (conditions.Any())
            {
                excelBuilder.NowRow()
                    .SetValue(0, "查詢條件:");

                foreach (string condition in conditions)
                {
                    excelBuilder.NowRow()
                        .SetValue(1, condition);

                    excelBuilder.CreateRow();
                }
            }

            // Type: 場地類別
            // SiteName: 場地名稱
            // Time: 時段
            // yyyy-MM-dd: 已預約客戶名稱

            TableDefinition<IEnumerable<KeyValuePair<string, string>>> table =
                excelBuilder.StartDefineTable<IEnumerable<KeyValuePair<string, string>>>();

            table.SetDataRows(items)
                .StringColumn(0, "場地類別", g => g.FirstOrDefault(kvp => kvp.Key == Type).Value)
                .StringColumn(1, "場地名稱", g => g.FirstOrDefault(kvp => kvp.Key == SiteName).Value)
                .StringColumn(2, "時段", g => g.FirstOrDefault(kvp => kvp.Key == Time).Value);

            // 日期是動態長進 dictionary 的, 所以也要動態定義 column

            int cellNo = 3;
            foreach (string dateKey in dateKeys)
            {
                table.StringColumn(cellNo, dateKey, g => g.FirstOrDefault(kvp => kvp.Key == dateKey).Value);
                cellNo++;
            }

            table.AddToBuilder(excelBuilder);

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report11_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}