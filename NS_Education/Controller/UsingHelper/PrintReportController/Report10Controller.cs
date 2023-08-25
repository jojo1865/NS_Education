using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report10;
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
    public class Report10Controller : PublicClass, IPrintReport<Report10_Input_APIItem, Report10_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report10_Output_Row_APIItem>> GetResultAsync(
            Report10_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime targetDate = input.TargetDate?.ParseDateTime() ?? DateTime.Today;
                targetDate = targetDate.Date;

                var query = dbContext.CustomerVisit
                    .Include(cv => cv.Customer)
                    .Include(cv => cv.B_StaticCode)
                    .Include(cv => cv.B_StaticCode1)
                    .Include(cv => cv.BusinessUser)
                    .Where(cv => !cv.DeleteFlag)
                    .Where(cv => DbFunctions.TruncateTime(cv.VisitDate) == targetDate)
                    .Where(cv => cv.BSCID15 != null)
                    .AsQueryable();

                var results = await query.ToArrayAsync();

                Report10_Output_APIItem response = new Report10_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(cv => new Report10_Output_Row_APIItem
                    {
                        CustomerCode = cv.Customer.Code,
                        CustomerName = cv.Customer.TitleC,
                        TargetTitle = cv.TargetTitle,
                        VisitMethod = cv.B_StaticCode1.Title,
                        VisitDate = cv.VisitDate.ToString("yyyy/MM/dd"),
                        Agent = cv.BusinessUser.Name,
                        Title = cv.Title,
                        Description = cv.Description,
                        AfterNote = cv.AfterNote,
                        NoDealReason = cv.B_StaticCode.Title
                    })
                    .Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount())
                    .ToList();
                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = results.Count();
                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report10_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}