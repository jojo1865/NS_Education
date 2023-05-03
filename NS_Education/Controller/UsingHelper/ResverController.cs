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

namespace NS_Education.Controller.UsingHelper
{
    public class ResverController : PublicClass, 
        IGetListPaged<Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>,
        IGetInfoById<Resver_Head, Resver_GetAllInfoById_Output_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Resver_GetHeadList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public ResverController()
        {
            _getListPagedHelper = new GetListPagedHelper<ResverController, Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<ResverController, Resver_Head, Resver_GetAllInfoById_Output_APIItem>(this);
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
                .Where(rh => rh.RHID == id);
        }

        public async Task<Resver_GetAllInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(Resver_Head entity)
        {
            var result = new Resver_GetAllInfoById_Output_APIItem
            {
                RHID = entity.RHID,
                BSCID12 = entity.BSCID12,
                BSC12_Title = entity.BSCID12Navigation?.Title ?? "",
                BSC12_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSCID12Navigation?.CodeType, entity.BSCID12),
                BSCID11 = entity.BSCID11,
                BSC11_Title = entity.BSCID11Navigation?.Title ?? "",
                BSC11_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSCID11Navigation?.CodeType, entity.BSCID11),
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
                ContactItems = DC.M_Contect.Where(c => 
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
                    .ToList(),
                SiteItems = entity.Resver_Site
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
                        BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(rs.BSC?.CodeType, rs.BSCID)).Result,
                        TimeSpanItems = GetTimeSpanFromResverSite(rs),
                        ThrowItems = rs.Resver_Throw
                            .Select(rt => new Resver_GetAllInfoById_Output_ThrowItem_APIItem
                        {
                            RTID = rt.RTID,
                            TargetDate = rt.TargetDate.ToFormattedStringDate(),
                            BSCID = rt.BSCID,
                            BSC_Title = rt.BSC?.Title ?? "",
                            BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(rt.BSC?.CodeType, rt.BSCID)).Result,
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
                            TimeSpanItems = GetTimeSpanFromResverSite(rt.RS),
                            FoodItems = rt.BSC?.Title != DbConstants.ReseverThrowDineTitle
                                ? new List<Resver_GetAllInfoById_Output_FoodItem_APIItem>()
                                : rt.Resver_Throw_Food
                                    .Select(rtf => new Resver_GetAllInfoById_Output_FoodItem_APIItem
                                    {
                                        RTFID = rtf.RTFID,
                                        DFCID = rtf.DFCID,
                                        DFC_Title = rtf.DFC?.Title,
                                        DFC_List = Task.Run(() => DC.D_FoodCategory.GetFoodCategorySelectable(rtf.DFCID)).Result,
                                        BSCID = rtf.BSCID,
                                        BSC_Title = rtf.BSC?.Title,
                                        BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(rtf.BSC?.CodeType, rtf.BSCID)).Result,
                                        BPID = rtf.BPID,
                                        BP_Title = rtf.BP?.Title,
                                        BP_List = Task.Run(() => DC.B_Partner.GetPartnerSelectable(rtf.BPID)).Result,
                                        Ct = rtf.Ct,
                                        UnitPrice = rtf.UnitPrice,
                                        Price = rtf.Price
                                    })
                                    .ToList()
                        })
                            .OrderBy(rt => rt.SortNo)
                            .ToList(),
                        DeviceItems = null
                    })
                    .OrderBy(rs => rs.SortNo)
                    .ToList(),
                OtherItems = null,
                BillItems = null,
                GiveBackItems = null
            };

            return await Task.FromResult(result);
        }

        private List<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem> GetTimeSpanFromResverSite(Resver_Site rs)
        {
            return rs.RH.M_Resver_TimeSpan
                .Where(rts => rts.TargetTable == DC.GetTableName<Resver_Head>()
                              && rts.RHID == rs.RHID
                              && rts.TargetID == rs.RSID)
                .OrderBy(rts => rts.SortNo)
                .Select(rts => new Resver_GetAllInfoById_Output_TimeSpanItem_APIItem
                {
                    DTSID = rts.DTSID,
                    Title = rts.DTS?.Title ?? "",
                    TimeS = (rts.DTS?.HourS ?? 0, rts.DTS?.MinuteS ?? 0).ToFormattedHourAndMinute(),
                    TimeE = (rts.DTS?.HourE ?? 0, rts.DTS?.MinuteE ?? 0).ToFormattedHourAndMinute(),
                    Minutes = (rts.DTS?.HourS ?? 0, rts.DTS?.MinuteS ?? 0).GetMinutesUntil((rts.DTS?.HourE ?? 0, rts.DTS?.MinuteE ?? 0))
                })
                .ToList();
        }

        #endregion
    }
}