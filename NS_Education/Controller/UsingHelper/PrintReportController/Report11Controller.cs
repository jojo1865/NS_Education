using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
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
    public class Report11Controller : PublicClass,
        IPrintReport<Report11_Input_APIItem, IDictionary<string, string>>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<IDictionary<string, string>>> GetResultAsync(
            Report11_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();

                // startDate 跟 endDate 不得差距超過 31 天
                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;

                // endDate 未輸入時，自動帶入今天
                DateTime endTime = input.EndDate?.ParseDateTime().Date ?? DateTime.Now.AddDays(1).Date;
                DateTime earliestTimeAllowed = endTime.AddDays(-31);

                if (startTime < earliestTimeAllowed)
                    startTime = earliestTimeAllowed;

                input.StartDate = startTime.ToFormattedStringDate();
                input.EndDate = endTime.ToFormattedStringDate();

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .Include(rs => rs.Resver_Head)
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

                var results = await query
                    .OrderBy(e => e.rs.RSID)
                    .ToArrayAsync();

                var siteData = await dbContext.B_SiteData
                    .Include(sd => sd.B_Category)
                    .Where(sd => sd.ActiveFlag && !sd.DeleteFlag)
                    .OrderBy(sd => sd.BSID)
                    .ToArrayAsync();

                var timeSpans = await dbContext.D_TimeSpan
                    .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                    .ToArrayAsync();

                Report11_Output_APIItem response = new Report11_Output_APIItem();
                response.SetByInput(input);

                // 欄位：
                // Type
                // SiteName
                // Time
                // yyyy-MM-dd
                // yyyy-MM-dd
                foreach (B_SiteData sd in siteData)
                {
                    foreach (D_TimeSpan dts in timeSpans)
                    {
                        IDictionary<string, string> newRow = new Dictionary<string, string>();
                        response.Items.Add(newRow);

                        newRow.Add("Type", sd.B_Category.TitleC);
                        newRow.Add("SiteName", sd.Title);
                        newRow.Add("Time", dts.Title);

                        foreach (DateTime dt in startTime.Range(endTime))
                        {
                            newRow.Add(dt.ToFormattedStringDate(),
                                results
                                    .Where(g => g.rs.BSID == sd.BSID)
                                    .Where(g => g.rts.DTSID == dts.DTSID)
                                    .Where(g => g.rs.TargetDate.Date == dt.Date)
                                    .Select(g => g.rs.Resver_Head.CustomerTitle)
                                    .FirstOrDefault());
                        }
                    }
                }

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.SortWithInput(input).Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount()).ToList();

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