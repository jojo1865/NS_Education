using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report6;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 未成交原因分析的處理。
    /// </summary>
    public class Report6Controller : PublicClass, IPrintReport<Report6_Input_APIItem, Report6_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report6_Output_Row_APIItem>> GetResultAsync(
            Report6_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startDate = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endDate = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Head
                    .Include(rh => rh.Customer)
                    .Include(rh => rh.BusinessUser)
                    .Include(rh => rh.BusinessUser1)
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => startDate <= rh.SDate && rh.EDate <= endDate)
                    .Where(rh => (input.Internal && input.External)
                                 || (input.Internal && rh.Customer.InFlag == true)
                                 || (input.External && rh.Customer.InFlag == false))
                    .AsQueryable();

                var results = await query.ToArrayAsync();

                Report6_Output_APIItem response = new Report6_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(rh => new Report6_Output_Row_APIItem
                {
                    StartDate = rh.SDate.ToString("yyyy/MM/dd"),
                    EndDate = rh.EDate.ToString("yyyy/MM/dd"),
                    RHID = rh.RHID,
                    CustomerCode = rh.Customer.Code,
                    HostName = rh.Customer.TitleC,
                    CustomerType = rh.Customer.InFlag ? "內部單位" : "外部單位",
                    MkSales = rh.BusinessUser.Name,
                    OpSales = rh.BusinessUser1.Name,
                    EventName = rh.Title,
                    PeopleCt = rh.PeopleCt,
                    PersonTime = rh.PeopleCt * ((rh.EDate.Date - rh.SDate.Date).Days + 1)
                }).ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);

                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report6_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}