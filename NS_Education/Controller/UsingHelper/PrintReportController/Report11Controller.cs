using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report11;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地庫存狀況表的處理。
    /// </summary>
    public class Report11Controller : PublicClass, IPrintReport<Report11_Input_APIItem, Report11_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report11_Output_Row_APIItem>> GetResultAsync(
            Report11_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();
                DateTime targetDate = input.TargetDate?.ParseDateTime() ?? DateTime.Today;
                targetDate = targetDate.Date;

                var query = dbContext.B_SiteData
                    .Include(sd => sd.B_Category)
                    .Include(sd => sd.Resver_Site)
                    .Include(sd => sd.Resver_Site.Select(rs => rs.Resver_Head))
                    .Where(sd => sd.ActiveFlag && !sd.DeleteFlag);

                var result = await query.ToArrayAsync();

                var timeSpans = await dbContext.D_TimeSpan
                    .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                    .ToArrayAsync();
                Report11_Output_APIItem response = new Report11_Output_APIItem();
                response.SetByInput(input);

                response.Items = result.GroupBy(sd => sd.BCID)
                    .Select(grouping => new Report11_Output_Row_APIItem
                    {
                        Name = grouping.Max(g => g.B_Category.TitleC),
                        Sites = grouping.Select(s => new Report11_Output_Row_Site_APIItem
                        {
                            Name = s.Title,
                            TimeSpans = timeSpans.Select(dts => new Report11_Output_Row_TimeSpan_APIItem
                            {
                                Name = dts.Title,
                                Customer = s.Resver_Site
                                    .Where(rs => !rs.DeleteFlag)
                                    .Where(rs => rs.TargetDate.Date == targetDate)
                                    .Where(rs => dbContext.M_Resver_TimeSpan
                                        .Where(rts => rts.TargetTable == tableName)
                                        .Any(rts => rts.TargetID == rs.RSID)
                                    ).Max(rs => rs.Resver_Head.CustomerTitle)
                            }).ToArray()
                        }).ToArray()
                    })
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);

                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report11_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}