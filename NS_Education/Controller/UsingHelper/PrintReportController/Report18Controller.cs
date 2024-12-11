using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report18;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 日誌贈送報表的處理。
    /// </summary>
    public class Report18Controller : PublicClass, IPrintReport<Report18_Input_APIItem, Report18_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report18_Output_Row_APIItem>> GetResultAsync(
            Report18_Input_APIItem input)
        {
            var query = DC.M_Customer_Gift
                .Include(mcg => mcg.Customer)
                .Include(mcg => mcg.GiftSending)
                .Include(mcg => mcg.GiftSending.B_StaticCode)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(mcg => mcg.GiftSending.B_StaticCode.Title.Contains(input.Keyword));

            if (input.CustomerTitleC.HasContent())
                query = query.Where(mcg => mcg.Customer.TitleC.Contains(input.CustomerTitleC));

            if (input.SendYear.IsAboveZero())
                query = query.Where(mcg => mcg.GiftSending.Year == input.SendYear);

            if (input.SDate.TryParseDateTime(out DateTime startDate))
                query = query.Where(mcg => DbFunctions.TruncateTime(mcg.GiftSending.SendDate) >= startDate.Date);

            if (input.EDate.TryParseDateTime(out DateTime endDate))
                query = query.Where(mcg => DbFunctions.TruncateTime(mcg.GiftSending.SendDate) <= endDate.Date);

            query = query.OrderByDescending(mcg => mcg.GiftSending.SendDate)
                .ThenBy(mcg => mcg.CID)
                .ThenBy(mcg => mcg.GSID);

            var results = await query.ToArrayAsync();

            Report18_Output_APIItem response = new Report18_Output_APIItem();
            response.SetByInput(input);

            response.Items = results
                .Select(mcg => new Report18_Output_Row_APIItem
                {
                    SendDate = mcg.GiftSending.SendDate.ToFormattedStringDate(),
                    C_TitleC = mcg.Customer?.TitleC ?? "",
                    Title = mcg.GiftSending.B_StaticCode?.Title ?? "",
                    Ct = mcg.Ct
                })
                .ToList();

            response.UID = GetUid();
            response.Username = await GetUserNameByID(response.UID);

            response.AllItemCt = response.Items.Count;

            response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();

            return response;
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report18_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            
            CommonResponseForPagedList<Report18_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "日誌贈送報表",
                Columns = 6
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo();

            excelBuilder.CreateHeader(info);

            IDictionary<string, string> conditions = new (string name, string value)[]
                {
                    ("查詢區間:",
                        new[] { input.SDate, input.EDate }.Distinct().Where(s => s.HasContent()).StringJoin("~")),
                    ("客戶名稱:", input.CustomerTitleC),
                    ("年份:", input.SendYear.ToString()),
                    ("禮品:", input.Keyword)
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

            excelBuilder.StartDefineTable<Report18_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "贈送日期", i => i.SendDate)
                .StringColumn(1, "客戶", i => i.C_TitleC)
                .StringColumn(3, "品項", i => i.Title)
                .NumberColumn(5, "數量", i => i.Ct)
                .AddToBuilder(excelBuilder);

            excelBuilder.CreateRow()
                .DrawBorder(BorderDirection.Top)
                .SetValue(0, "合計:")
                .SetValue(1, data.Items.Count)
                .SetValue(2, "筆")
                .Align(1, HorizontalAlignment.Right);

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report18_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}