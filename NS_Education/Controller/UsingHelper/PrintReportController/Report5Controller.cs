using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report5;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
                    HostName = rtf.Resver_Throw.Resver_Site.Resver_Head.CustomerTitle,
                    ReservedQuantity = rtf.Ct,
                    UnitPrice = rtf.UnitPrice,
                    QuotedPrice = rtf.Price
                }).ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

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

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // basic
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(ts =>
                        ts.FontFamily("Microsoft JhengHei UI")
                            .Black()
                            .Bold()
                            .FontSize(10));

                    // header
                    page.Header()
                        .Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .AlignLeft()
                                    .Text(async t =>
                                    {
                                        t.AlignLeft();
                                        t.Line($"製表者 ID: {GetUid()}").FontSize(16).Bold()
                                            .FontColor(Colors.Purple.Darken2);
                                        t.Line($"製表者: {Task.Run(() => GetUserNameByID(GetUid())).Result}")
                                            .FontSize(16).Bold().FontColor(Colors.Purple.Darken2);
                                        t.Line($"查詢條件: bla bla bla").FontSize(16).Bold()
                                            .FontColor(Colors.Grey.Medium);
                                    });

                                row.RelativeItem()
                                    .AlignCenter()
                                    .Text(t =>
                                    {
                                        ;
                                        t.AlignCenter();
                                        t.Line("南山人壽教育訓練中心").FontSize(26).Bold().FontColor(Colors.Cyan.Darken2);
                                        t.EmptyLine();
                                        t.Line("餐飲明細表").FontSize(26).Bold().Black();
                                    });

                                row.RelativeItem()
                                    .AlignRight()
                                    .Text(t =>
                                    {
                                        t.AlignLeft();
                                        t.Line($"製表日: {DateTime.Now.ToFormattedStringDate()}").FontSize(16).Bold()
                                            .FontColor(Colors.Purple.Darken2);
                                        t.CurrentPageNumber().Format(i => $"頁次: {i ?? 0}").FontSize(16).Bold()
                                            .FontColor(Colors.Purple.Darken2);
                                    });
                            });
                        });

                    page.Content()
                        .PaddingTop(0)
                        .Section("mainTable")
                        .Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn(4);
                                cd.RelativeColumn(4);
                                cd.RelativeColumn(8);
                                cd.RelativeColumn(8);
                                cd.RelativeColumn(2);
                                cd.RelativeColumn(6);
                                cd.RelativeColumn(8);
                                cd.RelativeColumn(2);
                                cd.RelativeColumn(4);
                                cd.RelativeColumn(4);
                                cd.RelativeColumn(4);
                                cd.RelativeColumn(4);
                            });

                            string[] columns = new[]
                            {
                                "預約日期", "預約單號", "活動名稱", "廠商名稱", "餐種", "餐種名稱", "主辦單位", "數量", "成本單價", "成本總價", "報價單價",
                                "報價總價"
                            };

                            Func<Report5_Output_Row_APIItem, object>[] selectors =
                            {
                                r => r.ReserveDate,
                                r => r.RHID,
                                r => r.EventName,
                                r => r.PartnerName,
                                r => r.CuisineType,
                                r => r.CuisineName,
                                r => r.HostName,
                                r => r.ReservedQuantity.ToString("N0"),
                                r => r.UnitPrice.ToString("N0"),
                                r => r.UnitPriceSum.ToString("N0"),
                                r => r.QuotedPrice.ToString("N0"),
                                r => r.QuotedPriceSum.ToString("N0")
                            };

                            table.Header(header =>
                            {
                                foreach (string column in columns)
                                {
                                    header.Cell()
                                        .Column(c =>
                                        {
                                            c.Item().Text(column);
                                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                                        });
                                }
                            });

                            foreach (Report5_Output_Row_APIItem row in data.Items)
                            {
                                foreach (Func<Report5_Output_Row_APIItem, object> selector in selectors)
                                {
                                    table.Cell()
                                        .Column(c => { c.Item().Text(selector.Invoke(row).ToString()); });
                                }
                            }

                            table.Footer(f =>
                            {
                                f.Cell()
                                    .Column(c =>
                                    {
                                        c.Item().LineHorizontal(1).LineColor(Colors.Black);
                                        c.Item().Text("合計: ");
                                    });

                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c =>
                                {
                                    c.Item().LineHorizontal(1).LineColor(Colors.Black);
                                    c.Item().Text(data.Items.Sum(i => i.UnitPriceSum).ToString("N0"));
                                });
                                f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                f.Cell().Column(c =>
                                {
                                    c.Item().LineHorizontal(1).LineColor(Colors.Black);
                                    c.Item().Text(data.Items.Sum(i => i.QuotedPriceSum).ToString("N0"));
                                });
                            });
                        });
                });
            });

            byte[] pdf = document.GeneratePdf();
            return new FileContentResult(pdf, "application/pdf");
        }
    }
}