using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report8;
using NS_Education.Models.Entities;
using NS_Education.Models.Utilities.PrintReport;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using QuestPDF.Helpers;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 滿意度調查表報表的處理。
    /// </summary>
    public class Report8Controller : PublicClass, IPrintReport<Report8_Input_APIItem, Report8_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report8_Output_Row_APIItem>> GetResultAsync(
            Report8_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime().Date.AddDays(1) ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.B_SiteData
                        .Include(bs => bs.Resver_Site)
                        .Include(bs => bs.Resver_Site.Select(rs => rs.Resver_Head))
                        .Include(
                            bs => bs.Resver_Site.Select(rs => rs.Resver_Head).Select(rh => rh.Resver_Questionnaire))
                        .Where(bs => bs.ActiveFlag)
                        .Where(bs => !bs.DeleteFlag)
                    ;

                var results = (await query
                        .ToArrayAsync())
                    .Where(bs => bs.Resver_Site.Any())
                    .ToArray();

                // 取得資料，排除掉已刪除/未啟用的預約單，並且確實有 RQ 的
                // 然後依據 RS 數量排序

                foreach (B_SiteData bs in results)
                {
                    bs.Resver_Site = bs.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Where(rs => !rs.Resver_Head.DeleteFlag)
                        .Where(rs => startTime <= rs.TargetDate)
                        .Where(rs => rs.TargetDate <= endTime)
                        .Where(rs => rs.Resver_Head.Resver_Questionnaire.Any())
                        .ToList();
                }

                Report8_Output_APIItem response = new Report8_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(bs =>
                    {
                        ILookup<string, Resver_Questionnaire> questionnaires = bs.Resver_Site
                            .Select(rs => rs.Resver_Head)
                            .SelectMany(rh => rh.Resver_Questionnaire)
                            .ToLookup(rq => rq.QuestionKey, rq => rq);

                        IEnumerable<Resver_Questionnaire> willUseAgainRows = questionnaires
                            .GetValueOrDefault("WillUseAgain") ?? Array.Empty<Resver_Questionnaire>();

                        int willUseAgainCount = willUseAgainRows
                            .Count(rq => rq.TextContent == "Y");

                        int totalRentCount = bs.Resver_Site
                            .Select(rs => rs.Resver_Head)
                            .DistinctBy(rh => rh.RHID)
                            .Count();

                        decimal percentage =
                            totalRentCount <= 0 ? 0 : Decimal.Divide(willUseAgainCount, totalRentCount);

                        return new Report8_Output_Row_APIItem
                        {
                            SiteName = bs.Title ?? "",
                            SiteCode = bs.Code ?? "",
                            RentCt = totalRentCount,
                            SiteSatisfied = GetQuestionnaireDictionary(questionnaires, "SiteSatisfied"),
                            DeviceSatisfied = GetQuestionnaireDictionary(questionnaires, "DeviceSatisfied"),
                            CleanSatisfied = GetQuestionnaireDictionary(questionnaires, "CleanSatisfied"),
                            NegotiatorSatisfied = GetQuestionnaireDictionary(questionnaires, "NegotiatorSatisfied"),
                            ServiceSatisfied = GetQuestionnaireDictionary(questionnaires, "ServiceSatisfied"),
                            MealSatisfied = GetQuestionnaireDictionary(questionnaires, "MealSatisfied"),
                            DessertSatisfied = GetQuestionnaireDictionary(questionnaires, "DessertSatisfied"),
                            WillUseAgainPercentage = percentage.ToString("P2")
                        };
                    })
                    .OrderByDescending(i => i.RentCt)
                    .ThenBy(i => i.SiteName)
                    .ThenBy(i => i.SiteCode)
                    .ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.SortWithInput(input).Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        private static Dictionary<int, int> GetQuestionnaireDictionary(
            ILookup<string, Resver_Questionnaire> questionnaires, string key)
        {
            IEnumerable<Resver_Questionnaire> values =
                questionnaires.GetValueOrDefault(key) ?? Array.Empty<Resver_Questionnaire>();

            Dictionary<int, int> result = new Dictionary<int, int>();

            // 固定 populate 1~5
            for (int i = 1; i <= 5; i++)
            {
                if (!result.ContainsKey(i))
                    result[i] = 0;
            }

            // 寫入實際的值
            foreach (IGrouping<int, Resver_Questionnaire> grouping in values.GroupBy(rq => rq.NumberContent ?? 0))
            {
                result[grouping.Key] = grouping.Count();
            }

            return result;
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report8_Input_APIItem input)
        {
            // 以場地為 group key 的端點 (報表分系, 舊 PDF)
            // 前端的滿意~不滿意是相反的, 所以這裡需要做調整
            CommonResponseForPagedList<Report8_Output_Row_APIItem> result = await GetResultAsync(input);

            foreach (Report8_Output_Row_APIItem item in result.Items)
            {
                Invert(item.SiteSatisfied);
                Invert(item.ServiceSatisfied);
                Invert(item.CleanSatisfied);
                Invert(item.DessertSatisfied);
                Invert(item.DeviceSatisfied);
                Invert(item.MealSatisfied);
                Invert(item.NegotiatorSatisfied);
            }

            return GetResponseJson(result);
        }

        private void Invert(IDictionary<int, int> satisfied)
        {
            (satisfied[1], satisfied[5]) = (satisfied[5], satisfied[1]);
            (satisfied[2], satisfied[4]) = (satisfied[4], satisfied[2]);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetPdf(Report8_Input_APIItem input)
        {
            CommonResponseForPagedList<Report8_Output_Row_APIItem> data = await GetResultAsync(input);

            ICollection<KeyValuePair<string, ICollection<PdfColumn<Report8_Output_Row_APIItem>>>> tableDefinitions =
                new List<KeyValuePair<string, ICollection<PdfColumn<Report8_Output_Row_APIItem>>>>();

            AddColumnDefinition(tableDefinitions, "場地空間", r => r.SiteSatisfied);
            AddColumnDefinition(tableDefinitions, "視聽設備", r => r.DeviceSatisfied);
            AddColumnDefinition(tableDefinitions, "環境清潔", r => r.CleanSatisfied);
            AddColumnDefinition(tableDefinitions, "洽談人員", r => r.NegotiatorSatisfied);
            AddColumnDefinition(tableDefinitions, "服務人員", r => r.ServiceSatisfied);
            AddColumnDefinition(tableDefinitions, "午、晚餐", r => r.MealSatisfied);
            AddColumnDefinition(tableDefinitions, "茶點", r => r.DessertSatisfied);

            byte[] pdf = data.MakeMultiTablePdf(GetUid(),
                await GetUserNameByID(GetUid()),
                tableDefinitions,
                $"\n日期區間={input.StartDate} - {input.EndDate}",
                PageSizes.A4.Landscape());

            return new FileContentResult(pdf, "application/pdf");
        }

        private static void AddColumnDefinition(
            ICollection<KeyValuePair<string, ICollection<PdfColumn<Report8_Output_Row_APIItem>>>> tables,
            string fieldName, Func<Report8_Output_Row_APIItem, IDictionary<int, int>> scoreSelector)
        {
            // 一個 fieldName 一個 table
            ICollection<PdfColumn<Report8_Output_Row_APIItem>>
                table = new List<PdfColumn<Report8_Output_Row_APIItem>>();

            KeyValuePair<string, ICollection<PdfColumn<Report8_Output_Row_APIItem>>> tableDefinition =
                new KeyValuePair<string, ICollection<PdfColumn<Report8_Output_Row_APIItem>>>($"場地滿意調查表\n（{fieldName}）",
                    table);

            table.Add(new PdfColumn<Report8_Output_Row_APIItem>
            {
                Name = "場地",
                LengthWeight = 6,
                Selector = r => r.SiteName,
                OutputTotal = false
            });
            table.Add(new PdfColumn<Report8_Output_Row_APIItem>
            {
                Name = "編號",
                LengthWeight = 3,
                Selector = r => r.SiteCode,
                OutputTotal = false
            });
            table.Add(new PdfColumn<Report8_Output_Row_APIItem>
            {
                Name = "總次數",
                LengthWeight = 3,
                Selector = r => r.RentCt,
                OutputTotal = true
            });

            string[] scoreNames = { "很滿意", "滿意", "可", "不滿意", "很不滿意" };
            for (int i = 0; i <= 4; i++)
            {
                int j = 5 - i;
                table.Add(new PdfColumn<Report8_Output_Row_APIItem>
                {
                    Name = scoreNames[i],
                    LengthWeight = 3,
                    // 這是 delegate, 所以不能直接寫 5-i 在這裡
                    Selector = r => scoreSelector(r)[j],
                    OutputTotal = true
                });
            }

            table.Add(new PdfColumn<Report8_Output_Row_APIItem>
            {
                Name = "願意再度預約",
                LengthWeight = 4,
                Selector = r => r.WillUseAgainPercentage,
                OutputTotal = false
            });

            tables.Add(tableDefinition);
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report8_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            
            // 這張報表的 PDF/報表分析 格式和後來取得的正式樣張不同
            // 而最後報表匯出定案統一都用 Excel
            // 所以舊的目前不動, 新的 Excel 需要獨立寫查詢
            IEnumerable<Report8_Output_Excel_Row_APIItem> data = await GetExcelRows(input);

            data = data?.ToArray();

            if (data == null)
                return GetContentResult();

            ExcelBuilderInfo info = await GetExcelBuilderInfo();
            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "滿意度調查表",
                Columns = 13
            };

            excelBuilder.CreateHeader(info);

            string dateRange = String.Join("~", new[] { input.StartDate, input.EndDate }
                .Where(d => d.HasContent())
                .Distinct());

            if (dateRange.HasContent())
            {
                excelBuilder.CreateRow()
                    .SetValue(0, "查詢條件:")
                    .SetValue(1, dateRange);
            }

            excelBuilder.CreateRow();

            excelBuilder.StartDefineTable<Report8_Output_Excel_Row_APIItem>()
                .SetDataRows(data)
                .StringColumn(0, "活動日期", i => i.EventDate)
                .StringColumn(1, "客戶名稱", i => i.CustomerTitle)
                .NumberColumn(2, "一-1", i => i.A1)
                .NumberColumn(3, "一-2", i => i.A2)
                .NumberColumn(4, "一-3", i => i.A3)
                .NumberColumn(5, "二-1", i => i.B1)
                .NumberColumn(6, "二-2", i => i.B2)
                .NumberColumn(7, "三-1", i => i.C1)
                .NumberColumn(8, "三-2", i => i.C2)
                .NumberColumn(9, "四-1", i => i.D1 ? 1 : 0)
                .StringColumn(10, "MK", i => i.MK)
                .StringColumn(11, "OP", i => i.OP)
                .NumberColumn(12, "月份", i => i.Month)
                .AddToBuilder(excelBuilder);

            excelBuilder.CreateRow()
                .DrawBorder(BorderDirection.Top)
                .SetValue(0, "合計:")
                .SetValue(1, data.Count())
                .Align(0, HorizontalAlignment.Right)
                .Align(1, HorizontalAlignment.Left);

            return excelBuilder.GetFile();
        }

        private async Task<IEnumerable<Report8_Output_Excel_Row_APIItem>> GetExcelRows(Report8_Input_APIItem input)
        {
            DateTime startDate = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
            DateTime endDate = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

            startDate = startDate.Date;
            endDate = endDate.Date;

            ILookup<int, Resver_Questionnaire> results = (await DC.Resver_Questionnaire
                    .Include(rq => rq.Resver_Head)
                    .Include(rq => rq.Resver_Head.BusinessUser)
                    .Include(rq => rq.Resver_Head.BusinessUser1)
                    .Include(rq => rq.Resver_Head.Customer)
                    .Where(rq => !rq.Resver_Head.DeleteFlag)
                    .Where(rq => startDate <= rq.Resver_Head.SDate)
                    .Where(rq => rq.Resver_Head.EDate <= endDate)
                    .ToArrayAsync())
                .ToLookup(r => r.RHID, r => r);


            return results
                .Select(g =>
                {
                    string willUseAgain = g.FirstOrDefault(rq => rq.QuestionKey == "WillUseAgain")?.TextContent;

                    return new Report8_Output_Excel_Row_APIItem
                    {
                        EventDate = g.Max(rq => rq.Resver_Head.SDate).ToFormattedStringDate(),
                        CustomerTitle = g.Max(rq => rq.Resver_Head.Customer.TitleC),
                        A1 = GetScores(g, "SiteSatisfied"),
                        A2 = GetScores(g, "DeviceSatisfied"),
                        A3 = GetScores(g, "CleanSatisfied"),
                        B1 = GetScores(g, "NegotiatorSatisfied"),
                        B2 = GetScores(g, "ServiceSatisfied"),
                        C1 = GetScores(g, "MealSatisfied"),
                        C2 = GetScores(g, "DessertSatisfied"),
                        D1 = "Y".Equals(willUseAgain, StringComparison.InvariantCultureIgnoreCase)
                             || "True".Equals(willUseAgain, StringComparison.InvariantCultureIgnoreCase),
                        MK = g.Max(rq => rq.Resver_Head.BusinessUser.Name),
                        OP = g.Max(rq => rq.Resver_Head.BusinessUser1.Name),
                        Month = g.Max(rq => rq.Resver_Head.SDate.Month)
                    };
                });
        }

        private static int GetScores(IGrouping<int, Resver_Questionnaire> g, string questionKey)
        {
            int score = g.Where(rq => rq.QuestionKey == questionKey)
                .Select(rq => rq.NumberContent)
                .Where(i => i.HasValue)
                .Max() ?? 3;

            return score;
        }

        #endregion
    }
}