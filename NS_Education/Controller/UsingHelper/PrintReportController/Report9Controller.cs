using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report9;
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
    /// 客戶歷史資料報表的處理。
    /// </summary>
    public class Report9Controller : PublicClass, IPrintReport<Report9_Input_APIItem, Report9_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report9_Output_Row_APIItem>> GetResultAsync(
            Report9_Input_APIItem input)
        {
            // 如果 CommDept, Internal, External 有任何一者為 true 時
            // null 視為 false

            if (input.CommDept is true || input.External is true || input.Internal is true)
            {
                input.CommDept = input.CommDept ?? false;
                input.External = input.External ?? false;
                input.Internal = input.Internal ?? false;
            }
            else
            {
                input.CommDept = input.CommDept ?? true;
                input.External = input.External ?? true;
                input.Internal = input.Internal ?? true;
            }

            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

                IQueryable<Resver_Head> query = dbContext.Resver_Head
                    .Include(rh => rh.Customer)
                    .Include(rh => rh.Resver_Site)
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Head))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Head).Select(rh2 => rh2.M_Resver_TimeSpan))
                    .Include(rh =>
                        rh.Resver_Site
                            .Select(rs => rs.Resver_Head)
                            .Select(rh2 => rh2.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan)))
                    .Include(rh =>
                        rh.Resver_Site
                            .Select(rs => rs.Resver_Head)
                            .Select(rh2 => rh2.Customer))
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => startTime <= rh.SDate && rh.EDate <= endTime)
                    .Where(rh => (input.Internal.Value && rh.Customer.TypeFlag == (int)CustomerType.Internal)
                                 || (input.External.Value && rh.Customer.TypeFlag == (int)CustomerType.External)
                                 || (input.CommDept.Value && rh.Customer.TypeFlag == (int)CustomerType.CommDept)
                    )
                    .AsQueryable();

                if (input.CID != null)
                    query = query.Where(rh => input.CID.Contains(rh.CID));

                if (input.CustomerName != null)
                    query = query.Where(rh => rh.Customer.TitleC.Contains(input.CustomerName));

                if (input.CustomerCode.HasContent())
                    query = query.Where(rh => rh.Customer.Code.Contains(input.CustomerCode));

                if (input.BSCID6.IsAboveZero())
                    query = query.Where(rh => rh.Customer.BSCID6 == input.BSCID6);

                if (input.ContactName.HasContent())
                    query = query.Where(rh => rh.Customer.ContectName.Contains(input.ContactName));

                Resver_Head[] result = await query
                    .OrderBy(rh => rh.RHID)
                    .ToArrayAsync();

                if (input.ContactData.HasContent())
                {
                    // 找出所有包含輸入內容的 M_Contect
                    string customerTableName = DC.GetTableName<Customer>();

                    HashSet<int> customerIds = DC.M_Contect
                        .Where(mc => mc.TargetTable == customerTableName)
                        .Where(mc => mc.ContectData.Contains(input.ContactData))
                        .Select(mc => mc.TargetID)
                        .Distinct()
                        .ToHashSet();

                    result = result.Where(r => customerIds.Contains(r.CID)).ToArray();
                }

                Report9_Output_APIItem response = new Report9_Output_APIItem();
                response.SetByInput(input);

                string resverSiteTableName = dbContext.GetTableName<Resver_Site>();

                // 取得聯絡方式資料

                string resverHeadTableName = dbContext.GetTableName<Resver_Head>();
                IEnumerable<int> resverHeadIds = result.Select(r => r.RHID);

                ILookup<int, M_Contect> rhIdToContacts = (await DC.M_Contect
                        .Where(mc => mc.TargetTable == resverHeadTableName)
                        .Where(mc => resverHeadIds.Contains(mc.TargetID))
                        .ToArrayAsync())
                    .ToLookup(mc => mc.TargetID, mc => mc);


                response.Items = result
                    .SelectMany(rh => rh.Resver_Site)
                    .GroupBy(rs => new { rs.RHID, rs.BSID, rs.TargetDate, rs.QuotedPrice })
                    .Select(grouping =>
                    {
                        string[] contactData = rhIdToContacts
                            .GetValueOrEmpty(grouping.Max(rs => rs.RHID))
                            .Select(mc => mc.ContectData)
                            .Where(s => s.HasContent())
                            .ToArray();

                        return new Report9_Output_Row_APIItem
                        {
                            RHID = grouping.Max(rs => rs.RHID),
                            HostName = grouping.Max(rs => rs.Resver_Head.Customer.TitleC),
                            EventName = grouping.Max(rs => rs.Resver_Head.Title),
                            TotalIncome = grouping.Max(rs => rs.Resver_Head.QuotedPrice),
                            Date = grouping.Max(rs => rs.TargetDate).ToFormattedStringDate(),
                            SiteName = grouping.Max(rs => rs.B_SiteData.Title),
                            EarliestTimeSpan = grouping.Max(rs => rs.Resver_Head.M_Resver_TimeSpan
                                .Where(rts => rts.TargetTable == resverSiteTableName)
                                .Where(rts => rts.TargetID == rs.RSID)
                                .OrderBy(rts => rts.D_TimeSpan.HourS)
                                .ThenBy(rts => rts.D_TimeSpan.MinuteS)
                                .Select(rts => rts.D_TimeSpan.Title)
                                .FirstOrDefault()) ?? "無",
                            LatestTimeSpan = grouping.Max(rs => rs.Resver_Head.M_Resver_TimeSpan
                                .Where(rts => rts.TargetTable == resverSiteTableName)
                                .Where(rts => rts.TargetID == rs.RSID)
                                .OrderByDescending(rts => rts.D_TimeSpan.HourE)
                                .ThenByDescending(rts => rts.D_TimeSpan.MinuteE)
                                .Select(rts => rts.D_TimeSpan.Title)
                                .FirstOrDefault()) ?? "無",
                            SitePrice = grouping.Max(rs => rs.QuotedPrice),
                            ContactName = grouping.Max(rs => rs.Resver_Head.ContactName),
                            ContactContent1 = contactData.ElementAtOrDefault(0),
                            ContactContent2 = contactData.ElementAtOrDefault(1),
                            Email = grouping.Max(rs => rs.Resver_Head.Customer.Email)
                        };
                    })
                    .OrderBy(row => row.RHID)
                    .ToList();

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
        public async Task<ActionResult> GetExcel(Report9_Input_APIItem input)
        {
            // 匯出時忽略頁數篩選
            input.NowPage = 0;

            CommonResponseForPagedList<Report9_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilderInfo info = await GetExcelBuilderInfo();
            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "客戶歷史資料報表",
                Columns = 13
            };

            excelBuilder.CreateHeader(info);

            excelBuilder.CreateRow()
                .SetValue(0, "查詢條件:")
                .SetValue(1, "查詢日期:")
                .Align(1, HorizontalAlignment.Right)
                .SetValue(2, new[]
                    {
                        input.StartDate,
                        input.EndDate
                    }.Distinct()
                    .Where(s => s.HasContent())
                    .StringJoin("~"));

            excelBuilder.CreateRow()
                .SetValue(1, "客戶名稱:")
                .Align(1, HorizontalAlignment.Right)
                .SetValue(2, input.CustomerName);

            excelBuilder.CreateRow()
                .SetValue(1, "客戶代號:")
                .Align(1, HorizontalAlignment.Right)
                .SetValue(2, input.CustomerCode);

            excelBuilder.CreateRow()
                .SetValue(1, "類別:")
                .Align(1, HorizontalAlignment.Right)
                .SetValue(2, new[]
                    {
                        input.Internal != null && input.Internal.Value ? CustomerType.Internal.GetTypeFlagName() : null,
                        input.External != null && input.External.Value ? CustomerType.External.GetTypeFlagName() : null,
                        input.CommDept != null && input.CommDept.Value ? CustomerType.CommDept.GetTypeFlagName() : null
                    }
                    .Where(s => s.HasContent())
                    .StringJoin(","));

            excelBuilder.CreateRow();

            bool IsSameHead(Report9_Output_Row_APIItem l, Report9_Output_Row_APIItem t) => l.RHID == t.RHID;

            excelBuilder.StartDefineTable<Report9_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "預約單號", i => i.RHID.ToString(), IsSameHead)
                .StringColumn(1, "主辦單位", i => i.HostName, IsSameHead)
                .StringColumn(2, "活動名稱", i => i.EventName, IsSameHead)
                .NumberColumn(3, "總營收", i => i.TotalIncome, true, IsSameHead)
                .StringColumn(4, "日期", i => i.Date)
                .StringColumn(5, "場地", i => i.SiteName)
                .StringColumn(6, "開始時段", i => i.EarliestTimeSpan)
                .StringColumn(7, "結束時段", i => i.LatestTimeSpan)
                .NumberColumn(8, "場地報價", i => i.SitePrice, true)
                .StringColumn(9, "聯絡人", i => i.ContactName, IsSameHead)
                .StringColumn(10, "聯絡方式", i => i.ContactContent1, IsSameHead)
                .StringColumn(11, "聯絡方式", i => i.ContactContent2, IsSameHead)
                .StringColumn(12, "E-mail", i => i.Email, IsSameHead)
                .AddToBuilder(excelBuilder);


            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report9_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}