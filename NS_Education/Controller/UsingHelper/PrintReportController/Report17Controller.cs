using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.PrintReport.Report1;
using NS_Education.Models.Utilities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 對帳單的處理。
    /// </summary>
    public class Report17Controller : PublicClass
    {
        public async Task<FileContentResult> GetPdf(Report17_Input_APIItem input)
        {
            string userName = "dummy";
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

                                    t.Cell().Text("結帳日期： 2023/10/03");
                                    t.Cell().Text("結帳客戶名稱　： 星穹鐵道列車組");

                                    t.Cell().Text("預約單號： 1");
                                    t.Cell().Text("課程／活動名稱： 員工訓練：認識星核的危險性與如何預防");

                                    t.Cell().Text("聯絡人　： 三月七");
                                    t.Cell().Text("主辦單位　　　： 貝洛柏格總督府");
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
                                        cd.RelativeColumn(6);
                                        cd.RelativeColumn(4);
                                        cd.RelativeColumn(0.25f);
                                    });

                                    t.Cell().ColumnSpan(3).AlignCenter().Text("項　　　　目");
                                    t.Cell().Text("");
                                    t.Cell().AlignRight().Text("金　　　　額");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);

                                    t.Cell().Text("");
                                    t.Cell().Text("場地租金").Underline();
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("2023/10/03");
                                    t.Cell().Text("208訓練教室 上午 下午");
                                    t.Cell().AlignRight().Text("18,050");
                                    t.Cell().Text("");

                                    t.Cell();
                                    t.DrawLine(1, 4);
                                    t.Cell();

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().AlignRight().Text("18,050");
                                    t.Cell().Text("");

                                    t.Cell().Text("");
                                    t.Cell().Text("餐飲費").Underline();
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("2023/10/03");
                                    t.Cell().Text("茶點200 500份 禧樂盛宴有限公司");
                                    t.Cell().AlignRight().Text("10,000");
                                    t.Cell().Text("");

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("2023/10/03");
                                    t.Cell().Text("餐盒100 68份 夏爾國際餐飲有限公司");
                                    t.Cell().AlignRight().Text("10,000");
                                    t.Cell().Text("");

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("2023/10/03");
                                    t.Cell().Text("點心盒100 70份 葡萄樹食品股份有限公司");
                                    t.Cell().AlignRight().Text("10,000");
                                    t.Cell().Text("");

                                    t.Cell();
                                    t.DrawLine(1, 4);
                                    t.Cell();

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().AlignRight().Text("23,800");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);

                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().ExtendHorizontal().AlignRight().Text("合計　　41,850");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);

                                    t.Cell().Unconstrained().Text("■費用合計：　41,850");
                                    t.Cell().Text("");
                                    t.Cell().Text("");
                                    t.Cell().Text("■預付金額：　0");
                                    t.Cell().ExtendHorizontal().AlignRight().Text("■餘額：　41,850");
                                    t.Cell().Text("");

                                    t.DrawLine(2, 6);
                                });

                            content.Item().Text("");

                            // 下段資訊
                            content.Item().Text("統一編號： 88498124");
                            content.Item().Text("發票抬頭： 財團法人農田水利人力發展中心");
                            content.Item().Text("◎備註");
                            content.Item().Text("　如需更換發票請於5日內通知，謝謝");
                            content.Item().PaddingVertical(0.4f, Unit.Centimetre).Text("　發票號碼：");
                            content.Item().Text("　文件號碼：");
                            content.Item().Text("");
                            content.Item().Text($"　場地租金(含工本費) $18,050 以{input.PayMethod}支付南山" +
                                                (input.PayDescription.HasContent() ? $"，{input.PayDescription}" : ""));
                            content.Item().Text($"　餐飲費 $10,000 支付 禧樂盛宴有限公司");
                            content.Item().Text($"　餐飲費 $6,800 支付 夏爾國際餐飲有限公司");
                            content.Item().Text($"　餐飲費 $7,000 支付 葡萄樹食品股份有限公司");

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
    }
}