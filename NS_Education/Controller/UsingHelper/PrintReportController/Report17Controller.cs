using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report17;
using NS_Education.Models.Entities;
using NS_Education.Models.Utilities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 對帳單的處理。
    /// </summary>
    public class Report17Controller : PublicClass, IPrintReport<Report17_Input_APIItem, Report17_Output_APIItem>
    {
        #region 報表

        public async Task<FileContentResult> GetPdf(Report17_Input_APIItem input)
        {
            Report17_Output_APIItem data = await GetResult(input);

            if (HasError())
                throw new Exception($"產表失敗：{String.Join(" \n", _errors.Select(e => e.ErrorMessage))}");

            string userName = await GetUserNameByID(GetUid());
            Document document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // basic
                    page.Size(PageSizes.A4);
                    page.Margin(0.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(ts =>
                        ts.FontFamily("Noto Sans TC")
                            .Black()
                            .ExtraBold()
                            .FontSize(12)
                            .LineHeight(0.9f));

                    // header
                    page.Header()
                        .Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .AlignTop()
                                    .AlignLeft()
                                    .Text(t =>
                                    {
                                        t.AlignLeft();
                                        t.Line($"製表者ID： {GetUid()}")
                                            .FontSize(10)
                                            .FontColor(Colors.Purple.Darken2);
                                        t.Span($"服務人員： {userName}")
                                            .FontSize(10)
                                            .FontColor(Colors.Purple.Darken2);
                                    });

                                row.RelativeItem()
                                    .AlignTop()
                                    .AlignCenter()
                                    .Text(t =>
                                    {
                                        t.ParagraphSpacing(1);
                                        t.AlignCenter();
                                        t.Line("南山人壽教育訓練中心").FontSize(18)
                                            .FontColor(Colors.Cyan.Darken2);
                                        t.Span("對帳單").FontSize(20)
                                            .Black();
                                    });

                                row.RelativeItem()
                                    .AlignTop()
                                    .AlignRight()
                                    .Text(t =>
                                    {
                                        t.AlignLeft();
                                        t.Line($"製表日： {DateTime.Now:yyyy/MM/dd HH:mm:ss}")
                                            .FontSize(10)
                                            .FontColor(Colors.Purple.Darken2);

                                        t.CurrentPageNumber().Format(i => $"頁　次： {i ?? 0}")
                                            .FontSize(10)
                                            .FontColor(Colors.Purple.Darken2);

                                        t.TotalPages().Format(i => $" / {i ?? 0}")
                                            .FontSize(10)
                                            .FontColor(Colors.Purple.Darken2);
                                    });
                            });
                        });

                    // content
                    page.Content()
                        .PaddingHorizontal(0.5f, Unit.Centimetre)
                        .Column(content =>
                        {
                            content.Spacing(0);

                            content.Item().Text("");

                            // 上段資訊
                            content.Item()
                                .Table(t =>
                                {
                                    t.ColumnsDefinition(cd =>
                                    {
                                        cd.RelativeColumn(2);
                                        cd.RelativeColumn(3);
                                    });

                                    t.Cell().Text($"結帳日期： {data.AccountDate}");
                                    t.Cell().Text($"結帳客戶名稱　： {data.CustomerName}");

                                    t.Cell().Text($"預約單號： {data.RHID}");
                                    t.Cell().Text($"課程／活動名稱： {data.EventName}");

                                    t.Cell().Text($"聯絡人　： {data.ContactName}");
                                    t.Cell().Text("");
                                });

                            content.Item().Text("");

                            // 中段表格
                            content.Item()
                                .Table(t =>
                                {
                                    t.ColumnsDefinition(cd =>
                                    {
                                        cd.RelativeColumn(0.25f);
                                        cd.RelativeColumn(2);
                                        cd.RelativeColumn(2);
                                        cd.RelativeColumn(7);
                                        cd.RelativeColumn(3.5f);
                                        cd.RelativeColumn(0.25f);
                                    });

                                    t.Cell().ColumnSpan(3).AlignCenter().Text("項　　　　目");
                                    t.Cell().Text("");
                                    t.Cell().AlignRight().Text("金　　　　額");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);

                                    foreach (Report17_Output_SubTable_APIItem subTable in data.SubTables)
                                    {
                                        t.Cell().Text("");
                                        t.Cell().Text(subTable.Name).Underline();
                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().Text("");

                                        foreach (Report17_Output_TableRow_APIItem row in subTable.Rows)
                                        {
                                            t.Cell().Text("");
                                            t.Cell().Text("");
                                            t.Cell().Text(row.Date);
                                            t.Cell().Text(row.Description);
                                            t.Cell().AlignRight().Text(row.Amount.ToString("N0"));
                                            t.Cell().Text("");
                                        }

                                        t.Cell();
                                        t.DrawLine(1, 4);
                                        t.Cell();

                                        // 如果這個 subTable 有 quotedPrice，要顯示總定價和總報價
                                        // 否則，只顯示總定價

                                        bool hasQuotedPrice = subTable.QuotedPrice.HasValue;

                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().AlignRight().Text(hasQuotedPrice ? "定價" : "");
                                        t.Cell().AlignRight().Text(subTable.Sum.ToString("N0"));
                                        t.Cell().Text("");

                                        if (!hasQuotedPrice)
                                            continue;

                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().AlignRight().Text("報價");
                                        t.Cell().AlignRight().Text(subTable.QuotedPrice.Value.ToString("N0"));
                                        t.Cell().Text("");
                                    }

                                    t.DrawLine(2, 6);

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().ExtendHorizontal().AlignRight().Text($"合計　　{data.TotalAmount:N0}");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);

                                    t.Cell().Unconstrained().Text($"■費用合計：　{data.TotalAmount:N0}");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text($"■預付金額：　{data.PrepaidAmount:N0}");
                                    t.Cell().ExtendHorizontal().AlignRight().Text($"■餘額：　{data.UnpaidAmount:N0}");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);
                                });

                            content.Item().Text("");

                            // 下段資訊
                            content.Item().Text($"統一編號： {data.Compilation}");
                            content.Item().Text($"發票抬頭： {data.PrintTitle}");
                            content.Item().Text("◎備註");
                            content.Item().Text("　如需更換發票請於5日內通知，謝謝");
                            content.Item().PaddingVertical(0.2f, Unit.Centimetre).Text("　發票號碼：");
                            content.Item().Text("　文件號碼：");
                            content.Item().Text("");
                            content.Item().Text($"　場地租金(含工本費) ${data.SitePayments.Amount:N0} 以{input.PayMethod}支付南山" +
                                                (input.PayDescription.HasContent() ? $"，{input.PayDescription}" : ""));

                            foreach (Report17_Output_Payment_APIItem payment in data.Payments)
                            {
                                content.Item().Text($"　{payment.Type} ${payment.Amount:N0} 支付 {payment.PartnerName}");
                            }

                            // 客戶簽名 footer，因為只在最後一頁秀，不做成 page.footer。
                            content.Item()
                                .ShowEntire()
                                .ExtendVertical()
                                .AlignBottom()
                                .Column(c =>
                                {
                                    c.Spacing(0);
                                    c.Item()
                                        .Text(t =>
                                        {
                                            t.AlignRight();
                                            t.Span("客戶簽名：　＿＿＿＿＿＿＿＿＿＿＿")
                                                .FontSize(26);
                                        });

                                    c.Item().PaddingVertical(4);

                                    c.Item()
                                        .Row(r =>
                                        {
                                            r.RelativeItem(2)
                                                .AlignBottom()
                                                .AlignLeft()
                                                .Text(t =>
                                                {
                                                    t.AlignLeft();
                                                    t.Span("謝謝您對教育訓練中心的關愛與支持，並期待您下次的光臨")
                                                        .FontSize(12);
                                                });

                                            r.RelativeItem()
                                                .AlignBottom()
                                                .AlignRight()
                                                .Text(t =>
                                                {
                                                    t.AlignRight();
                                                    t.Span("南山人壽教育訓練中心")
                                                        .FontSize(16);
                                                });
                                        });
                                });
                        });
                });
            });

            return new FileContentResult(document.GeneratePdf(), "application/pdf");
        }

        #endregion

        #region 端點

        [NonAction]
        public async Task<CommonResponseForPagedList<Report17_Output_APIItem>> GetResultAsync(
            Report17_Input_APIItem input)
        {
            Resver_Head entity = await DC.Resver_Head
                .Include(rh => rh.Resver_Head_Log)
                .Include(rh => rh.Customer)
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.Resver_Throw))))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_Partner))))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.D_FoodCategory))))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device.B_StaticCode)))
                .Include(rh => rh.Resver_Bill)
                .Include(rh => rh.M_Resver_TimeSpan)
                .Include(rh => rh.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Include(rh => rh.Resver_Other)
                .Include(rh => rh.Resver_Other.Select(ro => ro.B_StaticCode))
                .FirstOrDefaultAsync(rh => rh.RHID == input.RHID);

            if (entity is null)
            {
                AddError(NotFound($"預約單號 {input.RHID}", nameof(input.RHID)));
                return null;
            }

            // 取得表格用的 ResverTimeSpan

            var resverTimeSpans = entity.M_Resver_TimeSpan
                .OrderBy(rts => rts.D_TimeSpan.HourS)
                .ThenBy(rts => rts.D_TimeSpan.MinuteS)
                .ToArray();

            // 場地租金
            string resverSiteTableName = DC.GetTableName<Resver_Site>();
            Report17_Output_SubTable_APIItem resverSites = new Report17_Output_SubTable_APIItem
            {
                Name = "場地租金",
                Rows = entity.Resver_Site
                    .OrderBy(rs => rs.TargetDate)
                    .Select(rs => new Report17_Output_TableRow_APIItem
                    {
                        Date = rs.TargetDate.ToFormattedStringDate(),
                        Description = (rs.B_SiteData.Title ?? "")
                                      + " "
                                      + String.Join(" ", resverTimeSpans
                                          .Where(rts => rts.TargetTable == resverSiteTableName)
                                          .Where(rts => rts.TargetID == rs.RSID)
                                          .Select(rts => rts.D_TimeSpan.Title)),
                        Amount = rs.QuotedPrice
                    })
            };
            // 場地這邊以總額來顯示，所以獨立一個參數
            Report17_Output_Payment_APIItem sitePayments = new Report17_Output_Payment_APIItem
            {
                Type = resverSites.Name,
                Amount = resverSites.Sum,
                PartnerName = "南山人壽教育訓練中心"
            };

            // 課程費
            string resverThrowTableName = DC.GetTableName<Resver_Throw>();
            Report17_Output_SubTable_APIItem resverThrows = new Report17_Output_SubTable_APIItem
            {
                Name = "課程費",
                Rows = entity.Resver_Site
                    .SelectMany(rs => rs.Resver_Throw)
                    .Where(rt => !rt.Resver_Throw_Food.Any())
                    .OrderBy(rt => rt.TargetDate)
                    .Select(rt => new Report17_Output_TableRow_APIItem
                    {
                        Date = rt.TargetDate.ToFormattedStringDate(),
                        Description = (rt.Title ?? "")
                                      + " "
                                      + String.Join(" ", resverTimeSpans
                                          .Where(rts => rts.TargetTable == resverThrowTableName)
                                          .Where(rts => rts.TargetID == rt.RTID)
                                          .Select(rts => rts.D_TimeSpan.Title)),
                        Amount = rt.QuotedPrice
                    })
            };

            // 餐飲費
            Report17_Output_SubTable_APIItem resverFoods = new Report17_Output_SubTable_APIItem
            {
                Name = "餐飲費",
                QuotedPrice = entity.Resver_Site
                    .SelectMany(rs => rs.Resver_Throw)
                    .Where(rt => rt.Resver_Throw_Food.Any())
                    .Sum(rt => (int?)rt.QuotedPrice),
                Rows = entity.Resver_Site
                    .SelectMany(rs => rs.Resver_Throw)
                    .OrderBy(rt => rt.TargetDate)
                    .SelectMany(rt => rt.Resver_Throw_Food)
                    .Select(rtf => new Report17_Output_TableRow_APIItem
                    {
                        Date = rtf.Resver_Throw.TargetDate.ToFormattedStringDate(),
                        Description = $"{rtf.D_FoodCategory.Title} {rtf.Ct}份 {rtf.B_Partner.Title}",
                        Amount = rtf.Price,
                        PartnerName = rtf.B_Partner.Title
                    })
            };


            // 設備租金
            string resverDeviceTableName = DC.GetTableName<Resver_Device>();
            Report17_Output_SubTable_APIItem resverDevices = new Report17_Output_SubTable_APIItem
            {
                Name = "設備租金",
                Rows = entity.Resver_Site
                    .Where(rs => rs.Resver_Device.Any())
                    .OrderBy(rs => rs.TargetDate)
                    .SelectMany(rs => rs.Resver_Device
                        .GroupBy(rd => new { rd.TargetDate, rd.B_Device, rd.Ct })
                        .Select(grouping => new Report17_Output_TableRow_APIItem
                        {
                            Date = grouping.Key.TargetDate.ToFormattedStringDate(),
                            Description =
                                $"{rs.B_SiteData.Title} {grouping.Key.B_Device.Title} {grouping.Key.Ct}{grouping.Key.B_Device.B_StaticCode.Title} "
                                + (String.Join(" ", resverTimeSpans
                                    .Where(rts => rts.TargetTable == resverDeviceTableName)
                                    .Where(rts => grouping.Any(rd => rd.RDID == rts.TargetID))
                                    .Select(rts => rts.D_TimeSpan)
                                    .Select(dts => dts.Title))),
                            Amount = grouping.Sum(rd => (int?)rd.QuotedPrice) ?? 0
                        })
                    )
            };

            // 其他項目
            Report17_Output_SubTable_APIItem resverOthers = new Report17_Output_SubTable_APIItem
            {
                Name = "其他項目",
                Rows = entity.Resver_Other
                    .OrderBy(ro => ro.TargetDate)
                    .Select(ro => new Report17_Output_TableRow_APIItem
                    {
                        Date = ro.TargetDate.ToFormattedStringDate(),
                        Description =
                            String.Join(" ", new[] { ro.PrintTitle ?? "其他項目", ro.PrintNote }.Where(s => s != null)) +
                            $" {ro.Ct}{ro.B_StaticCode.Title ?? "個"}",
                        Amount = ro.QuotedPrice
                    })
            };

            return new CommonResponseForPagedList<Report17_Output_APIItem>
            {
                Items = new List<Report17_Output_APIItem>()
                {
                    new Report17_Output_APIItem
                    {
                        RHID = entity.RHID,
                        PayMethod = input.PayMethod ?? "",
                        PayDescription = input.PayDescription ?? "",
                        AccountDate = entity.Resver_Head_Log
                            .Where(rhl => rhl.Type == 4) // 已結帳
                            .OrderByDescending(rhl => rhl.CreDate)
                            .Select(rhl => rhl.CreDate)
                            .FirstOrDefault()
                            .ToFormattedStringDate(),
                        ContactName = entity.Customer?.ContectName ?? "",
                        CustomerName = entity.Customer?.TitleC ?? "",
                        EventName = entity.Title ?? "",
                        SubTables = new[]
                        {
                            resverSites,
                            resverThrows,
                            resverFoods,
                            resverDevices,
                            resverOthers
                        }.Where(st => st.Rows.Any()),
                        PrepaidAmount = entity.Resver_Bill
                            .Where(rb => !rb.DeleteFlag)
                            .Where(rb => rb.PayFlag)
                            .Sum(rb => (int?)rb.Price) ?? 0,
                        Compilation = entity.Customer?.Compilation ?? "",
                        PrintTitle = entity.Customer?.InvoiceTitle ?? "",
                        SitePayments = sitePayments,
                        Payments = new[] { resverThrows, resverFoods, resverDevices, resverOthers }
                            .Where(st => st.Rows.Any())
                            .SelectMany(st =>
                            {
                                // 如果有報價，直接寫一筆報價就好
                                bool hasQuotedPrice = st.QuotedPrice.HasValue;
                                const string defaultPartnerName = "南山人壽教育訓練中心";

                                return hasQuotedPrice
                                    ? new[]
                                    {
                                        new Report17_Output_Payment_APIItem
                                        {
                                            Type = st.Name,
                                            Amount = st.QuotedPrice.Value,
                                            PartnerName = String.Join(" / ", st.Rows
                                                .Where(r => r.PartnerName.HasContent())
                                                .Select(r => r.PartnerName)
                                                .DefaultIfEmpty(defaultPartnerName))
                                        }
                                    }
                                    : st.Rows.Select(r => new Report17_Output_Payment_APIItem
                                    {
                                        Type = st.Name,
                                        Amount = r.Amount,
                                        PartnerName = r.PartnerName ?? defaultPartnerName
                                    });
                            })
                    }
                }
            };
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report17_Input_APIItem input)
        {
            Report17_Output_APIItem result = await GetResult(input);

            return GetResponseJson(result);
        }

        private async Task<Report17_Output_APIItem> GetResult(Report17_Input_APIItem input)
        {
            CommonResponseForPagedList<Report17_Output_APIItem> results = await GetResultAsync(input);
            Report17_Output_APIItem result = results?.Items?.FirstOrDefault(i => i.RHID == input.RHID);
            return result;
        }

        #endregion
    }
}