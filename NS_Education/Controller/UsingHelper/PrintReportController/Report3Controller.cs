using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report3;
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
    /// 客戶授權簽核表的處理。
    /// </summary>
    public class Report3Controller : PublicClass, IPrintReport<Report3_Input_APIItem, Report3_Output_Row_APIItem>
    {
        #region 報表分析 Get

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report3_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }

        #endregion

        #region 報表PDF GetPdf

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetPdf(Report3_Input_APIItem input)
        {
            CommonResponseForPagedList<Report3_Output_Row_APIItem> data = await GetResultAsync(input);

            if (!data.Items.Any())
                throw new Exception("查無可匯出資料！");

            string queryConditionString = String.Join(", ", input.RHID ?? Array.Empty<int>());

            PageSize pageSize = PageSizes.A4.Portrait();
            string userName = await GetUserNameByID(GetUid());
            Document document = Document.Create(d =>
            {
                // 一個預約單號一份主表
                foreach (Report3_Output_Row_APIItem item in data.Items)
                {
                    d.Page(p =>
                    {
                        p.BasicSetting(pageSize);
                        p.BasicHeader(GetUid(),
                            userName,
                            "授權簽核表",
                            queryConditionString.HasContent() ? $"預約單號 {queryConditionString}" : null,
                            18);

                        p.Content()
                            .PaddingLeft(0.5f, Unit.Centimetre)
                            .PaddingRight(0.5f, Unit.Centimetre)
                            .Column(c =>
                            {
                                c.Spacing(0f);

                                // 上部 - 基本資訊
                                c.Item()
                                    .Table(t =>
                                    {
                                        t.ColumnsDefinition(cd =>
                                        {
                                            cd.RelativeColumn();
                                            cd.RelativeColumn(9);
                                        });

                                        t.Cell().Text("預約單號");
                                        t.Cell().Text(item.RHID.ToString());

                                        t.Cell().Text("主辦單位");
                                        t.Cell().Text(item.HostName);

                                        t.Cell().Text("活動名稱");
                                        t.Cell().Text(item.EventName);

                                        t.Cell().Text("活動日期");
                                        t.Cell().Text(item.StartDate != item.EndDate
                                            ? $"{item.StartDate} ~ {item.EndDate}"
                                            : $"{item.StartDate}");

                                        t.Cell().Text("人　　數");
                                        t.Cell().Text($"{item.PeopleCt} 人");
                                    });

                                // 中部 - 收入分析

                                c.Item().Text("");

                                c.Item()
                                    .Text("一、收入分析");

                                c.Item()
                                    .Table(t =>
                                    {
                                        t.ColumnsDefinition(cd =>
                                        {
                                            cd.RelativeColumn(4);
                                            cd.RelativeColumn(2);
                                            cd.RelativeColumn(2);
                                            cd.RelativeColumn(2);
                                            cd.RelativeColumn(2);
                                            cd.RelativeColumn(4);
                                        });

                                        t.Cell();
                                        t.Cell().AlignRight().Text("定價");
                                        t.Cell().AlignRight().Text("報價");
                                        t.Cell().AlignRight().Text("成本");
                                        t.Cell().AlignRight().Text("價差");
                                        t.Cell().AlignRight().Text("備　　　　註");

                                        t.DrawLine(1, 6);

                                        foreach (Report3_Output_Row_Income_APIItem income in item.Incomes)
                                        {
                                            // 每個 title 中間加入空白
                                            t.Cell().AlignCenter()
                                                .Text(String.Join("　　　　", income.Title.ToCharArray()));
                                            t.Cell().AlignRight().Text($"{income.FixedPrice:N0}");
                                            t.Cell().AlignRight().Text($"{income.QuotedPrice:N0}");
                                            t.Cell().AlignRight().Text($"{income.UnitPrice:N0}");
                                            t.Cell().AlignRight().Text($"{income.Difference:N0}");
                                            t.Cell().AlignRight().Text(income.Note.Trim());
                                        }

                                        t.DrawLine(1, 6);

                                        t.Cell().AlignCenter().Text("合　　　　計");
                                        t.Cell().AlignRight()
                                            .Text($"{item.Incomes.Sum(i => (int?)i.FixedPrice) ?? 0:N0}");
                                        t.Cell().AlignRight()
                                            .Text($"{item.Incomes.Sum(i => (int?)i.QuotedPrice) ?? 0:N0}");
                                        t.Cell().AlignRight()
                                            .Text($"{item.Incomes.Sum(i => (int?)i.UnitPrice) ?? 0:N0}");
                                        t.Cell().AlignRight()
                                            .Text($"{item.Incomes.Sum(i => (int?)i.Difference) ?? 0:N0}");
                                        t.Cell();
                                    });

                                // 中部 - 細項說明

                                c.Item().Text("");

                                c.Item()
                                    .Text("二、細項說明");

                                foreach (IGrouping<string, Report3_Output_Row_Detail_APIItem> detailGroup in item
                                             .Details.GroupBy(detail => detail.TypeName))
                                {
                                    c.Item().Text("");

                                    c.Item()
                                        .Text($"§ {detailGroup.Key}費用");

                                    c.Item()
                                        .Table(t =>
                                        {
                                            t.ColumnsDefinition(cd =>
                                            {
                                                cd.RelativeColumn(2);
                                                cd.RelativeColumn(3);
                                                cd.RelativeColumn(5);
                                                cd.RelativeColumn(3);
                                                cd.RelativeColumn(3);
                                                cd.RelativeColumn(3);
                                            });

                                            t.Cell().AlignLeft().Text("日期");

                                            // 有些資料如「其他收費項目」沒有時段，這邊判定沒有任何時段資料，就直接不顯示欄位名稱
                                            t.Cell().AlignLeft()
                                                .Text(detailGroup.Any(i => i.TimeSpans.Any()) ? "時段" : "");
                                            t.Cell().AlignLeft()
                                                .Text(detailGroup.FirstOrDefault()?.OverrideColumnTypeName);
                                            t.Cell().AlignLeft().Text(detailGroup.FirstOrDefault()?.SubTypeName);
                                            t.Cell().AlignRight().Text("定價");
                                            t.Cell().AlignRight().Text("報價");

                                            t.DrawLine(1, 6);

                                            foreach (Report3_Output_Row_Detail_APIItem detailItem in detailGroup)
                                            {
                                                t.Cell().AlignLeft().Text(detailItem.Date);
                                                t.Cell().AlignLeft().Text(String.Join("、", detailItem.TimeSpans));
                                                t.Cell().AlignLeft().Text(detailItem.Title);
                                                t.Cell().AlignLeft().Text(detailItem.SubType);
                                                t.Cell().AlignRight().Text($"{detailItem.FixedPrice:N0}");
                                                t.Cell().AlignRight().Text($"{detailItem.QuotedPrice:N0}");
                                            }

                                            t.DrawLine(1, 6);

                                            t.Cell().AlignCenter().Text("合計：");
                                            t.Cell();
                                            t.Cell();
                                            t.Cell();
                                            t.Cell().AlignRight()
                                                .Text($"{detailGroup.Sum(dg => (int?)dg.FixedPrice) ?? 0:N0}");
                                            t.Cell().AlignRight()
                                                .Text($"{detailGroup.Sum(dg => (int?)dg.QuotedPrice) ?? 0:N0}");
                                        });
                                }

                                // 下部 - 備註

                                c.Item().Text("");

                                c.Item().Text("◎");

                                c.Item().PaddingLeft(1.5f, Unit.Centimetre).Text(input.Description);

                                c.Item().Text("◎");

                                c.Item().Text("");

                                c.Item()
                                    .EnsureSpace()
                                    .Table(t =>
                                    {
                                        t.ColumnsDefinition(cd =>
                                        {
                                            cd.RelativeColumn(3);
                                            cd.RelativeColumn(6);
                                            cd.RelativeColumn(9);
                                        });

                                        t.Cell().AlignCenter().Text("製表者");
                                        t.Cell().AlignCenter().Text("業務服務組長");
                                        t.Cell().AlignCenter().Text("教育訓練中心業務主管");

                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().Text("");

                                        t.Cell().Text("");
                                        t.Cell().Text("");
                                        t.Cell().Text("");

                                        t.DrawLine(1, 3);
                                    });
                            });
                    });
                }
            });

            byte[] pdf = document.GeneratePdf();

            return new FileContentResult(pdf, "application/pdf");
        }

        #endregion

        #region query

        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report3_Output_Row_APIItem>> GetResultAsync(
            Report3_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                Report3_Output_APIItem response = new Report3_Output_APIItem();
                response.SetByInput(input);
                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);

                IEnumerable<Resver_Head> heads = await GetHead(input, dbContext);
                heads = heads.ToArray();

                if (!heads.Any())
                {
                    AddError(NotFound("預約單", nameof(input.RHID)));
                    return null;
                }

                foreach (Resver_Head head in heads)
                {
                    Report3_Output_Row_APIItem row = new Report3_Output_Row_APIItem();
                    response.Items.Add(row);

                    AssignBasicFields(row, head);
                    AssignIncomes(head, row);
                    AssignDetails(head, row);
                }

                return response;
            }
        }

        private static async Task<IEnumerable<Resver_Head>> GetHead(Report3_Input_APIItem input, NsDbContext dbContext)
        {
            var query = dbContext.Resver_Head.AsQueryable()
                .Include(rh => rh.Customer)
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData).Select(bs => bs.B_StaticCode1))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh =>
                    rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device).Select(bd => bd.B_Category)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.B_StaticCode)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                .Include(rh => rh.Resver_Other)
                .Include(rh => rh.Resver_Other.Select(ro => ro.D_OtherPayItem))
                .Include(rh => rh.M_Resver_TimeSpan)
                .Include(rh => rh.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Where(rh => !rh.DeleteFlag);

            if (input.RHID != null)
                query = query.Where(rh => input.RHID.Contains(rh.RHID));

            return await query.ToArrayAsync();
        }

        private void AssignBasicFields(Report3_Output_Row_APIItem response, Resver_Head head)
        {
            response.RHID = head.RHID;
            response.StartDate = head.SDate.ToFormattedStringDate();
            response.EndDate = head.EDate.ToFormattedStringDate();
            response.PeopleCt = head.PeopleCt;
            response.HostName = head.Customer?.TitleC ?? "";
            response.EventName = head.Title;
        }

        private static void AssignDetails(Resver_Head head, Report3_Output_Row_APIItem response)
        {
            var siteDetails = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .GroupBy(rs => new { rs.TargetDate, rs.BSID, rs.QuotedPrice })
                .Select(grouping => new Report3_Output_Row_Detail_APIItem
                {
                    TypeName = "場地",
                    Date = grouping.Max(rs => rs.TargetDate).ToFormattedStringDate(),
                    TimeSpans = GetResverTimeSpans(head, typeof(Resver_Site), grouping.Select(rs => rs.RSID)),
                    Title = grouping.Max(rs => rs.B_SiteData.Title),
                    SubTypeName = "桌型",
                    SubType = grouping.Max(rs => rs.B_SiteData.B_StaticCode1.Title),
                    FixedPrice = grouping.Max(rs => rs.FixedPrice),
                    QuotedPrice = grouping.Max(rs => rs.QuotedPrice)
                });

            var deviceDetails = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .SelectMany(rs => rs.Resver_Device)
                .Where(rd => !rd.DeleteFlag)
                .GroupBy(rd => new { rd.TargetDate, rd.BDID, rd.QuotedPrice })
                .Select(grouping => new Report3_Output_Row_Detail_APIItem
                {
                    TypeName = "設備",
                    Date = grouping.Max(rd => rd.TargetDate).ToFormattedStringDate(),
                    TimeSpans = GetResverTimeSpans(head, typeof(Resver_Device), grouping.Select(rd => rd.RDID)),
                    Title = grouping.Max(rd => rd.B_Device.Title),
                    SubTypeName = "種類",
                    SubType = grouping.Max(rd => rd.B_Device.B_Category.TitleC),
                    FixedPrice = grouping.Max(rd => rd.FixedPrice),
                    QuotedPrice = grouping.Max(rd => rd.QuotedPrice)
                });

            var throwDetails = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .SelectMany(rs => rs.Resver_Throw)
                .Where(rt => !rt.DeleteFlag)
                .GroupBy(rt => new { rt.TargetDate, rt.BSCID, rt.QuotedPrice, IsFood = rt.Resver_Throw_Food.Any() })
                .Select(grouping => new Report3_Output_Row_Detail_APIItem
                {
                    TypeName = "行程",
                    Date = grouping.Max(rt => rt.TargetDate).ToFormattedStringDate(),
                    TimeSpans = GetResverTimeSpans(head, typeof(Resver_Throw), grouping.Select(rt => rt.RTID)),
                    Title = grouping.Max(rt => rt.Title),
                    SubTypeName = "類型",
                    SubType = grouping.Key.IsFood ? "餐飲" : "訓練",
                    FixedPrice = grouping.Max(rt => rt.FixedPrice),
                    QuotedPrice = grouping.Max(rt => rt.QuotedPrice)
                });

            var otherDetails = head.Resver_Other
                .Where(ro => !ro.DeleteFlag)
                .GroupBy(ro => new { ro.TargetDate, ro.PrintTitle, ro.QuotedPrice })
                .Select(grouping => new Report3_Output_Row_Detail_APIItem
                {
                    TypeName = "其他收費項目",
                    Date = grouping.Max(ro => ro.TargetDate).ToFormattedStringDate(),
                    TimeSpans = GetResverTimeSpans(head, typeof(Resver_Other), grouping.Select(ro => ro.ROID)),
                    Title = grouping.Max(ro => ro.D_OtherPayItem.Title),
                    SubTypeName = "帳單列印說明",
                    SubType = grouping.Max(ro => ro.PrintNote),
                    FixedPrice = grouping.Max(ro => ro.FixedPrice),
                    QuotedPrice = grouping.Max(ro => ro.QuotedPrice),
                    OverrideColumnTypeName = "收費項目"
                });

            response.Details = siteDetails
                .Concat(deviceDetails)
                .Concat(throwDetails)
                .Concat(otherDetails);
        }

        private static string[] GetResverTimeSpans(Resver_Head head, Type type, IEnumerable<int> ids)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                string tableName = dbContext.GetTableName(type);
                return head.M_Resver_TimeSpan
                    .Where(rts => rts.TargetTable == tableName)
                    .Where(rts => ids.Contains(rts.TargetID))
                    .Select(rts => rts.D_TimeSpan.Title)
                    .Distinct()
                    .ToArray();
            }
        }

        private static void AssignIncomes(Resver_Head head, Report3_Output_Row_APIItem response)
        {
            var siteIncomes = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .GroupBy(rs => rs.RHID)
                .Select(grouping => new Report3_Output_Row_Income_APIItem
                {
                    Title = "場地",
                    FixedPrice = grouping.Sum(rs => (int?)rs.FixedPrice) ?? 0,
                    QuotedPrice = grouping.Sum(rs => (int?)rs.QuotedPrice) ?? 0,
                    UnitPrice = grouping.Sum(rs => (int?)rs.UnitPrice) ?? 0,
                    Note = String.Join("\n", grouping.Select(rs => rs.Note))
                });

            var deviceIncomes = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .SelectMany(rs => rs.Resver_Device)
                .Where(rd => !rd.DeleteFlag)
                .GroupBy(rd => rd.RSID)
                .Select(grouping =>
                    new Report3_Output_Row_Income_APIItem
                    {
                        Title = "設備",
                        FixedPrice = grouping.Sum(rd => (int?)rd.FixedPrice) ?? 0,
                        QuotedPrice = grouping.Sum(rd => (int?)rd.QuotedPrice) ?? 0,
                        UnitPrice = grouping.Sum(rd => (int?)rd.UnitPrice) ?? 0,
                        Note = String.Join("\n", grouping.Select(rd => rd.Note))
                    });

            var throwIncomes = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .SelectMany(rs => rs.Resver_Throw)
                .Where(rt => !rt.DeleteFlag)
                .GroupBy(rt => rt.RSID)
                .Select(grouping =>
                    new Report3_Output_Row_Income_APIItem
                    {
                        Title = "行程",
                        FixedPrice = grouping.Sum(rt => (int?)rt.FixedPrice) ?? 0,
                        QuotedPrice = grouping.Sum(rt => (int?)rt.QuotedPrice) ?? 0,
                        UnitPrice = grouping.Sum(rt => (int?)rt.UnitPrice) ?? 0,
                        Note = String.Join("\n", grouping.Select(rt => rt.Note))
                    });

            var otherIncomes = head.Resver_Other
                .Where(ro => !ro.DeleteFlag)
                .GroupBy(ro => ro.RHID)
                .Select(grouping => new Report3_Output_Row_Income_APIItem
                {
                    Title = "其他",
                    FixedPrice = grouping.Sum(ro => (int?)ro.FixedPrice) ?? 0,
                    QuotedPrice = grouping.Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    UnitPrice = grouping.Sum(ro => (int?)ro.UnitPrice) ?? 0,
                    Note = String.Join("\n", grouping.Select(ro => ro.Note))
                });

            response.Incomes = siteIncomes
                .Concat(deviceIncomes)
                .Concat(throwIncomes)
                .Concat(otherIncomes);
        }

        #endregion
    }
}