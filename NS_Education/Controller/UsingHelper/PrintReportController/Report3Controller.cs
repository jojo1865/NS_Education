using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report3;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 客戶授權簽核表的處理。
    /// </summary>
    public class Report3Controller : PublicClass, IPrintReport<Report3_Input_APIItem, Report3_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report3_Output_Row_APIItem>> GetResultAsync(
            Report3_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                Report3_Output_APIItem response = new Report3_Output_APIItem();
                response.SetByInput(input);

                Resver_Head head = await GetHead(input, dbContext);

                if (head is null)
                {
                    AddError(NotFound("預約單", nameof(input.RHID)));
                    return null;
                }

                await AssignBasicFields(response, head);
                AssignIncomes(head, response);
                AssignDetails(head, response);

                return response;
            }
        }

        private static async Task<Resver_Head> GetHead(Report3_Input_APIItem input, NsDbContext dbContext)
        {
            return await dbContext.Resver_Head.AsQueryable()
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
                .Include(rh => rh.M_Resver_TimeSpan)
                .Include(rh => rh.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Where(rh => !rh.DeleteFlag)
                .FirstOrDefaultAsync(rh => rh.RHID == input.RHID);
        }

        private async Task AssignBasicFields(Report3_Output_APIItem response, Resver_Head head)
        {
            response.StartDate = head.SDate.ToFormattedStringDate();
            response.EndDate = head.EDate.ToFormattedStringDate();
            response.PeopleCt = head.PeopleCt;
            response.HostName = head.CustomerTitle;
            response.EventName = head.Title;
            response.UID = GetUid();
            response.Username = await GetUserNameByID(response.UID);
        }

        private static void AssignDetails(Resver_Head head, Report3_Output_APIItem response)
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
                .GroupBy(rt => new { rt.TargetDate, rt.BSCID, rt.QuotedPrice })
                .Select(grouping => new Report3_Output_Row_Detail_APIItem
                {
                    TypeName = "行程",
                    Date = grouping.Max(rt => rt.TargetDate).ToFormattedStringDate(),
                    TimeSpans = GetResverTimeSpans(head, typeof(Resver_Throw), grouping.Select(rt => rt.RTID)),
                    Title = grouping.Max(rt => rt.Title),
                    SubTypeName = "類型",
                    SubType = grouping.Max(rt => rt.B_StaticCode.Title),
                    FixedPrice = grouping.Max(rt => rt.FixedPrice),
                    QuotedPrice = grouping.Max(rt => rt.QuotedPrice)
                });

            var otherDetails = head.Resver_Other
                .Where(ro => !ro.DeleteFlag)
                .GroupBy(ro => new { ro.TargetDate, ro.PrintTitle, ro.QuotedPrice })
                .Select(grouping => new Report3_Output_Row_Detail_APIItem
                {
                    TypeName = "其他",
                    Date = grouping.Max(ro => ro.TargetDate).ToFormattedStringDate(),
                    TimeSpans = GetResverTimeSpans(head, typeof(Resver_Other), grouping.Select(ro => ro.ROID)),
                    Title = grouping.Max(ro => ro.PrintTitle),
                    SubTypeName = "帳單列印說明",
                    SubType = grouping.Max(ro => ro.PrintNote),
                    FixedPrice = grouping.Max(ro => ro.FixedPrice),
                    QuotedPrice = grouping.Max(ro => ro.QuotedPrice)
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

        private static void AssignIncomes(Resver_Head head, Report3_Output_APIItem response)
        {
            var siteIncomes = head.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .GroupBy(rs => rs.RHID)
                .Select(grouping => new Report3_Output_Row_Income_APIItem
                {
                    Title = "場地",
                    FixedPrice = grouping.Sum(rs => (int?)rs.FixedPrice) ?? 0,
                    QuotedPrice = grouping.Sum(rs => (int?)rs.QuotedPrice) ?? 0,
                    UnitPrice = grouping.Sum(rs => (int?)rs.UnitPrice) ?? 0
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
                        FixedPrice = grouping.Sum(rs => (int?)rs.FixedPrice) ?? 0,
                        QuotedPrice = grouping.Sum(rs => (int?)rs.QuotedPrice) ?? 0,
                        UnitPrice = grouping.Sum(rs => (int?)rs.UnitPrice) ?? 0
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
                        FixedPrice = grouping.Sum(rs => (int?)rs.FixedPrice) ?? 0,
                        QuotedPrice = grouping.Sum(rs => (int?)rs.QuotedPrice) ?? 0,
                        UnitPrice = grouping.Sum(rs => (int?)rs.UnitPrice) ?? 0
                    });

            var otherIncomes = head.Resver_Other
                .Where(ro => !ro.DeleteFlag)
                .GroupBy(ro => ro.RHID)
                .Select(grouping => new Report3_Output_Row_Income_APIItem
                {
                    Title = "其他",
                    FixedPrice = grouping.Sum(rs => (int?)rs.FixedPrice) ?? 0,
                    QuotedPrice = grouping.Sum(rs => (int?)rs.QuotedPrice) ?? 0,
                    UnitPrice = grouping.Sum(rs => (int?)rs.UnitPrice) ?? 0
                });

            response.Incomes = siteIncomes
                .Concat(deviceIncomes)
                .Concat(throwIncomes)
                .Concat(otherIncomes);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report3_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}