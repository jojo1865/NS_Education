using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report16;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地使用一覽表的處理。
    /// </summary>
    public class Report16Controller : PublicClass, IPrintReport<Report16_Input_APIItem, Report16_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report16_Output_Row_APIItem>> GetResultAsync(
            Report16_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();

                DateTime startTime = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .Include(rs => rs.Resver_Head)
                    .Include(rs => rs.Resver_Head.Customer)
                    .Where(rs => !rs.DeleteFlag)
                    .Where(rs => !rs.Resver_Head.DeleteFlag)
                    .Where(rs => startTime <= rs.TargetDate && rs.TargetDate <= endTime)
                    .GroupJoin(dbContext.M_Resver_TimeSpan
                            .Include(rts => rts.D_TimeSpan)
                            .Where(rts => rts.TargetTable == tableName),
                        rs => rs.RSID,
                        rts => rts.TargetID,
                        (rs, rts) => new { rs, rts }
                    )
                    .SelectMany(e => e.rts.DefaultIfEmpty(), (e, rts) => new { e.rs, rts })
                    .AsQueryable();

                if (input.SiteName.HasContent())
                    query = query.Where(x => x.rs.B_SiteData.Title.Contains(input.SiteName));

                if (input.BCID.IsAboveZero())
                    query = query.Where(x => x.rs.B_SiteData.BCID == input.BCID);

                if (input.IsActive.HasValue)
                    query = query.Where(x => x.rs.B_SiteData.ActiveFlag == input.IsActive);

                if (input.BSCID1.IsAboveZero())
                    query = query.Where(x => x.rs.B_SiteData.BSCID1 == input.BSCID1);

                if (input.BasicSize.IsAboveZero())
                    query = query.Where(x => x.rs.B_SiteData.BasicSize >= input.BasicSize);

                query = query.OrderBy(e => e.rs.RSID);

                var results = await query.ToArrayAsync();

                Report16_Output_APIItem response = new Report16_Output_APIItem();
                response.SetByInput(input);

                response.Items = results
                    .Where(e => e.rts != null)
                    .GroupBy(e => new { e.rs.TargetDate, e.rs.BSID, e.rts.DTSID, e.rs.RHID, e.rs.QuotedPrice })
                    .Select(e => new Report16_Output_Row_APIItem
                    {
                        Date = e.Max(grouping => grouping.rs.TargetDate).ToString("yy/MM/dd"),
                        Site = e.Max(grouping => grouping.rs.B_SiteData.Title),
                        TimeSpan = e.Max(grouping => grouping.rts.D_TimeSpan.Title),
                        StartDate = e.Max(grouping => grouping.rs.Resver_Head.SDate).ToString("yy/MM/dd"),
                        EndDate = e.Max(grouping => grouping.rs.Resver_Head.EDate).ToString("yy/MM/dd"),
                        RHID = e.Max(grouping => grouping.rs.RHID),
                        CustomerCode = e.Max(grouping => grouping.rs.Resver_Head.Customer.Code),
                        Host = e.Max(grouping => grouping.rs.Resver_Head.CustomerTitle),
                        HostType = e.Max(grouping => grouping.rs.Resver_Head.Customer.InFlag ? "內部單位" : "外部單位"),
                        MKSales = e.Max(grouping => grouping.rs.Resver_Head.BusinessUser.Name),
                        OPSales = e.Max(grouping => grouping.rs.Resver_Head.BusinessUser1.Name),
                        EventName = e.Max(grouping => grouping.rs.Resver_Head.Title),
                        UnitPrice = e.Max(grouping => grouping.rs.B_SiteData.UnitPrice),
                        QuotedPrice = e.Max(grouping => grouping.rs.QuotedPrice)
                    })
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
        public async Task<string> Get(Report16_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}