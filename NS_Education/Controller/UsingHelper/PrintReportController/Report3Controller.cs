using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report3;
using NS_Education.Models.Entities;
using NS_Education.Models.Utilities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;

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
                                            t.Cell().AlignRight().Text($"{income.FixedPrice:N0}");
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

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report3_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            
            CommonResponseForPagedList<Report3_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data is null)
                return GetContentResult();

            ExcelBuilderInfo info = await GetExcelBuilderInfo(data.Items.Count);
            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "授權簽核表",
                Columns = 9
            };

            foreach (Report3_Output_Row_APIItem item in data.Items)
            {
                GetExcel_MakePage(excelBuilder, item, info);
                info.NowPage++;
                excelBuilder.CreateRow();
            }

            return excelBuilder.GetFile();
        }

        private void GetExcel_MakePage(ExcelBuilder e, Report3_Output_Row_APIItem item, ExcelBuilderInfo info)
        {
            e.CreateHeader(info);

            e.CreateRow()
                .Align(0, HorizontalAlignment.Left)
                .SetValue(0, "預約單號:")
                .SetValue(1, item.RHID);

            e.CreateRow()
                .CombineCells(1, 3)
                .SetValue(0, "主辦單位:")
                .SetValue(1, item.HostName);

            e.CreateRow()
                .CombineCells(1, 3)
                .SetValue(0, "活動名稱:")
                .SetValue(1, item.EventName);

            string datesJoined = String.Join("~",
                new[] { item.StartDate, item.EndDate }.Where(s => s.HasContent()).Distinct());

            e.CreateRow()
                .CombineCells(1, 2)
                .SetValue(0, "活動日期:")
                .SetValue(1, datesJoined);

            e.CreateRow()
                .SetValue(0, "人數: ")
                .SetValue(1, item.PeopleCt, CellType.Numeric)
                .SetValue(2, "人");

            e.CreateRow();

            e.CreateRow()
                .SetValue(0, "一、收入分析");

            e.StartDefineTable<Report3_Output_Row_Income_APIItem>()
                .StringColumn(0, "", i => i.Title)
                .NumberColumn(1, "場地定價", i => i.FixedPrice, true)
                .NumberColumn(3, "場地報價", i => i.QuotedPrice, true)
                .NumberColumn(6, "成本", i => i.FixedPrice, true)
                .NumberColumn(7, "價差", i => i.Difference, true)
                .StringColumn(8, "備註", i => "")
                .SetDataRows(item.Incomes)
                .AddToBuilder(e);

            e.CreateRow();

            e.CreateRow()
                .SetValue(0, "二、細項說明");

            ILookup<string, Report3_Output_Row_Detail_APIItem> detailGroups =
                item.Details.ToLookup(d => d.TypeName, d => d);

            foreach (IGrouping<string, Report3_Output_Row_Detail_APIItem> g in detailGroups)
            {
                Report3_Output_Row_Detail_APIItem firstRow = g.First();
                string typeName = firstRow.TypeName;
                string typeFieldName = firstRow.OverrideColumnTypeName;
                string subTypeFieldName = firstRow.SubTypeName;

                e.CreateRow()
                    .SetValue(0, $"#{typeName}費用");

                e.StartDefineTable<Report3_Output_Row_Detail_APIItem>()
                    .SetDataRows(g)
                    .StringColumn(0, "日期", i => i.Date)
                    .StringColumn(1, "時段", i => String.Join("~",
                        new[] { i.TimeSpans.FirstOrDefault(), i.TimeSpans.LastOrDefault() }
                            .Where(s => s.HasContent())
                            .Distinct()))
                    .StringColumn(3, typeFieldName, i => i.Title)
                    .StringColumn(5, subTypeFieldName, i => i.SubType)
                    .NumberColumn(7, "定價", i => i.FixedPrice, true)
                    .NumberColumn(8, "報價", i => i.QuotedPrice, true)
                    .AddToBuilder(e);

                e.CreateRow();

                // 如果是場地，特殊輸出固定的文字

                if (g.Key != "場地") continue;

                e.CreateRow()
                    .SetValue(0, "◎");

                e.CreateRow()
                    .CombineCells(0, 1)
                    .SetValue(0, "場地租金優惠折扣      折");

                e.CreateRow()
                    .CombineCells(0, 2)
                    .SetValue(0, "特惠專案客戶場地優惠扣折     折");

                e.CreateRow();
            }

            e.CreateRow()
                .SetValue(0, "◎");

            e.CreateRow()
                .SetValue(0, "製表者")
                .SetValue(3, "業務服務組長")
                .CombineCells(6, 7)
                .SetValue(6, "教育訓練中心主管");

            e.CreateRow();

            e.CreateRow()
                .DrawBorder(BorderDirection.Bottom, 0, 0)
                .DrawBorder(BorderDirection.Bottom, 3, 3)
                .DrawBorder(BorderDirection.Bottom, 6, 7);
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
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.D_FoodCategory))))
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
                .Select(grouping =>
                {
                    string type = grouping.Key.IsFood ? "餐飲" : "訓練";

                    return new Report3_Output_Row_Detail_APIItem
                    {
                        TypeName = type,
                        Date = grouping.Max(rt => rt.TargetDate).ToFormattedStringDate(),
                        TimeSpans = GetResverTimeSpans(head, typeof(Resver_Throw), grouping.Select(rt => rt.RTID)),
                        Title = grouping.Key.IsFood
                            // 餐飲要從子表抓
                            ? grouping.SelectMany(rt => rt.Resver_Throw_Food)
                                .Max(rtf => rtf.D_FoodCategory.Title)
                            // 其他情況直接從行程本身抓
                            : grouping.Max(rt => rt.Title),
                        SubTypeName = "類型",
                        SubType = type,
                        FixedPrice = grouping.Max(rt => rt.FixedPrice),
                        QuotedPrice = grouping.Max(rt => rt.QuotedPrice)
                    };
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
                .GroupBy(rt => new {rt.RSID, isFood = rt.Resver_Throw_Food.Any()})
                .Select(grouping =>
                    new Report3_Output_Row_Income_APIItem
                    {
                        Title = grouping.Key.isFood ? "餐飲" : "訓練",
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