using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report9;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

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
                        rh.Resver_Site.Select(rs => rs.Resver_Head)
                            .Select(rh2 => rh2.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan)))
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => startTime <= rh.SDate && rh.EDate <= endTime)
                    .Where(rh => (input.Internal && input.External)
                                 || (input.Internal && rh.Customer.InFlag == true)
                                 || (input.External && rh.Customer.InFlag == false))
                    .AsQueryable();

                if (input.CID != null)
                    query = query.Where(rh => input.CID.Contains(rh.CID));

                if (input.CustomerName != null)
                    query = query.Where(rh => rh.ContactName.Contains(input.CustomerName));

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

                string tableName = dbContext.GetTableName<Resver_Site>();

                response.Items = result
                    .SelectMany(rh => rh.Resver_Site)
                    .GroupBy(rs => new { rs.RHID, rs.BSID, rs.QuotedPrice })
                    .Select(grouping => new Report9_Output_Row_APIItem
                    {
                        RHID = grouping.Max(rs => rs.RHID),
                        HostName = grouping.Max(rs => rs.Resver_Head.CustomerTitle),
                        EventName = grouping.Max(rs => rs.Resver_Head.Title),
                        TotalIncome = grouping.Max(rs => rs.Resver_Head.QuotedPrice),
                        Date = grouping.Max(rs => rs.TargetDate).ToFormattedStringDate(),
                        SiteName = grouping.Max(rs => rs.B_SiteData.Title),
                        EarliestTimeSpan = grouping.Max(rs => rs.Resver_Head.M_Resver_TimeSpan
                            .Where(rts => rts.TargetTable == tableName)
                            .Where(rts => rts.TargetID == rs.RSID)
                            .OrderBy(rts => rts.D_TimeSpan.HourS)
                            .ThenBy(rts => rts.D_TimeSpan.MinuteS)
                            .Select(rts => rts.D_TimeSpan.Title)
                            .FirstOrDefault()) ?? "無",
                        LatestTimeSpan = grouping.Max(rs => rs.Resver_Head.M_Resver_TimeSpan
                            .Where(rts => rts.TargetTable == tableName)
                            .Where(rts => rts.TargetID == rs.RSID)
                            .OrderByDescending(rts => rts.D_TimeSpan.HourE)
                            .ThenByDescending(rts => rts.D_TimeSpan.MinuteE)
                            .Select(rts => rts.D_TimeSpan.Title)
                            .FirstOrDefault()) ?? "無",
                        SitePrice = grouping.Max(rs => rs.QuotedPrice)
                    })
                    .OrderBy(row => row.RHID)
                    .ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();

                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report9_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}