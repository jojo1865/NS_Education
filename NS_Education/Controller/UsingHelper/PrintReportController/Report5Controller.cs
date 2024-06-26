using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report5;
using NS_Education.Models.Entities;
using NS_Education.Models.Utilities.PrintReport;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 餐飲明細表的處理。
    /// </summary>
    public class Report5Controller : PublicClass, IPrintReport<Report5_Input_APIItem, Report5_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report5_Output_Row_APIItem>> GetResultAsync(
            Report5_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startDate = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endDate = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Throw_Food
                    .Include(rtf => rtf.Resver_Throw)
                    .Include(rtf => rtf.Resver_Throw.Resver_Site)
                    .Include(rtf => rtf.Resver_Throw.Resver_Site.Resver_Head)
                    .Include(rtf => rtf.Resver_Throw.Resver_Site.Resver_Head.Customer)
                    .Include(rtf => rtf.B_Partner)
                    .Include(rtf => rtf.B_StaticCode)
                    .Include(rtf => rtf.D_FoodCategory)
                    .Where(rtf => !rtf.Resver_Throw.DeleteFlag)
                    .Where(rtf => !rtf.Resver_Throw.Resver_Site.DeleteFlag)
                    .Where(rtf => !rtf.Resver_Throw.Resver_Site.Resver_Head.DeleteFlag)
                    .Where(rtf => startDate <= rtf.Resver_Throw.TargetDate && rtf.Resver_Throw.TargetDate <= endDate)
                    .AsQueryable();

                if (input.RHID != null)
                    query = query.Where(rtf => input.RHID.Contains(rtf.Resver_Throw.Resver_Site.RHID));

                if (input.Partner.HasContent())
                    query = query
                        .Where(rtf => rtf.B_Partner.Title.Contains(input.Partner)
                                      || rtf.B_Partner.Compilation.Contains(input.Partner));

                if (input.CustomerName.HasContent())
                    query = query
                        .Where(rtf =>
                            rtf.Resver_Throw.Resver_Site.Resver_Head.Customer.TitleC.Contains(input.CustomerName));

                // 刪除的資料 State 不會變，所以要做特別處理
                if (input.State == (int)ReserveHeadGetListState.Deleted)
                    query = query.Where(rtf => rtf.Resver_Throw.Resver_Site.Resver_Head.DeleteFlag);
                else if (input.State.IsAboveZero())
                    query = query.Where(rtf => rtf.Resver_Throw.Resver_Site.Resver_Head.State == input.State);

                var results = await query
                    .OrderBy(rtf => rtf.Resver_Throw.TargetDate)
                    .ToArrayAsync();

                Report5_Output_APIItem response = new Report5_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(rtf => new Report5_Output_Row_APIItem
                {
                    ReserveDate = rtf.Resver_Throw.TargetDate.ToString("yy/MM/dd"),
                    RHID = rtf.Resver_Throw.Resver_Site.RHID,
                    EventName = rtf.Resver_Throw.Resver_Site.Resver_Head.Title,
                    PartnerName = rtf.B_Partner.Title,
                    CuisineType = rtf.B_StaticCode.Title,
                    CuisineName = rtf.D_FoodCategory.Title,
                    HostName = rtf.Resver_Throw.Resver_Site.Resver_Head.Customer.TitleC,
                    ReservedQuantity = rtf.Ct,
                    UnitPrice = rtf.UnitPrice,
                    QuotedPrice = rtf.Price
                }).ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.SortWithInput(input).Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report5_Input_APIItem input)
        {
            // 匯出時忽略頁數篩選
            input.NowPage = 0;

            CommonResponseForPagedList<Report5_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data is null)
                return GetContentResult();

            ExcelBuilderInfo info = await GetExcelBuilderInfo();
            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "餐飲明細表",
                Columns = 10
            };

            excelBuilder.CreateHeader(info);

            // 組合查詢條件的文字
            IEnumerable<string> conditions = new[]
                {
                    input.RHID?.Any() ?? false ? $"預約單號={String.Join(",", input.RHID)}" : null,
                    String.Join("~", new[] { input.StartDate, input.EndDate }.Where(d => d.HasContent()).Distinct()),
                    input.CustomerName.HasContent() ? $"主辦單位={input.CustomerName}" : null,
                    input.Partner.HasContent() ? $"廠商={input.Partner}" : null
                }.Where(s => s.HasContent())
                .ToArray();

            if (conditions.Any())
            {
                excelBuilder.CreateRow()
                    .SetValue(0, "查詢條件:");

                foreach (string condition in conditions)
                {
                    excelBuilder.NowRow()
                        .CombineCells(1, 9)
                        .SetValue(1, condition);

                    excelBuilder.CreateRow();
                }
            }

            excelBuilder.StartDefineTable<Report5_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "活動日期", i => i.ReserveDate)
                .StringColumn(1, "預約單號", i => i.RHID.ToString())
                .StringColumn(2, "活動名稱", i => i.EventName)
                .StringColumn(3, "廠商名稱", i => i.PartnerName)
                .StringColumn(4, "餐別", i => i.CuisineType)
                .StringColumn(5, "餐種", i => i.CuisineName)
                .StringColumn(6, "主辦單位", i => i.HostName)
                .NumberColumn(7, "數量", i => i.ReservedQuantity)
                .NumberColumn(8, "單價", i => i.UnitPrice)
                .NumberColumn(9, "總價", i => i.UnitPriceSum, true)
                .AddToBuilder(excelBuilder);

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report5_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<FileContentResult> GetPdf(Report5_Input_APIItem input)
        {
            CommonResponseForPagedList<Report5_Output_Row_APIItem> data = await GetResultAsync(input);

            byte[] pdf = data.MakePdf(input,
                GetUid(),
                await GetUserNameByID(GetUid()),
                "餐飲明細表",
                new[]
                {
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "預約日期",
                        LengthWeight = 4,
                        Selector = r => r.ReserveDate,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "預約單號",
                        LengthWeight = 4,
                        Selector = r => r.RHID,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "活動名稱",
                        LengthWeight = 8,
                        Selector = r => r.EventName,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "廠商名稱",
                        LengthWeight = 8,
                        Selector = r => r.PartnerName,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "餐種",
                        LengthWeight = 2,
                        Selector = r => r.CuisineType,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "餐種名稱",
                        LengthWeight = 6,
                        Selector = r => r.CuisineName,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "主辦單位",
                        LengthWeight = 8,
                        Selector = r => r.HostName,
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "數量",
                        LengthWeight = 4,
                        Selector = r => r.ReservedQuantity,
                        Formatter = qty => $"{qty:N0}",
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "成本單價",
                        LengthWeight = 4,
                        Selector = r => r.UnitPrice,
                        Formatter = price => $"{price:N0}",
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "成本總價",
                        LengthWeight = 4,
                        Selector = r => r.UnitPriceSum,
                        Formatter = price => $"{price:N0}",
                        OutputTotal = true
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "報價單價",
                        LengthWeight = 4,
                        Selector = r => r.QuotedPrice,
                        Formatter = price => $"{price:N0}",
                        OutputTotal = false
                    },
                    new PdfColumn<Report5_Output_Row_APIItem>
                    {
                        Name = "報價總價",
                        LengthWeight = 4,
                        Selector = r => r.QuotedPriceSum,
                        Formatter = price => $"{price:N0}",
                        OutputTotal = true
                    }
                });

            return new FileContentResult(pdf, "application/pdf");
        }
    }
}