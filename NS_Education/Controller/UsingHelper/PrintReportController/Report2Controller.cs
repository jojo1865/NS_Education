using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report2;
using NS_Education.Models.Entities;
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
    /// Function Order 的處理。
    /// </summary>
    public class Report2Controller : PublicClass, IPrintReport<Report2_Input_APIItem, Report2_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report2_Output_Row_APIItem>> GetResultAsync(
            Report2_Input_APIItem input)
        {
            input.RHID = input.RHID ?? Array.Empty<int>();

            using (NsDbContext dbContext = new NsDbContext())
            {
                var query = dbContext.Resver_Head
                    .Include(rh => rh.Customer)
                    .Include(rh => rh.Resver_Site)
                    .Include(rh => rh.Resver_Bill)
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_StaticCode))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData).Select(rs => rs.B_StaticCode1))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                    .Include(rh => rh.Resver_Site.Select(rs =>
                        rs.Resver_Device.Select(rd => rd.B_Device).Select(bd => bd.B_StaticCode)))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                    .Include(rh => rh.Resver_Site.Select(rs =>
                        rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.Resver_Throw))))
                    .Include(rh => rh.Resver_Site.Select(rs =>
                        rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_StaticCode))))
                    .Include(rh => rh.Resver_Site.Select(rs =>
                        rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.D_FoodCategory))))
                    .Include(rh => rh.Resver_Site.Select(rs =>
                        rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_Partner))))
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => input.RHID.Contains(rh.RHID))
                    .AsQueryable();

                var results = await query
                    .OrderBy(rh => rh.RHID)
                    .ToArrayAsync();

                Report2_Output_APIItem response = new Report2_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(rh => new Report2_Output_Row_APIItem
                {
                    HostName = rh.Customer.TitleC ?? rh.Customer.TitleE ?? "",
                    EventTitle = rh.Title,
                    RHID = rh.RHID,
                    StartDate = rh.SDate.FormatAsRocYyyMmDdWeekDay(),
                    EndDate = rh.EDate.FormatAsRocYyyMmDdWeekDay(),
                    SiteNames = rh.Resver_Site.Select(rs => rs.B_SiteData.Title).Distinct(),
                    MKT = rh.MKT,
                    Owner = rh.Owner,
                    ParkingNote = rh.ParkingNote,
                    Contact = rh.ContactName,
                    PayStatus = GetBills(rh),
                    Sites = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Select((rs, index) => new Report2_Output_Row_Site_APIItem
                        {
                            Title = $"場地 {index + 1}：{rs.B_SiteData?.Title ?? ""}",
                            Date = rs.TargetDate.FormatAsRocYyyMmDd(),
                            Lines = GetLines(rs),
                            SeatImage = rs.SeatImage != null ? Convert.ToBase64String(rs.SeatImage) : null,
                            Note = rs.Note
                        }),
                    Foods = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .SelectMany(rs => rs.Resver_Throw)
                        .Where(rt => !rt.DeleteFlag)
                        .SelectMany(rt => rt.Resver_Throw_Food)
                        .Select(rtf => new Report2_Output_Row_Food_APIItem
                        {
                            Date = rtf.Resver_Throw?.TargetDate.ToString("M/dd"),
                            FoodType = rtf.B_StaticCode?.Title ?? "",
                            ArriveTime = rtf.ArriveTime.ToFormattedStringTime(),
                            Form = rtf.Resver_Throw?.PrintNote ?? "",
                            Ct = rtf.Ct,
                            QuotedPrice = rtf.Price,
                            Partner = rtf.B_Partner?.Title ?? "",
                            Note = rtf.Resver_Throw?.Note ?? ""
                        })
                }).ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        private IEnumerable<string> GetLines(Resver_Site rs)
        {
            string siteTableName = DC.GetTableName<Resver_Site>();

            D_TimeSpan[] timeSpans = DC.M_Resver_TimeSpan
                .Include(rts => rts.D_TimeSpan)
                .Where(rts => rts.TargetTable == siteTableName)
                .Where(rts => rts.TargetID == rs.RSID)
                .Select(rts => rts.D_TimeSpan)
                .OrderBy(dts => dts.HourS)
                .ThenBy(dts => dts.MinuteS)
                .ThenBy(dts => dts.HourE)
                .ThenBy(dts => dts.MinuteE)
                .ToArray();

            D_TimeSpan earliest = timeSpans.FirstOrDefault();
            D_TimeSpan latest = timeSpans.LastOrDefault();

            TimeSpan? earliestTimeSpan = earliest != null
                ? new TimeSpan(earliest.HourS, earliest.MinuteS, 0)
                : (TimeSpan?)null;
            TimeSpan? latestTimeSpan = latest != null
                ? new TimeSpan(latest.HourE, latest.MinuteE, 0)
                : (TimeSpan?)null;

            IEnumerable<string> result = Array.Empty<string>();

            string arriveTime = GetArriveTime(rs);
            if (arriveTime != null)
                result = result.Append(arriveTime);

            result = result.Append($"活動時間：{FormatTwoTimes(earliestTimeSpan, latestTimeSpan)}。");

            result = result.Append($"{rs.B_StaticCode?.Title ?? "無資料"}：{rs.B_SiteData?.MaxSize ?? 0} 人" +
                                   (rs.TableDescription != null ? $"（{rs.TableDescription}）" : ""));

            var devices = rs.Resver_Device
                .Where(rd => !rd.DeleteFlag)
                .Select(rd =>
                    $"{rd.B_Device?.Title} * {rd.Ct} {rd.B_Device?.B_StaticCode?.Title ?? ""}（{rd.Note}）");

            result = result.Concat(devices);

            return result
                .Select((line, index) => $"{index + 1}、{line}")
                .ToArray();
        }

        private static string[] GetBills(Resver_Head rh)
        {
            ICollection<string> results = new List<string>();

            Resver_Bill[] paidBills = rh.Resver_Bill
                .Where(rb => !rb.DeleteFlag)
                .Where(rb => rb.PayFlag)
                .ToArray();

            foreach (Resver_Bill resverBill in paidBills)
            {
                results.Add($"已支付 {resverBill.Note}：{resverBill.Price:C0} 元");
            }

            int sum = paidBills.Sum(pb => (int?)pb.Price) ?? 0;

            if (sum < rh.QuotedPrice)
                results.Add($"事後匯款支付：{rh.QuotedPrice - sum:C0} 元");

            return results.ToArray();
        }

        private string GetArriveTime(Resver_Site rs)
        {
            if (rs.ArriveTimeStart == null && rs.ArriveTimeEnd == null)
                return null;

            StringBuilder result = new StringBuilder("活動抵達時間：");
            result.Append(FormatTwoTimes(rs.ArriveTimeStart, rs.ArriveTimeEnd));

            if (rs.ArriveDescription != null)
                result.Append($"（{rs.ArriveDescription}）");

            return result.ToString();
        }

        private string FormatTwoTimes(TimeSpan? a, TimeSpan? b)
        {
            return a != null && b != null && a != b
                ? $"{a.ToFormattedStringTime()} ~ {b.ToFormattedStringTime()}"
                : (a ?? b).ToFormattedStringTime();
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report2_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetPdf(Report2_Input_APIItem input)
        {
            CommonResponseForPagedList<Report2_Output_Row_APIItem> data = await GetResultAsync(input);

            if (!data.Items.Any())
                throw new Exception("查無可匯出資料！");

            PageSize pageSize = PageSizes.A4.Portrait();
            string userName = await GetUserNameByID(GetUid());
            Document document = Document.Create(d =>
            {
                foreach (Report2_Output_Row_APIItem item in data.Items)
                {
                    d.Page(p =>
                    {
                        p.BasicSetting(pageSize);
                        p.BasicHeader(GetUid(),
                            userName,
                            "Function Order",
                            titleSize: 18);

                        string startDateFormat = item.StartDate;
                        string endDateFormat = item.EndDate;
                        string eventDateString = startDateFormat == endDateFormat
                            ? startDateFormat
                            : $"{startDateFormat} ~ {endDateFormat}";
                        p.Content()
                            .Column(c =>
                            {
                                // 上部基本資訊
                                c.Item().Text($"主辦單位：{item.HostName}");
                                c.Item().Text($"活動名稱：{item.EventTitle}");
                                c.Item().Text($"預約單號：{item.RHID}");
                                c.Item().Text($"活動日期：{eventDateString}");
                                c.Item().Text($"使用場地：{String.Join("、", item.SiteNames)}");
                                c.Item().Text($"MKT：{item.MKT ?? "無"}／Owner：{item.Owner ?? "無"}");

                                c.Item().PaddingVertical(0.2f, Unit.Centimetre);

                                // 粗線細線
                                c.Item().LineHorizontal(5);
                                c.Item().LineHorizontal(2);

                                // 每個場地一個表格
                                foreach (Report2_Output_Row_Site_APIItem site in item.Sites)
                                {
                                    // 場地 Title 以：分隔，前段為場地編號，後段為場地名稱（紅字）
                                    string[] siteSplit = site.Title.Split('：');

                                    c.Item().Text(t =>
                                    {
                                        t.Span($"【{siteSplit[0]}】");
                                        t.Span($"{siteSplit[1]}").FontColor(Colors.Red.Accent1);
                                        t.Span($"（{site.Date}）");
                                    });

                                    // 場地內容表格
                                    c.Item()
                                        .Border(1)
                                        .Table(t =>
                                        {
                                            t.ColumnsDefinition(cd =>
                                            {
                                                cd.RelativeColumn(2);
                                                cd.RelativeColumn();
                                            });
                                            t.Header(h =>
                                            {
                                                h.Cell()
                                                    .Border(1)
                                                    .AlignCenter()
                                                    .Text("項目");

                                                h.Cell()
                                                    .Border(1)
                                                    .AlignCenter()
                                                    .Text("備註");
                                            });

                                            bool isFirst = true;
                                            foreach (string line in site.Lines)
                                            {
                                                t.Cell()
                                                    .Border(1)
                                                    .AlignLeft()
                                                    .PaddingLeft(0.1f, Unit.Centimetre)
                                                    .Text(line);

                                                if (isFirst)
                                                {
                                                    t.Cell()
                                                        .BorderTop(1)
                                                        .BorderLeft(1)
                                                        .BorderRight(1)
                                                        .AlignCenter()
                                                        .Text(site.Note);

                                                    isFirst = false;
                                                }
                                                else
                                                {
                                                    t.Cell();
                                                }
                                            }
                                        });
                                    c.Item().PaddingVertical(0.2f, Unit.Centimetre);
                                }

                                // 餐飲表格
                                ISet<string> allUniqueDescriptions = item.Foods
                                    .Select(f => f.Note.EndsWith("。") ? f.Note.Substring(0, f.Note.Length - 1) : f.Note)
                                    .Where(f => f.HasContent())
                                    .Distinct()
                                    .ToHashSet();

                                string joinedDescriptions = String.Join("。", allUniqueDescriptions);

                                c.Item().Text($"【餐飲】{joinedDescriptions}")
                                    .Bold();

                                c.Item()
                                    .Border(1)
                                    .Table(t =>
                                    {
                                        t.ColumnsDefinition(cd =>
                                        {
                                            cd.RelativeColumn();
                                            cd.RelativeColumn(3);
                                            cd.RelativeColumn(2);
                                            cd.RelativeColumn(6);
                                            cd.RelativeColumn(4);
                                            cd.RelativeColumn(3);
                                            cd.RelativeColumn(2);
                                        });

                                        // 表格 header
                                        t.Cell().Border(1).AlignCenter().Text("日期");
                                        t.Cell().Border(1).AlignCenter().Text("餐別");
                                        t.Cell().Border(1).AlignCenter().Text("送達時間");
                                        t.Cell().Border(1).AlignCenter().Text("形式");
                                        t.Cell().Border(1).AlignCenter().Text("數量/金額");
                                        t.Cell().Border(1).AlignCenter().Text("廠商");
                                        t.Cell().Border(1).AlignLeft().PaddingLeft(0.1f, Unit.Centimetre).Text("備註");

                                        foreach (Report2_Output_Row_Food_APIItem food in item.Foods)
                                        {
                                            t.Cell().Border(1).AlignCenter().Text(food.Date);
                                            t.Cell().Border(1).AlignCenter().Text(food.FoodType);
                                            t.Cell().Border(1).AlignCenter().Text(food.ArriveTime);
                                            t.Cell().Border(1).AlignCenter().Text(food.Form);

                                            // 只有 1 份就顯示總價，否則顯示每人價
                                            string singlePrice = "$" + food.QuotedPrice.ToString("N0");
                                            string personPrice =
                                                "$" + (food.QuotedPrice / Math.Max(food.Ct, 1)).ToString("N0") +
                                                "*" + food.Ct + "人份";

                                            t.Cell().Border(1).AlignCenter()
                                                .Text(food.Ct <= 1 ? singlePrice : personPrice);
                                            t.Cell().Border(1).AlignCenter().Text(food.Partner);
                                            t.Cell().Border(1).AlignLeft().PaddingLeft(0.1f, Unit.Centimetre).Text(food.Note);
                                        }
                                    });

                                // 設備
                                c.Item().PaddingVertical(0.2f, Unit.Centimetre);
                                c.Item().Text("【設備】")
                                    .Bold();

                                c.Item().Text("1、各教室標準設備");

                                // 交通
                                if (item.ParkingNote.HasContent())
                                {
                                    c.Item().PaddingVertical(0.2f, Unit.Centimetre);
                                    c.Item().Text("【交通】")
                                        .Bold();
                                    c.Item().Text(item.ParkingNote);
                                }

                                // 結帳
                                c.Item().PaddingVertical(0.2f, Unit.Centimetre);
                                c.Item().Text("【結帳】")
                                    .Bold();
                                c.Item().Text($"1、聯絡人：{item.Contact ?? "無資料"}");
                                c.Item().Text(t =>
                                {
                                    t.Span("2、付款方式：");
                                    foreach (string pay in item.PayStatus)
                                    {
                                        t.Span($"\n 　{pay}");
                                    }
                                });

                                c.Item().PaddingVertical(0.2f, Unit.Centimetre).Text("　　　　　　　□ 抬頭：");
                                c.Item().Text("　　　　　　　□ 統編：");
                            });
                    });
                }
            });

            byte[] pdf = document.GeneratePdf();

            return new FileContentResult(pdf, "application/pdf");
        }
    }
}