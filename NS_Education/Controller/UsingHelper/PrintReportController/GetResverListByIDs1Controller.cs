using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.PrintReport.GetResverListByIds1;
using NS_Education.Models.Entities;
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
    /// 處理 /PrintReport/GetResverListByIDs1 的端點。<br/>
    /// 實際 Route 請參考 RouteConfig。
    /// </summary>
    public class GetResverListByIDs1Controller : PublicClass,
        IGetListAll<Resver_Head, PrintReport_GetResverListByIds1_Input_APIItem, PrintReport_GetResverListByIds1_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<PrintReport_GetResverListByIds1_Input_APIItem> _getListAllHelper;

        public GetResverListByIDs1Controller()
        {
            _getListAllHelper =
                new GetListAllHelper<GetResverListByIDs1Controller, Resver_Head, PrintReport_GetResverListByIds1_Input_APIItem,
                    PrintReport_GetResverListByIds1_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetResverListByIDs1
        // 實際 Route 請參考 RouteConfig。
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> GetList(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Id != null && i.Id.Any(), () => AddError(EmptyNotAllowed("欲查詢之預約單 ID 集合")))
                .Validate(i => i.Id.Distinct().Count() == i.Id.Count, () => AddError(CopyNotAllowed("欲查詢之預約單 ID 集合")))
                .IsValid();

            // 檢查所有 RHID 是否都存在
            bool allIdValid = input.Id.Aggregate(true, (result, id) => result &
                id.StartValidate()
                    .Validate(_ => DC.Resver_Head.Any(rh => !rh.DeleteFlag && rh.RHID == id), () => AddError(NotFound($"預約單 ID {id}")))
                    .IsValid()
            );

            return await Task.FromResult(isValid && allIdValid);
        }

        public IOrderedQueryable<Resver_Head> GetListAllOrderedQuery(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.C)
                .Include(rh => rh.M_Resver_TimeSpan)
                .ThenInclude(rts => rts.DTS)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Device)
                .ThenInclude(rd => rd.BD)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.BS)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.BSC)
                .AsQueryable();

            query = query.Where(rh => input.Id.Contains(rh.RHID));

            return query.OrderBy(rh => rh.RHID);
        }

        public async Task<PrintReport_GetResverListByIds1_Output_Row_APIItem> GetListAllEntityToRow(Resver_Head entity)
        {
            // 取得這筆資料的 M_Contact
            M_Contect[] contacts = await DC.M_Contect
                .Where(mc => mc.TargetTable == DC.GetTableName<Resver_Head>() && mc.TargetID == entity.RHID)
                .OrderBy(mc => mc.SortNo)
                .Take(2)
                .ToArrayAsync();
            
            var row = GetListAllPopulateRow(entity, contacts);

            return await Task.FromResult(row);
        }

        private PrintReport_GetResverListByIds1_Output_Row_APIItem GetListAllPopulateRow(Resver_Head entity,
            IReadOnlyList<M_Contect> contacts)
        {
            var row = new PrintReport_GetResverListByIds1_Output_Row_APIItem
            {
                RHID = entity.RHID,
                Code = entity.Code ?? entity.RHID.ToString(),
                CustomerTitle = entity.CustomerTitle ?? "",
                ContactTitle1 = contacts.Count >= 1
                    ? ContactTypeController.GetContactTypeTitle(contacts[0].ContectType) ?? ""
                    : "",
                ContactValue1 = contacts.Count >= 1 ? contacts[0].ContectData ?? "" : "",
                ContactTitle2 = contacts.Count >= 2
                    ? ContactTypeController.GetContactTypeTitle(contacts[1].ContectType) ?? ""
                    : "",
                ContactValue2 = contacts.Count >= 2 ? contacts[1].ContectData ?? "" : "",
                ContactName = entity.ContactName ?? "",
                Compilation = entity.C?.Compilation ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                PeopleCt = entity.PeopleCt,
                QuotedPrice = entity.QuotedPrice,
                SiteItems = GetListAllPopulateRowSiteItems(entity)
            };
            return row;
        }

        private List<PrintReport_GetResverListByIds1_SiteItem_APIItem> GetListAllPopulateRowSiteItems(Resver_Head entity)
        {
            return entity.Resver_Site.Select(rs => new PrintReport_GetResverListByIds1_SiteItem_APIItem
            {
                RSID = rs.RSID,
                Date = rs.TargetDate.ToFormattedStringDate(),
                SiteTitle = rs.BS?.Title ?? "",
                TableTitle = rs.BSC?.Title ?? "",
                FixedPrice = rs.FixedPrice,
                QuotedPrice = rs.QuotedPrice,
                TimeSpanItems = GetListAllPopulateRowSiteItemTimeSpanItems(entity, rs),
                DeviceItems = GetListAllPopulateRowSiteItemDeviceItems(rs)
            }).ToList();
        }

        private List<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem> GetListAllPopulateRowSiteItemTimeSpanItems(Resver_Head entity, Resver_Site rs)
        {
            return entity.M_Resver_TimeSpan
                .Where(rts => 
                    rts.TargetTable == DC.GetTableName<Resver_Site>() 
                    && rts.TargetID == rs.RSID)
                .Select(rts => new PrintReport_GetResverListByIds1_TimeSpanItem_APIItem
                {
                    DTSID = rts.DTSID,
                    Title = rts.DTS != null ? rts.DTS.Title ?? "" : "",
                    TimeS = rts.DTS != null ? StandardColumnExtensionMethods.ToFormattedHourAndMinute(rts.DTS.HourS, rts.DTS.MinuteS) : "",
                    TimeE = rts.DTS != null ? StandardColumnExtensionMethods.ToFormattedHourAndMinute(rts.DTS.HourE, rts.DTS.MinuteE) : "",
                    Minutes = rts.DTS != null ? StandardColumnExtensionMethods.GetMinutesBetween(rts.DTS.HourS, rts.DTS.MinuteS, rts.DTS.HourE, rts.DTS.MinuteE): 0,
                }).ToList();
        }

        private static List<PrintReport_GetResverListByIds1_DeviceItem_APIItem> GetListAllPopulateRowSiteItemDeviceItems(Resver_Site rs)
        {
            return rs.Resver_Device.Select(rd => new PrintReport_GetResverListByIds1_DeviceItem_APIItem
            {
                RDID = rd.RDID,
                TargetDate = rd.TargetDate.ToFormattedStringDate(),
                BD_Title = rd.BD?.Title ?? "",
                SortNo = rd.SortNo,
                Note = rd.Note ?? ""
            }).ToList();
        }

        #endregion
    }
}