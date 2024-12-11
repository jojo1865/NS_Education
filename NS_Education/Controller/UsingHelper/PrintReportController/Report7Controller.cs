using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report7;
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
                    .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                    .Include(rh => rh.Resver_Other)
                    .Include(rh => rh.Resver_Other.Select(ro => ro.B_OrderCode))
                    .Where(rh => !rh.DeleteFlag)
                    .Where(rh => startTime <= rh.SDate && rh.EDate < endTime)
                    .AsQueryable();

                // 特殊情況：如果 RHID 只有「0」，視為沒有篩選。
                if (input.RHID != null && input.RHID.Any(id => id.IsAboveZero()))
                    query = query.Where(rh => input.RHID.Contains(rh.RHID));

                if (input.CustomerName.HasContent())
                {
                    // 如果可以拆成兩段，額外增加對照 Code
                    string[] split = input.CustomerName.Split(' ');
                    bool hasMultipleParts = split.Length >= 2;
                    string firstPart = split.FirstOrDefault() ?? "";

                    query = query
                        .Where(rh => rh.Customer.TitleC.Contains(input.CustomerName)
                                     || hasMultipleParts && rh.Customer.Code.Contains(firstPart));
                }

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
                    IsFirstTrade = firstTrades.GetValueOrDefault(rh.CID)?.RHID == rh.RHID,
                    CustomerType = ((CustomerType)rh.Customer.TypeFlag).GetTypeFlagName(),
                    MkSales = rh.BusinessUser.Name,
                    OpSales = rh.BusinessUser1.Name,
                    EventName = rh.Title,
                    PersonTime = rh.PeopleCt * ((rh.EDate.Date - rh.SDate.Date).Days + 1),
                    SiteCapacityTotal = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Select(rs => rs.B_SiteData)
                        .Where(bs => bs.ActiveFlag && !bs.DeleteFlag)
                        .Sum(bs => (int?)bs.BasicSize) ?? 0,
                    FoodPrice = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .SelectMany(rs => rs.Resver_Throw)
                        .Where(rt => !rt.DeleteFlag)
                        .Sum(rt => (int?)rt.QuotedPrice) ?? 0,
                    ResidencePrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Where(ro => ro.B_OrderCode.ActiveFlag && !ro.B_OrderCode.DeleteFlag)
                        .Where(ro => ro.B_OrderCode.CodeType == ((int)OrderCodeType.PartnerItem).ToString())
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    InsurancePrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Where(ro => ro.PrintTitle != null)
                        .Where(ro => ro.PrintTitle.Contains("保險"))
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    TransportationPrice = rh.Resver_Other
                        .Where(ro => !ro.DeleteFlag)
                        .Where(ro => ro.PrintTitle != null)
                        .Where(ro => ro.PrintTitle.Contains("交通費"))
                        .Sum(ro => (int?)ro.QuotedPrice) ?? 0,
                    // 其他收費包含：
                    // 1. 其他收費項目
                    // 2. 設備
                    OtherPrice = (rh.Resver_Other
                                     .Where(ro => !ro.DeleteFlag)
                                     .Sum(ro => (int?)ro.QuotedPrice) ?? 0)
                                 +
                                 (rh.Resver_Site
                                     .Where(rs => !rs.DeleteFlag)
                                     .SelectMany(rs => rs.Resver_Device)
                                     .Where(rd => !rd.DeleteFlag)
                                     .Sum(rd => (int?)rd.QuotedPrice) ?? 0),
                    Deposit = rh.Resver_Bill
                        .Where(rb => !rb.DeleteFlag)
                        .Where(rb => rb.PayFlag)
                        .Sum(rb => (int?)rb.Price) ?? 0,
                    Discount = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Sum(GetDiscount) ?? 0,
                    TotalPrice = rh.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Sum(GetSiteTotalPrice) ?? 0
                }).ToList();

                foreach (Report7_Output_Row_APIItem item in response.Items)
                {
                    item.OtherPrice -= item.TransportationPrice + item.InsurancePrice + item.ResidencePrice;
                }

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.SortWithInput(input).Skip(input.GetStartIndex())
                    .Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        private int? GetDiscount(Resver_Site rs)
        {
            // 內部/通訊處，全折
            if (isInternal(rs))
                return rs.QuotedPrice;

            return rs.FixedPrice - rs.QuotedPrice;
        }

        private static bool isInternal(Resver_Site rs)
        {
            return rs.Resver_Head.Customer.TypeFlag == (int)CustomerType.Internal
                   || rs.Resver_Head.Customer.TypeFlag == (int)CustomerType.CommDept;
        }

        private int? GetSiteTotalPrice(Resver_Site rs)
        {
            // 內部/通訊處，全折
            if (isInternal(rs))
                return 0;

            return rs.QuotedPrice;
        }

        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<ActionResult> GetExcel(Report7_Input_APIItem input)
        {
            input.NowPage = 0; // 報表時強制全查
            CommonResponseForPagedList<Report7_Output_Row_APIItem> data = await GetResultAsync(input);

            if (data == null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "營運報表明細",
                Columns = 21
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo();

            excelBuilder.CreateHeader(info);

            // 組合查詢條件的文字
            IEnumerable<string> conditions = new[]
                {
                    input.RHID?.Any() ?? false ? $"預約單號={String.Join(",", input.RHID)}" : null,
                    String.Join("~", new[] { input.StartDate, input.EndDate }.Where(d => d.HasContent()).Distinct()),
                    input.CustomerName.HasContent() ? $"主辦單位={input.CustomerName}" : null,
                    input.State.HasValue ? $"預約狀態={((ReserveHeadGetListState?)input.State).GetChineseName()}" : null
                }.Where(s => s.HasContent())
                .ToArray();

            if (conditions.Any())
            {
                excelBuilder.CreateRow()
                    .SetValue(0, "查詢條件：");
                foreach (string condition in conditions)
                {
                    excelBuilder.NowRow()
                        .CombineCells(1, 4)
                        .SetValue(1, condition);
                    excelBuilder.CreateRow();
                }
            }
            else
            {
                // 下一行寫死的東西要長在查詢條件的下一行，如果沒有查詢條件，會跟 header 重疊，所以沒有查詢條件時固定再寫一行空的，以騰出空間
                excelBuilder.CreateRow();
            }

            excelBuilder.NowRow()
                .CombineCells(12, 15)
                .SetValue(12, "*總營收=餐飲費+其他收費+總場租")
                .SetValue(17, "A")
                .SetValue(18, "B")
                .SetValue(19, "A+B")
                .Align(17, HorizontalAlignment.Center)
                .Align(18, HorizontalAlignment.Center)
                .Align(19, HorizontalAlignment.Center);

            // 表格

            excelBuilder.StartDefineTable<Report7_Output_Row_APIItem>()
                .SetDataRows(data.Items)
                .StringColumn(0, "起始日", i => i.StartDate)
                .StringColumn(1, "結束日", i => i.EndDate)
                .StringColumn(2, "預約單號", i => i.RHID.ToString())
                .StringColumn(3, "客戶代號", i => i.CustomerCode)
                .StringColumn(4, "客戶名稱", i => i.HostName)
                .StringColumn(5, "活動名稱", i => i.EventName)
                .StringColumn(6, "首次交易日", i => i.FirstTradeDate)
                .StringColumn(7, "MK", i => i.MkSales)
                .StringColumn(8, "OP", i => i.OpSales)
                .StringColumn(9, "類別", i => i.CustomerType)
                .NumberColumn(10, "人次", i => i.PersonTime, true)
                .NumberColumn(11, "教室使用人次", i => i.SiteCapacityTotal, true)
                .NumberColumn(12, "總營收", i => i.TotalProfit, true)
                .NumberColumn(13, "餐飲費", i => i.FoodPrice, true)
                .NumberColumn(14, "其他收費", i => i.OtherPrice, true)
                .NumberColumn(15, "總場租", i => i.TotalPrice, true)
                .NumberColumn(16, "訂金", i => i.Deposit, true)
                .NumberColumn(17, "場租折扣", i => i.Discount, true)
                .NumberColumn(18, "總場租(未稅)", i => i.TotalPriceWithoutTax, true)
                .NumberColumn(19, "帳面場租(未稅)", i => i.AccountPrice, true)
                .StringColumn(20, "首次交易", i => i.IsFirstTrade ? "首次" : "")
                .AddToBuilder(excelBuilder);

            return excelBuilder.GetFile();
        }

        #endregion

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report7_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}