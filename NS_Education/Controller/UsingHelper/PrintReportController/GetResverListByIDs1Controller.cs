using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds1;
using NS_Education.Models.Entities;
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
        IGetListAll<Resver_Head, PrintReport_GetResverListByIds1_Input_APIItem,
            PrintReport_GetResverListByIds1_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<PrintReport_GetResverListByIds1_Input_APIItem> _getListAllHelper;

        public GetResverListByIDs1Controller()
        {
            _getListAllHelper =
                new GetListAllHelper<GetResverListByIDs1Controller, Resver_Head,
                    PrintReport_GetResverListByIds1_Input_APIItem,
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
            bool isInputValid = input.StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Id != null && i.Id.Any(),
                    () => AddError(EmptyNotAllowed("欲查詢之預約單 ID 集合", nameof(input.Id))))
                .Validate(i => i.Id.Distinct().Count() == i.Id.Count,
                    () => AddError(CopyNotAllowed("欲查詢之預約單 ID 集合", nameof(input.Id))))
                .IsValid();

            // 檢查所有 RHID 是否都存在
            bool isValid = isInputValid && // short-circuit
                           input.Id.Aggregate(true, (result, id) =>
                               result & // 一定走過所有資料，以便一次顯示所有找不到的錯誤訊息
                               id.StartValidate()
                                   .Validate(_ => DC.Resver_Head.Any(rh => !rh.DeleteFlag && rh.RHID == id),
                                       () => AddError(NotFound($"預約單 ID {id}", nameof(input.Id))))
                                   .IsValid()
                           );

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head> GetListAllOrderedQuery(
            PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.Customer)
                .Include(rh => rh.M_Resver_TimeSpan)
                .Include(rh => rh.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_StaticCode))
                .AsQueryable();

            query = query.Where(rh => input.Id.Contains(rh.RHID));

            return query.OrderBy(rh => rh.RHID);
        }

        public async Task<PrintReport_GetResverListByIds1_Output_Row_APIItem> GetListAllEntityToRow(Resver_Head entity)
        {
            // 取得這筆資料的 M_Contact
            string tableName = DC.GetTableName<Resver_Head>();
            M_Contect[] contacts = await DC.M_Contect
                .Where(mc => mc.TargetTable == tableName && mc.TargetID == entity.RHID)
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
                Compilation = entity.Customer?.Compilation ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                PeopleCt = entity.PeopleCt,
                QuotedPrice = entity.QuotedPrice,
                SiteItems = GetListAllPopulateRowSiteItems(entity)
            };
            return row;
        }

        private List<PrintReport_GetResverListByIds1_SiteItem_APIItem> GetListAllPopulateRowSiteItems(
            Resver_Head entity)
        {
            return entity.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .Select(rs => new PrintReport_GetResverListByIds1_SiteItem_APIItem
                {
                    RSID = rs.RSID,
                    Date = rs.TargetDate.ToFormattedStringDate(),
                    SiteTitle = rs.B_SiteData?.Title ?? "",
                    TableTitle = rs.B_StaticCode?.Title ?? "",
                    FixedPrice = rs.FixedPrice,
                    QuotedPrice = rs.QuotedPrice,
                    TimeSpanItems = GetListAllPopulateRowSiteItemTimeSpanItems(entity, rs),
                    DeviceItems = GetListAllPopulateRowSiteItemDeviceItems(rs)
                }).ToList();
        }

        private List<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem> GetListAllPopulateRowSiteItemTimeSpanItems(
            Resver_Head entity, Resver_Site rs)
        {
            return entity.M_Resver_TimeSpan
                .Where(rts =>
                    rts.TargetTable == DC.GetTableName<Resver_Site>()
                    && rts.TargetID == rs.RSID)
                .Select(rts => new PrintReport_GetResverListByIds1_TimeSpanItem_APIItem
                {
                    DTSID = rts.DTSID,
                    Title = rts.D_TimeSpan != null ? rts.D_TimeSpan.Title ?? "" : "",
                    TimeS = rts.D_TimeSpan != null
                        ? (rts.D_TimeSpan.HourS, rts.D_TimeSpan.MinuteS).ToFormattedHourAndMinute()
                        : "",
                    TimeE = rts.D_TimeSpan != null
                        ? (rts.D_TimeSpan.HourE, rts.D_TimeSpan.MinuteE).ToFormattedHourAndMinute()
                        : "",
                    Minutes = rts.D_TimeSpan != null
                        ? (rts.D_TimeSpan.HourS, rts.D_TimeSpan.MinuteS).GetMinutesUntil((rts.D_TimeSpan.HourE,
                            rts.D_TimeSpan.MinuteE))
                        : 0,
                }).ToList();
        }

        private static List<PrintReport_GetResverListByIds1_DeviceItem_APIItem>
            GetListAllPopulateRowSiteItemDeviceItems(Resver_Site rs)
        {
            return rs.Resver_Device
                .Where(rd => !rd.DeleteFlag)
                .Select(rd => new PrintReport_GetResverListByIds1_DeviceItem_APIItem
                {
                    RDID = rd.RDID,
                    TargetDate = rd.TargetDate.ToFormattedStringDate(),
                    BD_Title = rd.B_Device.Title ?? "",
                    SortNo = rd.SortNo,
                    Note = rd.Note ?? ""
                }).ToList();
        }

        #endregion
    }
}