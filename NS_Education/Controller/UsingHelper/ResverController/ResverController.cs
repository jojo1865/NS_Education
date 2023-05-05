using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Resver.GetAllInfoById;
using NS_Education.Models.APIItems.Resver.GetHeadList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public class ResverController : PublicClass, 
        IGetListPaged<Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>,
        IGetInfoById<Resver_Head, Resver_GetAllInfoById_Output_APIItem>,
        IDeleteItem<Resver_Head>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Resver_GetHeadList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        private readonly IDeleteItemHelper
            _deleteItemHelper;

        public ResverController()
        {
            _getListPagedHelper = new GetListPagedHelper<ResverController, Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<ResverController, Resver_Head, Resver_GetAllInfoById_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<ResverController, Resver_Head>(this);
        }

        #endregion

        #region GetHeadList

        // route 請參照 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Resver_GetHeadList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Resver_GetHeadList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.BSCID12.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之預約狀態 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head> GetListPagedOrderedQuery(Resver_GetHeadList_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.BSCID12Navigation)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(rh => rh.Code != null && rh.Code.Contains(input.Keyword));

            if (input.TargetDate.TryParseDateTime(out DateTime targetDate))
                query = query.Where(rh => rh.SDate.Date >= targetDate.Date);

            if (input.CID.IsAboveZero())
                query = query.Where(rh => rh.CID == input.CID);

            if (input.BSCID12.IsAboveZero())
                query = query.Where(rh => rh.BSCID12 == input.BSCID12);

            return query.OrderByDescending(rh => rh.SDate)
                .ThenByDescending(rh => rh.EDate)
                .ThenBy(rh => rh.RHID);
        }

        public async Task<Resver_GetHeadList_Output_Row_APIItem> GetListPagedEntityToRow(Resver_Head entity)
        {
            return await Task.FromResult(new Resver_GetHeadList_Output_Row_APIItem
            {
                RHID = entity.RHID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                CustomerTitle = entity.CustomerTitle ?? "",
                CustomerCode = entity.CustomerTitle ?? "",
                PeopleCt = entity.PeopleCt,
                BSCID12 = entity.BSCID12,
                BSCID12_Title = entity.BSCID12Navigation?.Title ?? ""
            });
        }

        #endregion

        #region GetInfoById

        // 確切 route 請參照 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<Resver_Head> GetInfoByIdQuery(int id)
        {
            return DC.Resver_Head
                .Include(rh => rh.BSCID12Navigation)
                .Include(rh => rh.BSCID11Navigation)
                .Include(rh => rh.MK_BU)
                .Include(rh => rh.OP_BU)
                // site
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.BS)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.BOC)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.BSC)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.RH)
                // resver_timespan
                .Include(rh => rh.M_Resver_TimeSpan)
                .ThenInclude(rts => rts.DTS)
                // site -> throw
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Throw)
                .ThenInclude(rt => rt.BSC)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Throw)
                .ThenInclude(rt => rt.BOC)
                // site -> throw -> throw_food
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Throw)
                .ThenInclude(rt => rt.Resver_Throw_Food)
                .ThenInclude(rtf => rtf.DFC)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Throw)
                .ThenInclude(rt => rt.Resver_Throw_Food)
                .ThenInclude(rtf => rtf.BSC)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Throw)
                .ThenInclude(rt => rt.Resver_Throw_Food)
                .ThenInclude(rtf => rtf.BP)
                // site -> device
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Device)
                .ThenInclude(rd => rd.BD)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Device)
                .ThenInclude(rd => rd.BOC)
                // otherItem
                .Include(rh => rh.Resver_Other)
                .ThenInclude(ro => ro.DOPI)
                .ThenInclude(dopi => dopi.BSC)
                .Include(rh => rh.Resver_Other)
                .ThenInclude(ro => ro.BOC)
                // bill
                .Include(rs => rs.Resver_Bill)
                .ThenInclude(rb => rb.BC)
                .Include(rs => rs.Resver_Bill)
                .ThenInclude(rb => rb.DPT)
                // GiveBack
                .Include(rb => rb.Resver_GiveBack)

                .Where(rh => rh.RHID == id);
        }

        public async Task<Resver_GetAllInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(Resver_Head entity)
        {
            var result = new Resver_GetAllInfoById_Output_APIItem
            {
                RHID = entity.RHID,
                BSCID12 = entity.BSCID12,
                BSC12_Title = entity.BSCID12Navigation?.Title ?? "",
                BSC12_List =
                    await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSCID12Navigation?.CodeType, entity.BSCID12),
                BSCID11 = entity.BSCID11,
                BSC11_Title = entity.BSCID11Navigation?.Title ?? "",
                BSC11_List =
                    await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSCID11Navigation?.CodeType, entity.BSCID11),
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                PeopleCt = entity.PeopleCt,
                CID = entity.CID,
                CustomerTitle = entity.CustomerTitle ?? "",
                C_List = await DC.Customer.GetCustomerSelectable(entity.CID),
                ContactName = entity.ContactName ?? "",
                ContactTypeList = ContactTypeController.GetContactTypeList(),
                MK_BUID = entity.MK_BUID,
                MK_BU_Name = entity.MK_BU.Name ?? "",
                MK_BU_List = await DC.BusinessUser.GetBusinessUserSelectable(entity.MK_BUID),
                MK_Phone = entity.MK_Phone ?? "",
                OP_BUID = entity.OP_BUID,
                OP_BU_Name = entity.OP_BU?.Name ?? "",
                OP_BU_List = await DC.BusinessUser.GetBusinessUserSelectable(entity.OP_BUID),
                OP_Phone = entity.OP_Phone ?? "",
                Note = entity.Note,
                FixedPrice = entity.FixedPrice,
                QuotedPrice = entity.QuotedPrice,
                ContactItems = GetAllInfoByIdPopulateContactItems(entity),
                SiteItems = GetAllInfoByIdPopulateSiteItems(entity),
                OtherItems = GetAllInfoByIdPopulateOtherItems(entity),
                BillItems = GetAllInfoByIdPopulateBillItems(entity),
                GiveBackItems = GetAllInfoByIdPopulateGiveBackItems(entity)
            };

            return await Task.FromResult(result);
        }

        private static List<Resver_GetAllInfoById_Output_GiveBackItem_APIItem> GetAllInfoByIdPopulateGiveBackItems(
            Resver_Head entity)
        {
            return entity.Resver_GiveBack.Select(gb => new Resver_GetAllInfoById_Output_GiveBackItem_APIItem
            {
                RGBID = gb.RGBID,
                Title = gb.Title ?? "",
                Description = gb.Description ?? "",
                PointDecimal = gb.PointDecimal
            }).ToList();
        }

        private List<Resver_GetAllInfoById_Output_BillItem_APIItem> GetAllInfoByIdPopulateBillItems(Resver_Head entity)
        {
            return entity.Resver_Bill.Select(rb => new Resver_GetAllInfoById_Output_BillItem_APIItem
            {
                RBID = rb.RBID,
                BCID = rb.BCID,
                BC_Title = rb.BC?.TitleC ?? rb.BC?.TitleE ?? "",
                BC_List = Task.Run(() => DC.B_Category.GetCategorySelectable(rb.BC?.CategoryType, rb.BCID)).Result,
                DPTID = rb.DPTID,
                DPT_Title = rb.DPT?.Title ?? "",
                DPT_List = Task.Run(() => DC.D_PayType.GetOtherPayItemSelectable(rb.DPTID)).Result,
                Price = rb.Price,
                Note = rb.Note ?? "",
                PayFlag = rb.PayFlag,
                PayDate = rb.PayDate.ToFormattedStringDateTime()
            }).ToList();
        }

        private List<Resver_GetAllInfoById_Output_OtherItem_APIItem> GetAllInfoByIdPopulateOtherItems(
            Resver_Head entity)
        {
            return entity.Resver_Other.Select(ro => new Resver_GetAllInfoById_Output_OtherItem_APIItem
            {
                ROID = ro.ROID,
                TargetDate = ro.TargetDate.ToFormattedStringDate(),
                DOPIID = ro.DOPIID,
                DOPI_Title = ro.DOPI?.Title ?? "",
                DOPI_List = Task.Run(() => DC.D_OtherPayItem.GetOtherPayItemSelectable(ro.DOPIID)).Result,
                BSCID = ro.DOPI?.BSCID ?? 0,
                BSC_Title = ro.DOPI?.BSC?.Title ?? "",
                BOCID = ro.BOCID,
                BOC_Code = ro.BOC?.Code ?? "",
                BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(ro.BOC?.CodeType, ro.BOCID)).Result,
                PrintTitle = ro.PrintTitle ?? "",
                PrintNote = ro.PrintNote ?? "",
                UnitPrice = ro.UnitPrice,
                FixedPrice = ro.FixedPrice,
                Ct = ro.Ct,
                QuotedPrice = ro.QuotedPrice,
                SortNo = ro.SortNo,
                Note = ro.Note ?? ""
            }).ToList();
        }

        private List<Resver_GetAllInfoById_Output_SiteItem_APIItem> GetAllInfoByIdPopulateSiteItems(Resver_Head entity)
        {
            return entity.Resver_Site
                .Select(rs => new Resver_GetAllInfoById_Output_SiteItem_APIItem
                {
                    RSID = rs.RSID,
                    TargetDate = rs.TargetDate.ToFormattedStringDate(),
                    BSID = rs.BSID,
                    BS_Title = rs.BS?.Title ?? "",
                    BOCID = rs.BOCID,
                    BOC_Code = rs.BOC?.Code ?? "",
                    BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(rs.BOC?.CodeType, rs.BOCID)).Result,
                    PrintTitle = rs.PrintTitle ?? "",
                    PrintNote = rs.PrintNote ?? "",
                    UnitPrice = rs.UnitPrice,
                    FixedPrice = rs.FixedPrice,
                    QuotedPrice = rs.QuotedPrice,
                    SortNo = rs.SortNo,
                    Note = rs.Note ?? "",
                    BSCID = rs.BSCID,
                    BSC_Title = rs.BSC?.Title ?? "",
                    BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(rs.BSC?.CodeType, rs.BSCID))
                        .Result,
                    TimeSpanItems = GetTimeSpanFromHead<Resver_Site>(rs.RH, rs.RSID),
                    ThrowItems = GetAllInfoByIdPopulateThrowItems(rs),
                    DeviceItems = GetAllInfoByIdPopulateDeviceItems(rs)
                })
                .OrderBy(rs => rs.SortNo)
                .ToList();
        }

        private List<Resver_GetAllInfoById_Output_DeviceItem_APIItem> GetAllInfoByIdPopulateDeviceItems(Resver_Site rs)
        {
            return rs.Resver_Device.Select(rd => new Resver_GetAllInfoById_Output_DeviceItem_APIItem
            {
                RDID = rd.RDID,
                TargetDate = rd.TargetDate.ToFormattedStringDate(),
                BDID = rd.BDID,
                BD_Title = rd.BD?.Title ?? "",
                BD_List = Task.Run(() => DC.B_Device.GetOtherPayItemSelectable(rd.BDID)).Result,
                BOCID = rd.BOCID,
                BOC_Code = rd.BOC?.Code ?? "",
                BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(rd.BOC?.CodeType, rd.BOCID)).Result,
                PrintTitle = rd.PrintTitle ?? "",
                PrintNote = rd.PrintNote ?? "",
                UnitPrice = rd.UnitPrice,
                FixedPrice = rd.FixedPrice,
                QuotedPrice = rd.QuotedPrice,
                SortNo = rd.SortNo,
                Note = rd.Note ?? "",
                TimeSpanItems = GetTimeSpanFromHead<Resver_Device>(rs.RH, rd.RDID)
            }).ToList();
        }

        private List<Resver_GetAllInfoById_Output_ThrowItem_APIItem> GetAllInfoByIdPopulateThrowItems(Resver_Site rs)
        {
            return rs.Resver_Throw
                .Select(rt => new Resver_GetAllInfoById_Output_ThrowItem_APIItem
                {
                    RTID = rt.RTID,
                    TargetDate = rt.TargetDate.ToFormattedStringDate(),
                    BSCID = rt.BSCID,
                    BSC_Title = rt.BSC?.Title ?? "",
                    BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(rt.BSC?.CodeType, rt.BSCID))
                        .Result,
                    Title = rt.Title ?? "",
                    BOCID = rt.BOCID,
                    BOC_Title = rt.BOC?.Title ?? "",
                    BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(rt.BOC?.CodeType, rt.BOCID)).Result,
                    PrintTitle = rt.PrintTitle ?? "",
                    PrintNote = rt.PrintNote ?? "",
                    UnitPrice = rt.UnitPrice,
                    FixedPrice = rt.FixedPrice,
                    QuotedPrice = rt.QuotedPrice,
                    SortNo = rt.SortNo,
                    Note = rt.Note,
                    TimeSpanItems = GetTimeSpanFromHead<Resver_Throw>(rs.RH, rt.RTID),
                    FoodItems = rt.BSC?.Title != DbConstants.ReseverThrowDineTitle
                        ? new List<Resver_GetAllInfoById_Output_FoodItem_APIItem>()
                        : GetALlInfoByIdPopulateFoodItems(rt)
                })
                .OrderBy(rt => rt.SortNo)
                .ToList();
        }

        private List<Resver_GetAllInfoById_Output_FoodItem_APIItem> GetALlInfoByIdPopulateFoodItems(Resver_Throw rt)
        {
            return rt.Resver_Throw_Food
                .Select(rtf => new Resver_GetAllInfoById_Output_FoodItem_APIItem
                {
                    RTFID = rtf.RTFID,
                    DFCID = rtf.DFCID,
                    DFC_Title = rtf.DFC?.Title,
                    DFC_List = Task.Run(() => DC.D_FoodCategory.GetFoodCategorySelectable(rtf.DFCID)).Result,
                    BSCID = rtf.BSCID,
                    BSC_Title = rtf.BSC?.Title,
                    BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(rtf.BSC?.CodeType, rtf.BSCID))
                        .Result,
                    BPID = rtf.BPID,
                    BP_Title = rtf.BP?.Title,
                    BP_List = Task.Run(() => DC.B_Partner.GetPartnerSelectable(rtf.BPID)).Result,
                    Ct = rtf.Ct,
                    UnitPrice = rtf.UnitPrice,
                    Price = rtf.Price
                })
                .ToList();
        }

        private List<Resver_GetAllInfoById_Output_ContactItem_APIItem> GetAllInfoByIdPopulateContactItems(
            Resver_Head entity)
        {
            return DC.M_Contect.Where(c =>
                    c.TargetTable == DC.GetTableName<Resver_Head>()
                    && c.TargetID == entity.RHID)
                .OrderBy(c => c.SortNo)
                .Select(c => new Resver_GetAllInfoById_Output_ContactItem_APIItem
                {
                    MID = c.MID,
                    ContactType = c.ContectType,
                    ContactTypeList = ContactTypeController.GetContactTypeSelectable(c.ContectType),
                    ContactData = c.ContectData ?? ""
                })
                .ToList();
        }

        private List<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem> GetTimeSpanFromHead<T>(Resver_Head head,
            int entityId)
            where T : class
        {
            return head.M_Resver_TimeSpan
                .Where(rts => rts.TargetTable == DC.GetTableName<T>()
                              && rts.TargetID == entityId)
                .OrderBy(rts => rts.SortNo)
                .Select(rts => new Resver_GetAllInfoById_Output_TimeSpanItem_APIItem
                {
                    DTSID = rts.DTSID,
                    Title = rts.DTS?.Title ?? "",
                    TimeS = (rts.DTS?.HourS ?? 0, rts.DTS?.MinuteS ?? 0).ToFormattedHourAndMinute(),
                    TimeE = (rts.DTS?.HourE ?? 0, rts.DTS?.MinuteE ?? 0).ToFormattedHourAndMinute(),
                    Minutes = (rts.DTS?.HourS ?? 0, rts.DTS?.MinuteS ?? 0).GetMinutesUntil((rts.DTS?.HourE ?? 0,
                        rts.DTS?.MinuteE ?? 0))
                })
                .ToList();
        }

        #endregion

        #region DeleteItem
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<Resver_Head> DeleteItemQuery(int id)
        {
            return DC.Resver_Head.Where(rh => rh.RHID == id);
        }
        #endregion
    }
}