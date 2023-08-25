using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report14;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地實際使用統計表的處理。
    /// </summary>
    public class Report14Controller : PublicClass, IPrintReport<Report14_Input_APIItem, Report14_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report14_Output_Row_APIItem>> GetResultAsync(
            Report14_Input_APIItem input)
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

                query = query.OrderBy(e => e.rs.RSID);

                var results = await query.ToArrayAsync();

                Report14_Output_APIItem response = new Report14_Output_APIItem();
                response.SetByInput(input);

                int total = results.Length;

                response.Items = results
                    .Where(e => e.rts != null)
                    .GroupBy(e => new { e.rs.BSID, e.rts.DTSID })
                    .Select(e => new Report14_Output_Row_APIItem
                    {
                        SiteType = e.Max(grouping => grouping.rs.B_SiteData.B_Category.TitleC),
                        SiteCode = e.Max(grouping => grouping.rs.B_SiteData.Code),
                        SiteName = e.Max(grouping => grouping.rs.B_SiteData.Title),
                        TimeSpan = e.Max(grouping => grouping.rts.D_TimeSpan.Title),
                        UseCount = e.Count(),
                        UseRate = (Decimal.Divide(e.Count(), total)).ToString("P")
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
        public async Task<string> Get(Report14_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}