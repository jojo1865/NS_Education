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
                    .Include(rh => rh.Resver_Site)
                    .Include(rh => rh.Resver_Bill)
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
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
                    HostName = rh.CustomerTitle,
                    EventTitle = rh.Title,
                    RHID = rh.RHID,
                    StartDate = rh.SDate.FormatAsRocYyyMmDd(),
                    EndDate = rh.EDate.FormatAsRocYyyMmDd(),
                    SiteNames = rh.Resver_Site.Select(rs => rs.B_SiteData.Title).Distinct(),
                    MKT = rh.MKT,
                    Owner = rh.Owner,
                    ParkingNote = rh.ParkingNote,
                    Contact = rh.ContactName,
                    PayStatus = GetBills(rh),
                    Sites = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Select((rs, index) =>
                        {
                            return new Report2_Output_Row_Site_APIItem
                            {
                                Title = $"場地 {index + 1}：{rs.B_SiteData?.Title ?? ""}",
                                Date = rs.TargetDate.FormatAsRocYyyMmDd(),
                                Lines = GetLines(rs),
                                SeatImage = rs.SeatImage != null ? Convert.ToBase64String(rs.SeatImage) : null
                            };
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

            result = result.Append($"{rs.B_SiteData?.B_StaticCode1?.Title ?? "無資料"}：{rs.B_SiteData?.MaxSize ?? 0} 人" +
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
    }
}