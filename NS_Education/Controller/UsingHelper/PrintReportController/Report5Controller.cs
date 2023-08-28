using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report5;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 餐飲明細表的處理。
    /// </summary>
    public class Report5Controller : PublicClass, IPrintReport<Report5_Input_APIItem, Report5_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report5_Output_Row_APIItem>> GetResultAsync(
            Report5_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startDate = input.StartDate?.ParseDateTime() ?? SqlDateTime.MinValue.Value;
                DateTime endDate = input.EndDate?.ParseDateTime() ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Throw_Food
                    .Include(rtf => rtf.Resver_Throw)
                    .Include(rtf => rtf.Resver_Throw.Resver_Site)
                    .Include(rtf => rtf.Resver_Throw.Resver_Site.Resver_Head)
                    .Include(rtf => rtf.B_Partner)
                    .Include(rtf => rtf.B_StaticCode)
                    .Include(rtf => rtf.D_FoodCategory)
                    .Where(rtf => !rtf.Resver_Throw.DeleteFlag)
                    .Where(rtf => !rtf.Resver_Throw.Resver_Site.DeleteFlag)
                    .Where(rtf => !rtf.Resver_Throw.Resver_Site.Resver_Head.DeleteFlag)
                    .Where(rtf => startDate <= rtf.Resver_Throw.TargetDate && rtf.Resver_Throw.TargetDate <= endDate)
                    .AsQueryable();

                if (input.Partner.HasContent())
                    query = query
                        .Where(rtf => rtf.B_Partner.Title.Contains(input.Partner)
                                      || rtf.B_Partner.Compilation.Contains(input.Partner));

                var results = await query
                    .OrderBy(rtf => rtf.Resver_Throw.TargetDate)
                    .ToArrayAsync();

                Report5_Output_APIItem response = new Report5_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(rtf => new Report5_Output_Row_APIItem
                {
                    ReserveDate = rtf.Resver_Throw.TargetDate.ToString("yy/MM/dd"),
                    RHID = rtf.Resver_Throw.Resver_Site.RHID,
                    EventName = rtf.Resver_Throw.Resver_Site.Resver_Head.Title,
                    PartnerName = rtf.B_Partner.Title,
                    CuisineType = rtf.B_StaticCode.Title,
                    CuisineName = rtf.D_FoodCategory.Title,
                    HostName = rtf.Resver_Throw.Resver_Site.Resver_Head.CustomerTitle,
                    ReservedQuantity = rtf.Ct,
                    UnitPrice = rtf.UnitPrice,
                    QuotedPrice = rtf.Price
                }).ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report5_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}