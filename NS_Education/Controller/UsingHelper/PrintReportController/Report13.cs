using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report13;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地預估銷售月報表的處理。
    /// </summary>
    public class Report13 : IPrintReport<Report13_Input_APIItem, Report13_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report13_Output_Row_APIItem>> GetResultAsync(
            Report13_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .GroupJoin(dbContext.M_Resver_TimeSpan
                            .Include(rts => rts.D_TimeSpan)
                            .Where(rts => rts.TargetTable == tableName),
                        rs => rs.BSID,
                        rts => rts.TargetID,
                        (rs, rts) => new { rs, rts }
                    )
                    .SelectMany(e => e.rts.DefaultIfEmpty(), (e, rts) => new { e.rs, rts })
                    .AsQueryable();

                query = query.OrderBy(e => e.rs.RSID);

                var results = await query.ToArrayAsync();

                Report13_Output_APIItem response = new Report13_Output_APIItem();
                response.SetByInput(input);

                response.Items = results
                    .Select(e => new Report13_Output_Row_APIItem
                    {
                        SiteCode = e.rs.B_SiteData?.Code ?? "",
                        SiteName = e.rs.B_SiteData?.Title ?? "",
                        SiteTimeSpanUnitPrice = e.rs.QuotedPrice,
                        SiteUnitPrice = e.rs.B_SiteData?.UnitPrice ?? 0,
                        TimeSpan = e.rts.D_TimeSpan?.Title ?? "",
                        Quantity = ,
                        TotalPrice = 0
                    })
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                return response;
            }
        }
    }
}