using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report5;
using NS_Education.Models.Utilities.PrintReport;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NS_Education.Tools.Extensions
{
    public static class CommonResponseForPagedListExtension
    {
        public static byte[] MakePdf<TOutput, TInput>(this CommonResponseForList<TOutput> data
            , TInput input
            , int uid
            , string userName
            , string reportTitle
            , PdfColumn<TOutput>[] columnDefinition)
        {
            Document document = Document.Create(container =>
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
                                        t.Line($"製表者 ID: {uid}").FontSize(16).Bold()
                                            .FontColor(Colors.Purple.Darken2);
                                        t.Line($"製表者: {userName}")
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
                                        t.Line(reportTitle).FontSize(26).Bold().Black();
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
                                foreach (PdfColumn<TOutput> column in columnDefinition)
                                {
                                    cd.RelativeColumn(column.LengthWeight);
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (PdfColumn<TOutput> column in columnDefinition)
                                {
                                    header.Cell()
                                        .Column(c =>
                                        {
                                            c.Item().Text(column.Name);
                                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                                        });
                                }
                            });

                            foreach (TOutput row in data.Items)
                            {
                                foreach (PdfColumn<TOutput> column in columnDefinition)
                                {
                                    table.Cell()
                                        .Column(c =>
                                        {
                                            c.Item().Text(column.Formatter.Invoke(column.Selector.Invoke(row)));
                                        });
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

                                for (int j = 1; j < columnDefinition.Length; j++)
                                {
                                    PdfColumn<TOutput> column = columnDefinition[j];

                                    if (!column.OutputTotal)
                                    {
                                        f.Cell().Column(c => { c.Item().LineHorizontal(1).LineColor(Colors.Black); });
                                        continue;
                                    }

                                    f.Cell().Column(c =>
                                    {
                                        c.Item().LineHorizontal(1).LineColor(Colors.Black);
                                        c.Item().Text(data.Items.Sum(item => Convert.ToDecimal(column.Selector.Invoke(item))).ToString("N0"));
                                    });
                                }
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}