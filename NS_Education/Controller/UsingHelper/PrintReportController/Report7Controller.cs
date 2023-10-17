using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report7;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 營運報表明細的處理。
    /// </summary>
    public class Report7Controller : PublicClass, IPrintReport<Report7_Input_APIItem, Report7_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report7_Output_Row_APIItem>> GetResultAsync(
            Report7_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime().Date.AddDays(1) ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Head
                    .Include(rh => rh.Customer)
                    .Include(rh => rh.BusinessUser)
                    .Include(rh => rh.BusinessUser1)
                    .Include(rh => rh.Resver_Site)
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                    .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                    .Include(rh => rh.Resver_Other)
                    .Include(rh => rh.Resver_Other.Select(ro => ro.B_OrderCode))
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => startTime <= rh.SDate && rh.EDate < endTime)
                    .AsQueryable();

                if (input.RHID != null)
                    query = query.Where(rh => input.RHID.Contains(rh.RHID));

                if (input.CustomerName.HasContent())
                    query = query
                        .Where(rh => rh.CustomerTitle.Contains(input.CustomerName));

                // 刪除的資料 State 不會變，所以要做特別處理
                if (input.State == (int)ReserveHeadGetListState.Deleted)
                    query = query.Where(rh => rh.DeleteFlag);
                else if (input.State.IsAboveZero())
                    query = query.Where(rh => rh.State == input.State);

                var results = await query
                    .OrderBy(rh => rh.SDate)
                    .ThenByDescending(rh => rh.EDate)
                    .ToArrayAsync();

                Report7_Output_APIItem response = new Report7_Output_APIItem();
                response.SetByInput(input);

                var customerIds = results.Select(rh => rh.CID);
                var firstTrades = await dbContext.Resver_Head
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => customerIds.Contains(rh.CID))
                    .OrderBy(rh => rh.CreDate)
                    .GroupBy(rh => rh.CID)
                    .ToDictionaryAsync(grouping => grouping.Key, grouping => grouping.First());

                response.Items = results.Select(rh => new Report7_Output_Row_APIItem
                {
                    StartDate = rh.SDate.ToFormattedStringDate(),
                    EndDate = rh.EDate.ToFormattedStringDate(),
                    RHID = rh.RHID,
                    CustomerCode = rh.Customer.Code,
                    HostName = rh.Customer.TitleC,
                    FirstTradeDate = firstTrades.GetValueOrDefault(rh.CID)?.CreDate.ToFormattedStringDate() ?? "無資料",
                    CustomerType = rh.Customer.InFlag
                        ? "內部單位"
                        : "外部單位",
                    MkSales = rh.BusinessUser.Name,
                    OpSales = rh.BusinessUser1.Name,
                    EventName = rh.Title,
                    PersonTime = rh.PeopleCt * ((rh.EDate.Date - rh.SDate.Date).Days + 1),
                    TotalProfit = 0,
                    FoodPrice = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .SelectMany(rs => rs.Resver_Throw)
                        .Where(rt => !rt.DeleteFlag)
                        .SelectMany(rt => rt.Resver_Throw_Food)
                        .Sum(rtf => (int?)rtf.Price * rtf.Ct) ?? 0,
                    ResidencePrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Where(ro => ro.B_OrderCode.ActiveFlag && !ro.B_OrderCode.DeleteFlag)
                        .Where(ro => ro.B_OrderCode.CodeType == ((int)OrderCodeType.PartnerItem).ToString())
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    InsurancePrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Where(ro => ro.PrintTitle.Contains("保險"))
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    TransportationPrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Where(ro => ro.PrintTitle.Contains("交通費"))
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    OtherPrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    TotalPrice = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Sum(rs => (int?)rs.QuotedPrice) ?? 0,
                    Deposit = rh.Resver_Bill
                        .Where(rb => !rb.DeleteFlag)
                        .Where(rb => rb.PayFlag)
                        .Sum(rb => (int?)rb.Price) ?? 0,
                    Discount = Math.Max(0, rh.FixedPrice - rh.QuotedPrice),
                    TotalPriceWithoutTax = rh.QuotedPrice
                }).ToList();

                foreach (Report7_Output_Row_APIItem item in response.Items)
                {
                    item.OtherPrice -= item.TransportationPrice + item.InsurancePrice + item.ResidencePrice;
                }

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report7_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}