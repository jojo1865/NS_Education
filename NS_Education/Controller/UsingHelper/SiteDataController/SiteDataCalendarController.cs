using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.SiteData.GetListForCalendar;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.SiteDataController
{
    /// <summary>
    /// 處理行事曆用的取得場地列表的 API。<br/>
    /// 雖然是場地列表，但實際上查詢主體是「預約單」而非「場地」。<br/>
    /// 但因為目前開的 Route 為 SiteData，因此還是歸類在 SiteDataController，<br/>
    /// 但處理的是預約單的 Entity。
    /// </summary>
    public class SiteDataCalendarController : PublicClass,
        IGetListPaged<Resver_Site, SiteData_GetListForCalendar_Input_APIItem,
            SiteData_GetListForCalendar_Output_Row_APIItem>
    {
        #region Initializaton

        private readonly IGetListPagedHelper<SiteData_GetListForCalendar_Input_APIItem> _getListPagedHelper;

        public SiteDataCalendarController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<SiteDataCalendarController, Resver_Site,
                    SiteData_GetListForCalendar_Input_APIItem,
                    SiteData_GetListForCalendar_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetList - For calendar

        // Route 為 /SiteData/GetCalendarList
        // 詳見 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(SiteData_GetListForCalendar_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetListForCalendar_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.TargetYear.IsInBetween(1911, 9999), () => AddError(WrongFormat("目標年分")))
                .Validate(i => i.TargetMonth.IsInBetween(1, 12), () => AddError(WrongFormat("目標月份")))
                .Validate(i => i.BSID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之場地 ID")))
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.RHID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之預約單 ID")))
                .SkipIfAlreadyInvalid()
                .ForceSkipIf(i => i.BSID <= 0)
                .ValidateAsync(async i => await DC.B_SiteData.ValidateIdExists(i.BSID, nameof(B_SiteData.BSID)),
                    () => AddError(NotFound("欲篩選之場地 ID")))
                .StopForceSkipping()
                .ForceSkipIf(i => i.CID <= 0)
                .ValidateAsync(async i => await DC.Customer.ValidateIdExists(i.CID, nameof(Customer.CID)),
                    () => AddError(NotFound("欲篩選之客戶 ID")))
                .StopForceSkipping()
                .ForceSkipIf(i => i.RHID <= 0)
                .ValidateAsync(async i => await DC.Resver_Head.ValidateIdExists(i.RHID, nameof(Resver_Head.RHID)),
                    () => AddError(NotFound("欲篩選之預約單 ID")))
                .StopForceSkipping()
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Site> GetListPagedOrderedQuery(SiteData_GetListForCalendar_Input_APIItem input)
        {
            var query = DC.Resver_Site
                .Include(rs => rs.Resver_Head)
                .Include(rs => rs.Resver_Head.M_Resver_TimeSpan)
                .Include(rs => rs.Resver_Head.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Include(rs => rs.B_SiteData)
                .AsQueryable();

            // 年份和月份
            query = query.Where(
                rs => rs.TargetDate.Year == input.TargetYear && rs.TargetDate.Month == input.TargetMonth);

            // 預約單號
            if (input.RHID.IsAboveZero())
                query = query.Where(rs => rs.RHID == input.RHID);

            // 場地 ID
            if (input.BSID.IsAboveZero())
                query = query.Where(rs => rs.BSID == input.BSID);

            // 客戶 ID
            if (input.CID.IsAboveZero())
                query = query.Where(rs => rs.Resver_Head.CID == input.CID);

            return query.OrderBy(rs => rs.TargetDate)
                .ThenBy(rs => rs.SortNo)
                .ThenBy(rs => rs.RHID)
                .ThenBy(rs => rs.RSID);
        }

        public async Task<SiteData_GetListForCalendar_Output_Row_APIItem> GetListPagedEntityToRow(Resver_Site entity)
        {
            return await Task.FromResult(new SiteData_GetListForCalendar_Output_Row_APIItem
                {
                    BSID = entity.BSID,
                    Code = entity.B_SiteData.Code ?? "",
                    Title = entity.B_SiteData.Title ?? "",
                    RHID = entity.RHID,
                    RSID = entity.RSID,
                    RSSortNo = entity.SortNo,
                    RHCode = entity.Resver_Head?.Code ?? "",
                    RHTitle = entity.Resver_Head?.Title ?? "",
                    CID = entity.Resver_Head?.CID ?? 0,
                    CustomerTitle = entity.Resver_Head?.CustomerTitle ?? "",
                    TargetDate = entity.TargetDate.ToFormattedStringDate(),
                    Items = entity.Resver_Head?.M_Resver_TimeSpan.Select(rts =>
                            new SiteData_GetListForCalendar_TimeSpan_APIItem
                            {
                                DTSID = rts.DTSID,
                                Title = rts.D_TimeSpan.Title ?? "",
                                SortNo = rts.SortNo
                            }).OrderBy(item => item.SortNo)
                        .ThenBy(item => item.DTSID)
                }
            );
        }

        #endregion
    }
}