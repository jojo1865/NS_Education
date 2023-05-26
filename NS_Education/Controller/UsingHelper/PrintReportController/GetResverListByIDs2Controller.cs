using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds2;
using NS_Education.Models.Entities;
using NS_Education.Models.Utilities.PrintReport.GetResverListByIDs2;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 處理 /PrintReport/GetResverListByIDs2 的端點。<br/>
    /// 實際 Route 請參考 RouteConfig。
    /// </summary>
    public class GetResverListByIDs2Controller : PublicClass,
        IGetListAll<Resver_Head, PrintReport_GetResverListByIds2_Input_APIItem,
            PrintReport_GetResverListByIds2_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<PrintReport_GetResverListByIds2_Input_APIItem> _getListAllHelper;

        public GetResverListByIDs2Controller()
        {
            _getListAllHelper =
                new GetListAllHelper<GetResverListByIDs2Controller, Resver_Head,
                    PrintReport_GetResverListByIds2_Input_APIItem, PrintReport_GetResverListByIds2_Output_Row_APIItem>(
                    this);
        }

        #endregion

        #region GetResverListByIDs2

        // 實際 Route 請參考 RouteConfig。
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> GetList(PrintReport_GetResverListByIds2_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(PrintReport_GetResverListByIds2_Input_APIItem input)
        {
            bool isInputValid = input.StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Id != null && i.Id.Any(), () => AddError(EmptyNotAllowed("欲查詢之預約單 ID 集合")))
                .Validate(i => i.Id.Distinct().Count() == i.Id.Count, () => AddError(CopyNotAllowed("欲查詢之預約單 ID 集合")))
                .IsValid();

            // 檢查所有 RHID 是否都存在
            bool isValid = isInputValid && // short-circuit
                           input.Id.Aggregate(true, (result, id) =>
                               result & // 一定走過所有資料，以便一次顯示所有找不到的錯誤訊息
                               id.StartValidate()
                                   .Validate(_ => DC.Resver_Head.Any(rh => !rh.DeleteFlag && rh.RHID == id),
                                       () => AddError(NotFound($"預約單 ID {id}")))
                                   .IsValid()
                           );

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head> GetListAllOrderedQuery(
            PrintReport_GetResverListByIds2_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.Resver_Bill)
                .Include(rh => rh.Customer)
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Other)
                .AsQueryable();

            query = query.Where(rh => input.Id.Contains(rh.RHID));

            return query.OrderBy(rh => rh.RHID);
        }

        public async Task<PrintReport_GetResverListByIds2_Output_Row_APIItem> GetListAllEntityToRow(Resver_Head entity)
        {
            List<PrintReport_GetResverListByIds2_PayItem> payItems = GetListAllGetPayItems(entity);


            var row = new PrintReport_GetResverListByIds2_Output_Row_APIItem
            {
                RHID = entity.RHID,
                PrintDate = DateTime.Now.ToFormattedStringDateTime(),
                Code = entity.Code ?? entity.RHID.ToString(),
                CustomerTitle = entity.CustomerTitle ?? "",
                ContactName = entity.ContactName ?? "",
                Title = entity.Title ?? "",
                TotalPrice = payItems.Sum(pi => pi.Price),
                PaidPrice = entity.Resver_Bill
                    .Where(rb => rb.PayFlag && !rb.DeleteFlag)
                    .Sum(rb => rb.Price),
                Compilation = entity.Customer?.Compilation ?? "",
                Items = GetListAllPopulatePayItems(payItems)
            };

            return await Task.FromResult(row);
        }

        private static List<PrintReport_GetResverListByIds2_PayItem_APIItem> GetListAllPopulatePayItems(
            List<PrintReport_GetResverListByIds2_PayItem> payItems)
        {
            // 依照 printTitle 做 group 之後，依此產生 response 內容
            return payItems.GroupBy(pi => pi.PrintTitle)
                .Select(g => new PrintReport_GetResverListByIds2_PayItem_APIItem
                {
                    Title = g.Key ?? "",
                    Items = g.Select(pi => new PrintReport_GetResverListByIds2_PayItemDetail_APIItem
                    {
                        Date = pi.TargetDate.ToFormattedStringDate(),
                        Title = pi.PrintTitle ?? "",
                        Price = pi.Price
                    }).ToList()
                }).ToList();
        }

        private static List<PrintReport_GetResverListByIds2_PayItem> GetListAllGetPayItems(Resver_Head entity)
        {
            IEnumerable<PrintReport_GetResverListByIds2_PayItem> payItems = entity.Resver_Site
                .Where(rb => !rb.DeleteFlag)
                .Select(
                    rs => new PrintReport_GetResverListByIds2_PayItem
                    {
                        PrintTitle = rs.PrintTitle,
                        PrintNote = rs.PrintNote,
                        TargetDate = rs.TargetDate,
                        Price = rs.QuotedPrice
                    }
                );

            payItems = payItems.Concat(entity.Resver_Site.SelectMany(rs => rs.Resver_Throw)
                .Where(rt => !rt.DeleteFlag)
                .Select(rt => new PrintReport_GetResverListByIds2_PayItem
                {
                    PrintTitle = rt.PrintTitle,
                    PrintNote = rt.PrintNote,
                    TargetDate = rt.TargetDate,
                    Price = rt.QuotedPrice + rt.Resver_Throw_Food.Sum(rtf => rtf.Price * rtf.Ct)
                }));

            payItems = payItems.Concat(entity.Resver_Site.SelectMany(rs => rs.Resver_Device)
                .Where(rd => !rd.DeleteFlag)
                .Select(rd => new PrintReport_GetResverListByIds2_PayItem
                {
                    PrintTitle = rd.PrintTitle,
                    PrintNote = rd.PrintNote,
                    TargetDate = rd.TargetDate,
                    Price = rd.QuotedPrice
                }));

            payItems = payItems.Concat(entity.Resver_Other
                .Where(ro => !ro.DeleteFlag)
                .Select(ro => new PrintReport_GetResverListByIds2_PayItem
                {
                    PrintTitle = ro.PrintTitle,
                    PrintNote = ro.PrintNote,
                    TargetDate = ro.TargetDate,
                    Price = ro.QuotedPrice
                }));

            return payItems.ToList();
        }

        #endregion
    }
}