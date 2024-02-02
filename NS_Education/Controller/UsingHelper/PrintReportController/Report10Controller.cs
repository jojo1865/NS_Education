using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report10;
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
    /// 未成交原因分析的處理。
    /// </summary>
    public class Report10Controller : PublicClass, IPrintReport<Report10_Input_APIItem, Report10_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report10_Output_Row_APIItem>> GetResultAsync(
            Report10_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                var query = dbContext.CustomerVisit
                    .Include(cv => cv.Customer)
                    .Include(cv => cv.Customer.Resver_Head)
                    .Include(cv => cv.B_StaticCode)
                    .Include(cv => cv.B_StaticCode1)
                    .Include(cv => cv.BusinessUser)
                    .Where(cv => !cv.DeleteFlag)
                    .AsQueryable();

                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime().Date ?? SqlDateTime.MaxValue.Value;

                // 篩選未成交的客戶
                query = query.Where(cv => cv.Customer.Resver_Head
                    .AsQueryable()
                    .Where(ResverHeadExpression.IsDealtExpression)
                    .Any() == false);

                query = query.Where(cv => startTime <= cv.VisitDate)
                    .Where(cv => cv.VisitDate <= endTime);

                if (input.CID.HasValue)
                    query = query.Where(cv => cv.CID == input.CID);

                if (input.BSCID.HasValue)
                    query = query.Where(cv => cv.BSCID == input.BSCID);

                var results = await query
                    .OrderByDescending(cv => cv.VisitDate)
                    .ThenBy(cv => cv.CVID)
                    .ToArrayAsync();

                Report10_Output_APIItem response = new Report10_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(cv => new Report10_Output_Row_APIItem
                    {
                        CustomerCode = cv.Customer.Code,
                        CustomerName = cv.Customer.TitleC,
                        Contact = cv.Customer.ContectName,
                        TargetTitle = cv.TargetTitle,
                        VisitMethod = cv.B_StaticCode1?.Title ?? "",
                        VisitDate = cv.VisitDate.ToFormattedStringDate(),
                        Agent = cv.BusinessUser?.Name ?? "",
                        Title = cv.Title,
                        Description = cv.Description,
                        AfterNote = cv.AfterNote ?? "",
                        NoDealReason = cv.B_StaticCode?.Title ?? "未設定"
                    })
                    .SortWithInput(input)
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();
                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = results.Count();
                return response;
            }
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report10_Input_APIItem input)
        {
            CommonResponseForPagedList<Report10_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilderInfo info = await GetExcelBuilderInfo();
            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "未成交原因分析",
                Columns = 9
            };

            excelBuilder.CreateHeader(info);

            string dateRange = new[] { input.StartDate, input.EndDate }
                .Distinct()
                .Where(s => s.HasContent())
                .StringJoin("~");

            if (dateRange.HasContent())
            {
                excelBuilder.CreateRow()
                    .SetValue(0, "查詢條件:")
                    .SetValue(1, "查詢日期")
                    .SetValue(2, dateRange);
            }

            excelBuilder.CreateRow();

            excelBuilder.StartDefineTable<Report10_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "客戶代號", i => i.CustomerCode)
                .StringColumn(1, "聯絡人", i => i.Contact)
                .StringColumn(2, "客戶名稱", i => i.CustomerName)
                .StringColumn(3, "拜訪方式", i => i.VisitMethod)
                .StringColumn(4, "拜訪日期", i => i.VisitDate)
                .StringColumn(5, "MK", i => i.Agent)
                .StringColumn(6, "主旨", i => i.Title)
                .StringColumn(7, "內容(摘要)", i => i.Description)
                .StringColumn(8, "後續追蹤", i => i.AfterNote)
                .AddToBuilder(excelBuilder);

            excelBuilder.CreateRow()
                .DrawBorder(BorderDirection.Top)
                .SetValue(0, "合計:")
                .Align(0, HorizontalAlignment.Right)
                .SetValue(1, data.Items.Count, CellType.Numeric)
                .Align(1, HorizontalAlignment.Right)
                .SetValue(2, "筆");

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report10_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}