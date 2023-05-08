using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Resver.GetAllInfoById;
using NS_Education.Models.APIItems.Resver.GetHeadList;
using NS_Education.Models.APIItems.Resver.Submit;
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
        IDeleteItem<Resver_Head>,
        ISubmit<Resver_Head, Resver_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Resver_GetHeadList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<Resver_Submit_Input_APIItem> _submitHelper;

        public ResverController()
        {
            _getListPagedHelper = new GetListPagedHelper<ResverController, Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<ResverController, Resver_Head, Resver_GetAllInfoById_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<ResverController, Resver_Head>(this);
            _submitHelper = new SubmitHelper<ResverController, Resver_Head, Resver_Submit_Input_APIItem>(this);
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
                    FoodItems = rt.BSC?.Title != DbConstants.ThrowDineTitle
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
        
        #region ChangeCheck

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeCheck(int? id, bool? checkFlag)
        {
            // 1. 驗證輸入
            if (!ChangeCheckValidateInput(id, checkFlag))
                return GetResponseJson();

            // 2. 查詢 DB
            Resver_Head entity = await ChangeCheckQueryFromDb(id);

            if (entity is null)
            {
                AddError(NotFound());
                return GetResponseJson();
            }
            
            // 3. 修改 DB
            entity.CheckFlag = checkFlag ?? throw new ArgumentNullException(nameof(checkFlag));
            await ChangeCheckUpdateDb();
            
            // 4. 回傳
            return GetResponseJson();
        }

        private async Task ChangeCheckUpdateDb()
        {
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private async Task<Resver_Head> ChangeCheckQueryFromDb(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            
            return await DC.Resver_Head
                .Where(rh => !rh.DeleteFlag && rh.RHID == id)
                .FirstOrDefaultAsync();
        }

        private bool ChangeCheckValidateInput(int? id, bool? checkFlag)
        {
            bool isValid = this.StartValidate()
                .Validate(_ => id > 0, () => AddError(EmptyNotAllowed("欲更新的預約 ID")))
                .Validate(_ => checkFlag != null, () => AddError(EmptyNotAllowed("確認狀態")))
                .IsValid();

            return isValid;
        }

        #endregion
        
        #region ChangeCheckIn
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeCheckIn(int? id, bool? checkInFlag)
        {
            // 1. 驗證輸入
            if (!ChangeCheckInValidateInput(id, checkInFlag))
                return GetResponseJson();

            // 2. 查詢 DB
            Resver_Head entity = await ChangeCheckInQueryFromDb(id);

            if (entity is null)
            {
                AddError(NotFound());
                return GetResponseJson();
            }
            
            // 3. 修改 DB
            entity.CheckInFlag = checkInFlag ?? throw new ArgumentNullException(nameof(checkInFlag));
            await ChangeCheckInUpdateDb();
            
            // 4. 回傳
            return GetResponseJson();
        }

        private async Task ChangeCheckInUpdateDb()
        {
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private async Task<Resver_Head> ChangeCheckInQueryFromDb(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            
            return await DC.Resver_Head
                .Where(rh => !rh.DeleteFlag && rh.RHID == id)
                .FirstOrDefaultAsync();
        }

        private bool ChangeCheckInValidateInput(int? id, bool? checkInFlag)
        {
            bool isValid = this.StartValidate()
                .Validate(_ => id > 0, () => AddError(EmptyNotAllowed("欲更新的預約 ID")))
                .Validate(_ => checkInFlag != null, () => AddError(EmptyNotAllowed("報到狀態")))
                .IsValid();

            return isValid;
        }
        
        #endregion

        #region Submit
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Resver_Submit_Input_APIItem.RHID))]
        public async Task<string> Submit(Resver_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Resver_Submit_Input_APIItem input)
        {
            return input.RHID == 0;
        }
        
        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Resver_Submit_Input_APIItem input)
        {
            return await SubmitValidateInput(input);
        }

        public async Task<Resver_Head> SubmitCreateData(Resver_Submit_Input_APIItem input)
        {
            return await _SubmitCreateData(input);
        }

        private async Task<bool> SubmitValidateInput(Resver_Submit_Input_APIItem input)
        {
            bool isAdd = SubmitIsAdd(input);
            
            DateTime headStartDate = default;
            DateTime headEndDate = default;

            // 主預約單
            bool isHeadValid = await input.StartValidate()
                .Validate(i => isAdd ? i.RHID == 0 : i.RHID.IsZeroOrAbove(), () => AddError(WrongFormat("預約單 ID")))
                .ValidateAsync(async i => await SubmitValidateStaticCode(i.BSCID12, StaticCodeType.ResverStatus),
                    () => AddError(NotFound("預約狀態 ID")))
                .ValidateAsync(async i => await SubmitValidateStaticCode(i.BSCID11, StaticCodeType.ResverSource),
                    () => AddError(NotFound("預約來源 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("預約單編號")))
                .Validate(i => i.SDate.TryParseDateTime(out headStartDate), () => AddError(WrongFormat("預約單起始日")))
                .Validate(i => i.EDate.TryParseDateTime(out headEndDate), () => AddError(WrongFormat("預約單結束日")))
                .Validate(i => headEndDate.Date >= headStartDate.Date,
                    () => AddError(MinLargerThanMax("預約單起始日", "預約單結束日")))
                .ValidateAsync(async i => await SubmitValidateCustomerId(i.CID), () => AddError(NotFound("客戶")))
                .Validate(i => i.CustomerTitle.HasContent(), () => AddError(EmptyNotAllowed("客戶名稱")))
                .Validate(i => i.ContactName.HasContent(), () => AddError(EmptyNotAllowed("聯絡人名稱")))
                .ValidateAsync(async i => await SubmitValidateMKBusinessUser(i.MK_BUID), () => AddError(NotFound("MK 業務")))
                .ValidateAsync(async i => await SubmitValidateOPBusinessUser(i.OP_BUID), () => AddError(NotFound("OP 業務")))
                .IsValid();

            // 主預約單 -> 聯絡方式
            bool isContactItemValid = input.ContactItems.All(item =>
                item.StartValidate()
                    .Validate(ci => isAdd ? ci.MID == 0 : ci.MID.IsZeroOrAbove(), () => AddError(WrongFormat("聯絡方式對應 ID")))
                    .Validate(ci => SubmitValidateContactType(ci.ContactType), () => AddError(NotFound("聯絡方式編號")))
                    .Validate(ci => ci.ContactData.HasContent(), () => AddError(EmptyNotAllowed("聯絡方式內容")))
                    .IsValid());

            // 主預約單 -> 場地列表
            bool isSiteItemsValid = input.SiteItems.All(item =>
                item.StartValidate()
                    .Validate(si => isAdd ? si.RSID == 0 : si.RSID.IsZeroOrAbove(), () => AddError(WrongFormat("場地預約單 ID")))
                    .Validate(si => si.TargetDate.TryParseDateTime(out _), () => AddError(WrongFormat("場地使用日期")))
                    .Validate(si => Task.Run(() => SubmitValidateSiteData(si.BSID)).Result, () => AddError(NotFound("場地 ID")))
                    .Validate(si => Task.Run(() => SubmitValidateOrderCode(si.BOCID)).Result, () => AddError(NotFound("預約場地的入帳代號 ID")))
                    .Validate(si => Task.Run(() => SubmitValidateStaticCode(si.BSCID, StaticCodeType.SiteTable)).Result, () => AddError(NotFound("預約場地的桌型 ID")))
                    .IsValid());

            // 主預約單 -> 場地列表 -> 時段列表
            bool isSiteItemTimeSpanItemValid =
                SubmitValidateTimeSpanItems(input.SiteItems.SelectMany(si => si.TimeSpanItems));

            // 主預約單 -> 場地列表 -> 行程列表
            bool isSiteItemThrowItemValid = input.SiteItems
                .SelectMany(si => si.ThrowItems)
                .All(item =>
                    item.StartValidate()
                        .Validate(ti => isAdd ? ti.RTID == 0 : ti.RTID.IsZeroOrAbove(), () => AddError(WrongFormat("行程預約單 ID")))
                        .Validate(ti => ti.TargetDate.TryParseDateTime(out _),
                            () => AddError(WrongFormat("預約行程的預計使用日期")))
                        .Validate(ti => Task.Run(() => SubmitValidateStaticCode(ti.BSCID, StaticCodeType.ResverThrow)).Result,
                            () => AddError(WrongFormat("預約類型")))
                        .Validate(ti => ti.Title.HasContent(), () => AddError(EmptyNotAllowed("行程名稱")))
                        .Validate(ti => Task.Run(() => SubmitValidateOrderCode(ti.BOCID)).Result, () => AddError("預約行程的入帳代號 ID"))
                        .IsValid());

            // 主預約單 -> 場地列表 -> 行程列表 -> 時段列表
            bool isSiteItemThrowItemTimeSpanItemValid =
                SubmitValidateTimeSpanItems(input.SiteItems.SelectMany(si => si.ThrowItems)
                    .SelectMany(ti => ti.TimeSpanItems));

            // 主預約單 -> 場地列表 -> 行程列表 -> 餐飲補充列表
            bool isSiteItemThrowItemFoodItemValid =
                input.SiteItems
                    .SelectMany(si => si.ThrowItems).SelectMany(ti => ti.FoodItems)
                    .All(item =>
                        item.StartValidate()
                            .Validate(fi => isAdd ? fi.RTFID == 0 : fi.RTFID.IsZeroOrAbove(), () => AddError(WrongFormat("行程餐飲預約單 ID")))
                            .Validate(fi => SubmitValidateFoodCategory(fi.DFCID),
                                () => AddError(NotFound("預約行程的餐種 ID")))
                            .Validate(fi => Task.Run(() => SubmitValidateStaticCode(fi.BSCID, StaticCodeType.Cuisine)).Result,
                                () => AddError(NotFound("預約行程的餐別 ID")))
                            .Validate(fi => SubmitValidatePartner(fi.BPID), () => AddError(NotFound("預約行程的廠商 ID")))
                            .IsValid());

            // 主預約單 -> 場地列表 -> 設備列表
            bool isSiteItemDeviceItemValid =
                input.SiteItems.SelectMany(si => si.DeviceItems)
                    .All(item =>
                        item.StartValidate()
                            .Validate(di => isAdd ? di.RDID == 0 : di.RDID.IsZeroOrAbove(), () => AddError(WrongFormat("設備預約單 ID")))
                            .Validate(di => di.TargetDate.TryParseDateTime(out _),
                                () => AddError(WrongFormat("預約設備的預計使用日期")))
                            .Validate(di => SubmitValidateDevice(di.BDID), () => AddError(NotFound("預約設備 ID")))
                            .Validate(di => Task.Run(() => SubmitValidateOrderCode(di.BOCID)).Result, () => AddError(NotFound("預約設備的入帳代號 ID")))
                            .IsValid()
                    );

            // 主預約單 -> 場地列表 -> 設備列表 -> 時段列表
            bool isSiteItemDeviceItemTimeSpanItemValid = SubmitValidateTimeSpanItems(input.SiteItems
                .SelectMany(si => si.DeviceItems)
                .SelectMany(di => di.TimeSpanItems));

            // 主預約單 -> 其他收費項目列表
            bool isOtherItemValid =
                input.OtherItems.All(item => item.StartValidate()
                    .Validate(oi => isAdd ? oi.ROID == 0 : oi.ROID.IsZeroOrAbove(), () => AddError(WrongFormat("其他收費項目預約單 ID")))
                    .Validate(oi => oi.TargetDate.TryParseDateTime(out _), () => AddError(WrongFormat("其他收費項目的預計使用日期")))
                    .Validate(oi => SubmitValidateOtherPayItem(oi.DOPIID), () => AddError(NotFound("其他收費項目 ID")))
                    .Validate(oi => Task.Run(() => SubmitValidateOrderCode(oi.BOCID)).Result, () => AddError(NotFound("其他收費項目的入帳代號 ID")))
                    .IsValid());
            
            // 主預約單 -> 繳費紀錄列表
            bool isBillItemValid =
                input.BillItems.All(item => item.StartValidate()
                    .Validate(bi => isAdd ? bi.RBID == 0 : bi.RBID.IsZeroOrAbove(), () => AddError(WrongFormat("繳費紀錄預約單 ID")))
                    .Validate(bi => SubmitValidateCategory(bi.BCID), () => AddError(NotFound("繳費類別 ID")))
                    .Validate(bi => SubmitValidatePayType(bi.DPTID), () => AddError(NotFound("繳費紀錄的付款方式 ID")))
                    .Validate(bi => bi.PayDate.TryParseDateTime(out _, DateTimeParseType.DateTime), () => AddError("付款時間"))
                    .IsValid());
            
            // 主預約單 -> 預約回饋紀錄列表
            bool isGiveBackItemValid =
                input.GiveBackItems.All(item => item.StartValidate()
                    .Validate(gbi => isAdd ? gbi.RGBID == 0 : gbi.RGBID.IsZeroOrAbove(), () => AddError(WrongFormat("預約回饋預約單 ID")))
                    .Validate(gbi => gbi.PointDecimal.IsInBetween(0, 50), () => AddError(OutOfRange("回饋分數", 0, 50)))
                    .IsValid());
            
            return await Task.FromResult(isHeadValid 
                                         && isContactItemValid 
                                         && isSiteItemsValid 
                                         && isSiteItemTimeSpanItemValid 
                                         && isSiteItemThrowItemValid 
                                         && isSiteItemThrowItemTimeSpanItemValid
                                         && isSiteItemThrowItemFoodItemValid
                                         && isSiteItemDeviceItemValid
                                         && isSiteItemDeviceItemTimeSpanItemValid
                                         && isOtherItemValid
                                         && isBillItemValid
                                         && isGiveBackItemValid);
        }

        private bool SubmitValidatePayType(int payTypeId)
        {
            return payTypeId.IsAboveZero() && DC.D_PayType.Any(pt => pt.ActiveFlag && !pt.DeleteFlag && pt.DPTID == payTypeId);
        }

        private bool SubmitValidateCategory(int categoryId)
        {
            return Task.Run(() => DC.B_Category.ValidateCategoryExists(categoryId)).Result;
        }

        private bool SubmitValidateOtherPayItem(int otherPayItemId)
        {
            return otherPayItemId.IsAboveZero() && DC.D_OtherPayItem.Any(opi => opi.ActiveFlag && !opi.DeleteFlag && opi.DOPIID == otherPayItemId);
        }

        private bool SubmitValidateDevice(int deviceId)
        {
            return deviceId.IsAboveZero() && DC.B_Device.Any(bd => bd.ActiveFlag && !bd.DeleteFlag && bd.BDID == deviceId);
        }

        private bool SubmitValidatePartner(int partnerId)
        {
            return partnerId.IsAboveZero() && DC.B_Partner.Any(p => p.ActiveFlag && !p.DeleteFlag && p.BPID == partnerId);
        }

        private bool SubmitValidateFoodCategory(int foodCategoryId)
        {
            return foodCategoryId.IsAboveZero() && DC.D_FoodCategory.Any(dfc =>
                dfc.ActiveFlag && !dfc.DeleteFlag && dfc.DFCID == foodCategoryId);
        }

        private bool SubmitValidateTimeSpanItems(IEnumerable<Resver_Submit_TimeSpanItem_Input_APIItem> items)
        {
            return items.All(item =>
                item.StartValidate()
                    .Validate(tsi => Task.Run(() => SubmitValidateDataTimeSpan(tsi.DTSID)).Result)
                    .IsValid());
        }

        private async Task<bool> SubmitValidateDataTimeSpan(int dtsId)
        {
            return dtsId.IsAboveZero() && await DC.D_TimeSpan.AnyAsync(dts => dts.ActiveFlag && !dts.DeleteFlag && dts.DTSID == dtsId);
        }

        private async Task<bool> SubmitValidateOrderCode(int orderCodeId)
        {
            return orderCodeId.IsAboveZero() && await DC.B_OrderCode.AnyAsync(boc => boc.ActiveFlag && !boc.DeleteFlag && boc.BOCID == orderCodeId);
        }

        private async Task<bool> SubmitValidateSiteData(int siteDataId)
        {
            return siteDataId.IsAboveZero() && await DC.B_SiteData.AnyAsync(sd => sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == siteDataId);
        }

        private static bool SubmitValidateContactType(int contactType)
        {
            return ContactTypeController.GetContactTypeList().Any(ct => ct.ID == contactType);
        }

        private async Task<bool> SubmitValidateOPBusinessUser(int businessUserId)
        {
            return businessUserId.IsAboveZero() && await DC.BusinessUser.AnyAsync(bu => bu.ActiveFlag && !bu.DeleteFlag && bu.OPsalesFlag && bu.BUID == businessUserId);
        }

        private async Task<bool> SubmitValidateMKBusinessUser(int businessUserId)
        {
            return businessUserId.IsAboveZero() && await DC.BusinessUser.AnyAsync(bu => bu.ActiveFlag && !bu.DeleteFlag && bu.MKsalesFlag && bu.BUID == businessUserId);
        }

        private async Task<bool> SubmitValidateCustomerId(int customerId)
        {
            return customerId.IsAboveZero() && await DC.Customer.AnyAsync(c => c.ActiveFlag && !c.DeleteFlag && c.CID == customerId);
        }

        private async Task<bool> SubmitValidateStaticCode(int staticCodeId, StaticCodeType codeType)
        {
            return await DC.B_StaticCode.ValidateStaticCodeExists(staticCodeId, codeType);
        }

        private async Task<Resver_Head> _SubmitCreateData(Resver_Submit_Input_APIItem input, Resver_Head data = null)
        {
            // 基本參數初始化
            bool isAdd = SubmitIsAdd(input);
            IList<object> entitiesToAdd = new List<object>();

            // 取得主資料
            Resver_Head head = data ?? SubmitFindOrCreateNew<Resver_Head>(input.RHID);
            // 先寫入 DB, 這樣才有 RHID 可以提供給後面的功能用
            await DC.AddAsync(head);
            await DC.SaveChangesStandardProcedureAsync(GetUid());
            // 清理所有跟這張預約單有關的 ResverTimeSpan
            head.M_Resver_TimeSpan.Clear();

            // 開始寫入值
            SubmitPopulateHeadValues(input, head);
            SubmitPopulateHeadContactItems(input, head, entitiesToAdd, isAdd);
            await SubmitPopulateHeadSiteItems(input, head);
            SubmitPopulateHeadOtherItems(input, head, entitiesToAdd);
            SubmitPopulateHeadBillItems(input, head, entitiesToAdd);
            SubmitPopulateHeadGiveBackItems(input, head, entitiesToAdd);
            
            // 寫入 Db
            await DC.AddRangeAsync(entitiesToAdd);
            return head;
        }

        private void SubmitPopulateHeadGiveBackItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            foreach (var item in input.GiveBackItems)
            {
                Resver_GiveBack giveBack = SubmitFindOrCreateNew<Resver_GiveBack>(item.RGBID, entitiesToAdd);
                giveBack.RHID = head.RHID;
                giveBack.Title = item.Title;
                giveBack.Description = item.Description;
                giveBack.PointDecimal = item.PointDecimal;
            }
        }

        private void SubmitPopulateHeadBillItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            foreach (var item in input.BillItems)
            {
                Resver_Bill bill = SubmitFindOrCreateNew<Resver_Bill>(item.RBID, entitiesToAdd);
                bill.RHID = head.RHID;
                bill.BCID = item.BCID;
                bill.DPTID = item.DPTID;
                bill.Price = item.Price;
                bill.Note = item.Note;
                bill.PayFlag = item.PayFlag;
                bill.PayDate = item.PayDate.ParseDateTime();
                bill.CheckUID = head.UpdUID;
            }
        }

        private void SubmitPopulateHeadOtherItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            foreach (var item in input.OtherItems)
            {
                Resver_Other other = SubmitFindOrCreateNew<Resver_Other>(item.ROID, entitiesToAdd);
                other.TargetDate = item.TargetDate.ParseDateTime().Date;
                other.RHID = head.RHID;
                other.DOPIID = item.DOPIID;
                other.BOCID = item.BOCID;
                other.PrintTitle = item.PrintTitle;
                other.PrintNote = item.PrintNote;
                other.UnitPrice = item.UnitPrice;
                other.FixedPrice = item.FixedPrice;
                other.Ct = item.Ct;
                other.QuotedPrice = item.QuotedPrice;
                other.SortNo = item.SortNo;
                other.Note = item.Note;
            }
        }

        private async Task SubmitPopulateHeadSiteItems(Resver_Submit_Input_APIItem input, Resver_Head head)
        {
            foreach (var item in input.SiteItems)
            {
                Resver_Site site = SubmitFindOrCreateNew<Resver_Site>(item.RSID);
                site.TargetDate = item.TargetDate.ParseDateTime().Date;
                site.RHID = head.RHID;
                site.BSID = item.BSID;
                site.BOCID = item.BOCID;
                site.PrintTitle = item.PrintTitle;
                site.PrintNote = item.PrintNote;
                site.UnitPrice = item.UnitPrice;
                site.FixedPrice = item.FixedPrice;
                site.QuotedPrice = item.QuotedPrice;
                site.SortNo = item.SortNo;
                site.Note = item.Note;
                site.BSCID = item.BSCID;

                // 先儲存至 DB, 才有 RSID...
                await DC.AddAsync(site);
                await DC.SaveChangesStandardProcedureAsync(GetUid());

                SubmitPopulateSiteItemTimeSpanItems(item, head, site);
                await SubmitPopulateSiteItemThrowItems(item, head, site);
                await SubmitPopulateSiteItemDeviceItems(item, head, site);
            }
        }

        private async Task SubmitPopulateSiteItemDeviceItems(Resver_Submit_SiteItem_Input_APIItem item,
            Resver_Head head, Resver_Site site)
        {
            site.Resver_Device.Clear();
            foreach (var deviceItem in item.DeviceItems)
            {
                Resver_Device device = SubmitFindOrCreateNew<Resver_Device>(deviceItem.RDID);
                device.TargetDate = deviceItem.TargetDate.ParseDateTime().Date;
                device.RSID = site.RSID;
                device.BDID = deviceItem.BDID;
                device.BOCID = deviceItem.BOCID;
                device.PrintTitle = deviceItem.PrintTitle;
                device.PrintNote = deviceItem.PrintNote;
                device.UnitPrice = deviceItem.UnitPrice;
                device.FixedPrice = deviceItem.FixedPrice;
                device.QuotedPrice = deviceItem.QuotedPrice;
                device.SortNo = deviceItem.SortNo;
                device.Note = deviceItem.Note;

                site.Resver_Device.Add(device);
                // 先儲存至 DB 才有 RDID...
                await DC.Resver_Device.AddAsync(device);
                await DC.SaveChangesStandardProcedureAsync(GetUid());

                // 寫入 device 的 TimeSpan
                int sortNo = 0;
                foreach (var timeSpanItem in deviceItem.TimeSpanItems)
                {
                    M_Resver_TimeSpan ts = new M_Resver_TimeSpan
                    {
                        RHID = head.RHID,
                        TargetTable = DC.GetTableName<Resver_Device>(),
                        TargetID = device.RDID,
                        DTSID = timeSpanItem.DTSID,
                        SortNo = ++sortNo
                    };

                    head.M_Resver_TimeSpan.Add(ts);
                }
            }
        }

        private async Task SubmitPopulateSiteItemThrowItems(Resver_Submit_SiteItem_Input_APIItem item,
            Resver_Head head, Resver_Site site)
        {
            site.Resver_Throw.Clear();
            foreach (var throwItem in item.ThrowItems)
            {
                Resver_Throw throwData = SubmitFindOrCreateNew<Resver_Throw>(throwItem.RTID);
                throwData.TargetDate = throwItem.TargetDate.ParseDateTime().Date;
                throwData.RSID = site.RSID;
                throwData.BSCID = throwItem.BSCID;
                throwData.Title = throwItem.Title;
                throwData.BOCID = throwItem.BOCID;
                throwData.PrintTitle = throwItem.PrintTitle;
                throwData.PrintNote = throwItem.PrintNote;
                throwData.UnitPrice = throwItem.UnitPrice;
                throwData.FixedPrice = throwItem.FixedPrice;
                throwData.QuotedPrice = throwItem.QuotedPrice;
                throwData.SortNo = throwItem.SortNo;
                throwData.Note = throwItem.Note;
                
                site.Resver_Throw.Add(throwData);

                // 先儲存才有 RTID 給 Resver_TimeSpan 用...
                await DC.AddAsync(throwData);
                await DC.SaveChangesStandardProcedureAsync(GetUid());

                SubmitPopulateSiteItemThrowItemTimeSpanItems(throwData, throwItem, head);
                SubmitPopulateSiteItemThrowItemThrowFoodItems(throwData, throwItem);
            }
        }

        private static void SubmitPopulateSiteItemThrowItemThrowFoodItems(Resver_Throw throwData,
            Resver_Submit_ThrowItem_Input_APIItem throwItem)
        {
            throwData.Resver_Throw_Food = throwItem.FoodItems.Select(fi => new Resver_Throw_Food
            {
                RTID = throwData.RTID,
                DFCID = fi.DFCID,
                BSCID = fi.BSCID,
                BPID = fi.BPID,
                Ct = fi.Ct,
                UnitPrice = fi.UnitPrice,
                Price = fi.Price
            }).ToList();
        }

        private void SubmitPopulateSiteItemThrowItemTimeSpanItems(Resver_Throw throwData,
            Resver_Submit_ThrowItem_Input_APIItem throwItem,
            Resver_Head head)
        {
            int sortNo = 0;
            foreach (var timeSpanItem in throwItem.TimeSpanItems)
            {
                M_Resver_TimeSpan resverTimeSpan = new M_Resver_TimeSpan
                {
                    RHID = head.RHID,
                    TargetTable = DC.GetTableName<Resver_Throw>(),
                    TargetID = throwData.RTID,
                    DTSID = timeSpanItem.DTSID,
                    SortNo = ++sortNo
                };
                head.M_Resver_TimeSpan.Add(resverTimeSpan);
            }
        }

        private void SubmitPopulateSiteItemTimeSpanItems(Resver_Submit_SiteItem_Input_APIItem item, Resver_Head head,
            Resver_Site site)
        {
            int sortNo = 0;
            foreach (var timeSpanItem in item.TimeSpanItems)
            {
                M_Resver_TimeSpan resverTimeSpan = new M_Resver_TimeSpan
                {
                    RHID = head.RHID,
                    TargetTable = DC.GetTableName<Resver_Site>(),
                    TargetID = site.RSID,
                    DTSID = timeSpanItem.DTSID,
                    SortNo = ++sortNo
                };

                head.M_Resver_TimeSpan.Add(resverTimeSpan);
            }
        }

        private void SubmitPopulateHeadContactItems(Resver_Submit_Input_APIItem input, Resver_Head head, ICollection<object> entitiesToAdd, bool isAdd)
        {
            if (!isAdd)
            {
                // 先清除所有原本有的 M_Contect
                var originalContacts = DC.M_Contect
                    .Where(c => c.TargetTable == DC.GetTableName<Resver_Head>() && c.TargetID == input.RHID)
                    .AsEnumerable();
                
                DC.RemoveRange(originalContacts);
            }

            int sortNo = 0;
            foreach (var contactItem in input.ContactItems)
            {
                M_Contect contact = new M_Contect
                {
                    ContectType = contactItem.ContactType,
                    TargetTable = DC.GetTableName<Resver_Head>(),
                    TargetID = head.RHID,
                    ContectData = contactItem.ContactData,
                    SortNo = ++sortNo
                };
                entitiesToAdd.Add(contact);
            }
        }

        private static void SubmitPopulateHeadValues(Resver_Submit_Input_APIItem input, Resver_Head head)
        {
            head.BSCID12 = input.BSCID12;
            head.BSCID11 = input.BSCID11;
            head.Code = input.Code;
            head.Title = input.Title;
            head.SDate = input.SDate.ParseDateTime().Date;
            head.EDate = input.EDate.ParseDateTime().Date;
            head.PeopleCt = input.PeopleCt;
            head.CID = input.CID;
            head.CustomerTitle = input.CustomerTitle;
            head.ContactName = input.ContactName;
            head.MK_BUID = input.MK_BUID;
            head.MK_Phone = input.MK_Phone;
            head.OP_BUID = input.OP_BUID;
            head.OP_Phone = input.OP_Phone;
            head.Note = input.Note;
            head.FixedPrice = input.FixedPrice;
            head.QuotedPrice = input.QuotedPrice;
        }

        private T SubmitFindOrCreateNew<T>(int id, ICollection<object> entitiesToAdd = null)
            where T : class
        {
            T t = null;
            if (id != 0)
                t = DC.Find<T>(id);
            if (t != null
                && (t.GetIfHasProperty<T, bool?>(DbConstants.ActiveFlag) ?? true)
                && (t.GetIfHasProperty<T, bool?>(DbConstants.DeleteFlag) ?? false) == false)
            {
                // 讀取所有 FK property
                foreach (var navigationEntry in DC.Entry(t).Navigations)
                {
                    navigationEntry.Load();
                }

                return t;
            }

            t = Activator.CreateInstance<T>();
            entitiesToAdd?.Add(t);
            return t;
        }

        #endregion
        
        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Resver_Submit_Input_APIItem input)
        {
            return await SubmitValidateInput(input);
        }

        public IQueryable<Resver_Head> SubmitEditQuery(Resver_Submit_Input_APIItem input)
        {
            return DC.Resver_Head.Where(rh => rh.RHID == input.RHID)
                // site
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Throw)
                .ThenInclude(rt => rt.Resver_Throw_Food)
                .Include(rh => rh.Resver_Site)
                .ThenInclude(rs => rs.Resver_Device)
                // other
                .Include(rh => rh.Resver_Other)
                // bill
                .Include(rh => rh.Resver_Bill)
                // giveback
                .Include(rh => rh.Resver_GiveBack);
        }

        public void SubmitEditUpdateDataFields(Resver_Head data, Resver_Submit_Input_APIItem input)
        {
            Task.Run(() => _SubmitCreateData(input, data)).GetAwaiter().GetResult();
        }
        
        #endregion
        
        #endregion
    }
}