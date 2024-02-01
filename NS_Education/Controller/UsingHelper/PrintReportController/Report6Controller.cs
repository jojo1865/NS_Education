using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report6;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 人次統計表的處理。
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
                    .Include(rh => rh.Resver_Site)
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => startDate <= rh.SDate && rh.EDate <= endDate)
                    .Where(rh => (input.Internal && rh.Customer.TypeFlag == (int)CustomerType.Internal)
                                 || (input.External && rh.Customer.TypeFlag == (int)CustomerType.External)
                                 || (input.CommDept && rh.Customer.TypeFlag == (int)CustomerType.CommDept))
                    .AsQueryable();

                // 特殊情況：如果 RHID 只有「0」，視為沒有篩選。
                if (input.RHID != null && input.RHID.Any(id => id.IsAboveZero()))
                    query = query.Where(rh => input.RHID.Contains(rh.RHID));

                if (input.CustomerName.HasContent())
                    query = query
                        .Where(rh => rh.Customer.TitleC.Contains(input.CustomerName));

                // 刪除的資料 State 不會變，所以要做特別處理
                if (input.State == (int)ReserveHeadGetListState.Deleted)
                    query = query.Where(rh => rh.DeleteFlag);
                else if (input.State.IsAboveZero())
                    query = query.Where(rh => rh.State == input.State);

                var results = await query
                    .OrderBy(rh => rh.SDate)
                    .ThenByDescending(rh => rh.EDate)
                    .ToArrayAsync();

                Report6_Output_APIItem response = new Report6_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(rh => new Report6_Output_Row_APIItem
                {
                    StartDate = rh.SDate.ToString("yyyy/MM/dd"),
                    EndDate = rh.EDate.ToString("yyyy/MM/dd"),
                    RHID = rh.RHID,
                    CustomerCode = rh.Customer.Code,
                    HostName = rh.Customer.TitleC,
                    CustomerType = ((CustomerType)rh.Customer.TypeFlag).GetTypeFlagName(),
                    MkSales = rh.BusinessUser.Name,
                    OpSales = rh.BusinessUser1.Name,
                    EventName = rh.Title,
                    PeopleCt = rh.PeopleCt,
                    PersonTime = rh.PeopleCt * ((rh.EDate.Date - rh.SDate.Date).Days + 1),
                    SiteCapacityTotal = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Select(rs => rs.B_SiteData)
                        .Where(bs => bs.ActiveFlag && !bs.DeleteFlag)
                        .Sum(bs => (int?)bs.BasicSize) ?? 0
                }).ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.SortWithInput(input).Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report6_Input_APIItem input)
        {
            CommonResponseForPagedList<Report6_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data is null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "人次統計",
                Columns = 12
            };

            excelBuilder.CreateHeader(await GetExcelBuilderInfo());

            // 組合查詢條件的文字
            IEnumerable<string> conditions = new[]
                {
                    input.RHID?.Any() ?? false ? $"預約單號={String.Join(",", input.RHID)}" : null,
                    String.Join("~", new[] { input.StartDate, input.EndDate }.Where(d => d.HasContent()).Distinct()),
                    input.CustomerName.HasContent() ? $"主辦單位={input.CustomerName}" : null
                }.Where(s => s.HasContent())
                .ToArray();

            excelBuilder.CreateRow()
                .SetValue(4, "課程對象:")
                .SetValue(5, input.Internal ? "內部單位" : "")
                .SetValue(6, input.External ? "外部單位" : "")
                .SetValue(7, input.CommDept ? "通訊處" : "");

            if (conditions.Any())
            {
                excelBuilder.NowRow()
                    .SetValue(0, "查詢條件：");
                foreach (string condition in conditions)
                {
                    excelBuilder.NowRow()
                        .CombineCells(1, 2)
                        .SetValue(1, condition);

                    excelBuilder.CreateRow();
                }
            }

            // 表格

            excelBuilder.StartDefineTable<Report6_Output_Row_APIItem>()
                .StringColumn(0, "開始日期", i => i.StartDate)
                .StringColumn(1, "結束日期", i => i.EndDate)
                .StringColumn(2, "預約單號", i => i.RHID.ToString())
                .StringColumn(3, "客戶代號", i => i.CustomerCode)
                .StringColumn(4, "主辦單位", i => i.HostName)
                .StringColumn(5, "類別", i => i.CustomerType)
                .StringColumn(6, "MK", i => i.MkSales)
                .StringColumn(7, "OP", i => i.OpSales)
                .StringColumn(8, "活動名稱", i => i.EventName)
                .NumberColumn(9, "人數", i => i.PeopleCt, true)
                .NumberColumn(10, "人次", i => i.PersonTime, true)
                .NumberColumn(11, "教室使用人次", i => i.SiteCapacityTotal, true)
                .SetDataRows(data.Items)
                .AddToBuilder(excelBuilder);

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report6_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}