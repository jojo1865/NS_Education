using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById;
using NS_Education.Models.APIItems.Controller.Resver.GetHeadList;
using NS_Education.Models.APIItems.Controller.Resver.Submit;
using NS_Education.Models.Entities;
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
            _getListPagedHelper =
                new GetListPagedHelper<ResverController, Resver_Head, Resver_GetHeadList_Input_APIItem,
                    Resver_GetHeadList_Output_Row_APIItem>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<ResverController, Resver_Head, Resver_GetAllInfoById_Output_APIItem>(this);
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
                .Include(rh => rh.B_StaticCode1)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(rh => (rh.Code != null && rh.Code.Contains(input.Keyword))
                                          || (rh.Title != null && rh.Title.Contains(input.Keyword)));

            if (input.TargetDate.TryParseDateTime(out DateTime targetDate))
                query = query.Where(rh => DbFunctions.TruncateTime(rh.SDate) == targetDate.Date);

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
                BSCID12_Title = entity.B_StaticCode.Title ?? ""
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
                .Include(rh => rh.B_StaticCode)
                .Include(rh => rh.B_StaticCode1)
                .Include(rh => rh.BusinessUser)
                .Include(rh => rh.BusinessUser1)
                // site
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_OrderCode))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_StaticCode))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Head))
                // resver_timespan
                .Include(rh => rh.M_Resver_TimeSpan)
                .Include(rh => rh.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                // site -> throw
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.B_StaticCode)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.B_OrderCode)))
                // site -> throw -> throw_food
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.D_FoodCategory))))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_StaticCode))))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_Partner))))
                // site -> device
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_OrderCode)))
                // otherItem
                .Include(rh => rh.Resver_Other)
                .Include(rh => rh.Resver_Other.Select(ro => ro.D_OtherPayItem))
                .Include(rh => rh.Resver_Other.Select(ro => ro.D_OtherPayItem.B_StaticCode))
                .Include(rh => rh.Resver_Other.Select(ro => ro.D_OtherPayItem.B_OrderCode))
                // bill
                .Include(rs => rs.Resver_Bill)
                .Include(rs => rs.Resver_Bill.Select(rb => rb.B_Category))
                .Include(rs => rs.Resver_Bill.Select(rb => rb.D_PayType))
                // GiveBack
                .Include(rb => rb.Resver_GiveBack)
                .Include(rb => rb.Resver_GiveBack.Select(rg => rg.B_StaticCode))
                .Where(rh => rh.RHID == id);
        }

        public async Task<Resver_GetAllInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(Resver_Head entity)
        {
            var result = new Resver_GetAllInfoById_Output_APIItem
            {
                RHID = entity.RHID,
                BSCID12 = entity.BSCID12,
                BSC12_Title = entity.B_StaticCode.Title ?? "",
                BSC12_List =
                    await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode?.CodeType, entity.BSCID12),
                BSCID11 = entity.BSCID11,
                BSC11_Title = entity.B_StaticCode1?.Title ?? "",
                BSC11_List =
                    await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode1?.CodeType, entity.BSCID11),
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
                MK_BU_Name = entity.BusinessUser.Name ?? "",
                MK_BU_List = await DC.BusinessUser.GetBusinessUserSelectable(entity.MK_BUID),
                MK_Phone = entity.MK_Phone ?? "",
                OP_BUID = entity.OP_BUID,
                OP_BU_Name = entity.BusinessUser1?.Name ?? "",
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
                BSCID16 = gb.BSCID16,
                BSCID16_Title = gb.B_StaticCode?.Title ?? ""
            }).ToList();
        }

        private List<Resver_GetAllInfoById_Output_BillItem_APIItem> GetAllInfoByIdPopulateBillItems(Resver_Head entity)
        {
            return entity.Resver_Bill.Select(rb => new Resver_GetAllInfoById_Output_BillItem_APIItem
            {
                RBID = rb.RBID,
                BCID = rb.BCID,
                BC_Title = rb.B_Category?.TitleC ?? rb.B_Category?.TitleE ?? "",
                BC_List = Task.Run(() => DC.B_Category.GetCategorySelectable(rb.B_Category?.CategoryType, rb.BCID))
                    .Result,
                DPTID = rb.DPTID,
                DPT_Title = rb.D_PayType?.Title ?? "",
                DPT_List = Task.Run(() => DC.D_PayType.GetOtherPayItemSelectable(rb.DPTID)).Result,
                Price = rb.Price,
                Note = rb.Note ?? "",
                PayFlag = rb.PayFlag,
                PayDate = rb.PayFlag ? rb.PayDate.ToFormattedStringDateTime() : ""
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
                DOPI_Title = ro.D_OtherPayItem?.Title ?? "",
                DOPI_List = Task.Run(() => DC.D_OtherPayItem.GetOtherPayItemSelectable(ro.DOPIID)).Result,
                BSCID = ro.BSCID,
                BSC_Title = ro.B_StaticCode?.Title ?? "",
                BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable((int)StaticCodeType.Unit, ro.BSCID))
                    .Result,
                BOCID = ro.BOCID,
                BOC_Code = ro.B_OrderCode?.Code ?? "",
                BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(ro.B_OrderCode?.CodeType, ro.BOCID))
                    .Result,
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
                    BS_Title = rs.B_SiteData?.Title ?? "",
                    BOCID = rs.BOCID,
                    BOC_Code = rs.B_OrderCode?.Code ?? "",
                    BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(rs.B_OrderCode?.CodeType, rs.BOCID))
                        .Result,
                    PrintTitle = rs.PrintTitle ?? "",
                    PrintNote = rs.PrintNote ?? "",
                    UnitPrice = rs.UnitPrice,
                    FixedPrice = rs.FixedPrice,
                    QuotedPrice = rs.QuotedPrice,
                    SortNo = rs.SortNo,
                    Note = rs.Note ?? "",
                    BSCID = rs.BSCID,
                    BSC_Title = rs.B_StaticCode?.Title ?? "",
                    BSC_List = Task.Run(() =>
                            DC.B_StaticCode.GetStaticCodeSelectable(rs.B_StaticCode?.CodeType, rs.BSCID))
                        .Result,
                    TimeSpanItems = GetTimeSpanFromHead<Resver_Site>(rs.Resver_Head, rs.RSID),
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
                BD_Title = rd.B_Device?.Title ?? "",
                BD_List = Task.Run(() => DC.B_Device.GetOtherPayItemSelectable(rd.BDID)).Result,
                Ct = rd.Ct,
                BOCID = rd.BOCID,
                BOC_Code = rd.B_OrderCode?.Code ?? "",
                BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(rd.B_OrderCode?.CodeType, rd.BOCID))
                    .Result,
                PrintTitle = rd.PrintTitle ?? "",
                PrintNote = rd.PrintNote ?? "",
                UnitPrice = rd.UnitPrice,
                FixedPrice = rd.FixedPrice,
                QuotedPrice = rd.QuotedPrice,
                SortNo = rd.SortNo,
                Note = rd.Note ?? "",
                TimeSpanItems = GetTimeSpanFromHead<Resver_Device>(rs.Resver_Head, rd.RDID)
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
                    BSC_Title = rt.B_StaticCode?.Title ?? "",
                    BSC_List = Task.Run(() =>
                            DC.B_StaticCode.GetStaticCodeSelectable(rt.B_StaticCode?.CodeType, rt.BSCID))
                        .Result,
                    Title = rt.Title ?? "",
                    BOCID = rt.BOCID,
                    BOC_Title = rt.B_OrderCode?.Title ?? "",
                    BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(rt.B_OrderCode?.CodeType, rt.BOCID))
                        .Result,
                    PrintTitle = rt.PrintTitle ?? "",
                    PrintNote = rt.PrintNote ?? "",
                    UnitPrice = rt.UnitPrice,
                    FixedPrice = rt.FixedPrice,
                    QuotedPrice = rt.QuotedPrice,
                    SortNo = rt.SortNo,
                    Note = rt.Note,
                    TimeSpanItems = GetTimeSpanFromHead<Resver_Throw>(rs.Resver_Head, rt.RTID),
                    FoodItems = rt.B_StaticCode?.Title != DbConstants.ThrowDineTitle
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
                    DFC_Title = rtf.D_FoodCategory?.Title,
                    DFC_List = Task.Run(() => DC.D_FoodCategory.GetFoodCategorySelectable(rtf.DFCID)).Result,
                    BSCID = rtf.BSCID,
                    BSC_Title = rtf.B_StaticCode?.Title,
                    BSC_List = Task.Run(() =>
                            DC.B_StaticCode.GetStaticCodeSelectable(rtf.B_StaticCode?.CodeType, rtf.BSCID))
                        .Result,
                    BPID = rtf.BPID,
                    BP_Title = rtf.B_Partner?.Title,
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
            string tableName = DC.GetTableName<Resver_Head>();
            return DC.M_Contect.Where(c =>
                    c.TargetTable == tableName
                    && c.TargetID == entity.RHID)
                .OrderBy(c => c.SortNo)
                // 回記憶體
                .AsEnumerable()
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
            string tableName = DC.GetTableName<T>();
            return head.M_Resver_TimeSpan
                .Where(rts => rts.TargetTable == tableName
                              && rts.TargetID == entityId)
                .OrderBy(rts => rts.SortNo)
                .Select(rts => new Resver_GetAllInfoById_Output_TimeSpanItem_APIItem
                {
                    DTSID = rts.DTSID,
                    Title = rts.D_TimeSpan?.Title ?? "",
                    TimeS = (rts.D_TimeSpan?.HourS ?? 0, rts.D_TimeSpan?.MinuteS ?? 0).ToFormattedHourAndMinute(),
                    TimeE = (rts.D_TimeSpan?.HourE ?? 0, rts.D_TimeSpan?.MinuteE ?? 0).ToFormattedHourAndMinute(),
                    Minutes = (rts.D_TimeSpan?.HourS ?? 0, rts.D_TimeSpan?.MinuteS ?? 0).GetMinutesUntil((
                        rts.D_TimeSpan?.HourE ?? 0,
                        rts.D_TimeSpan?.MinuteE ?? 0))
                })
                .ToList();
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<Resver_Head> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.Resver_Head.Where(rh => ids.Contains(rh.RHID));
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

            if (entity.B_StaticCode1.Code == ReserveHeadState.Terminated &&
                entity.B_StaticCode1.Code == ReserveHeadState.FullyPaid)
            {
                AddError("已結帳或已中止的預約單無法修改確認狀態！");
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
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
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
                .Include(rh => rh.B_StaticCode1)
                // 已結帳或已中止時，不允許修改確認狀態。
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

            if (entity.B_StaticCode1.Code == ReserveHeadState.Terminated)
            {
                AddError("已中止的預約單無法修改報到狀態！");
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
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
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
                .Include(rh => rh.B_StaticCode1)
                // 已中止時，不允許修改報到狀態。
                .Where(rh => !rh.DeleteFlag && rh.RHID == id)
                .FirstOrDefaultAsync();
        }

        private bool ChangeCheckInValidateInput(int? id, bool? checkInFlag)
        {
            bool isValid = this.StartValidate()
                .Validate(_ => id > 0, () => AddError(EmptyNotAllowed("欲更新的預約 ID")))
                .Validate(_ => checkInFlag != null, () => AddError(EmptyNotAllowed("報到狀態")))
                .Validate(_ => checkInFlag != false, () => AddError(UnsupportedValue("報到狀態")))
                .IsValid();

            return isValid;
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Resver_Submit_Input_APIItem.RHID))]
        public async Task<string> Submit(Resver_Submit_Input_APIItem input)
        {
            // 預約的商業邏輯較長，且需要驗證許多資料，而且可能有異步而造成多筆訂單同時完成，設備超訂等問題。
            // 所以，這裡指定 Serializable 做處理。
            return await _submitHelper.Submit(input, IsolationLevel.Serializable);
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
            bool dataCheckFlag = false;

            // 修改時，有一些值需要參照已有資料
            if (!isAdd)
            {
                // 先確認預約單狀態，如果是已中止，直接報錯
                Resver_Head head = await DC.Resver_Head
                    .Include(rh => rh.B_StaticCode1)
                    .FirstOrDefaultAsync(rh => rh.RHID == input.RHID);

                dataCheckFlag = head?.CheckFlag ?? false;

                if (head != null && head.B_StaticCode1.Code == ReserveHeadState.Terminated)
                {
                    AddError("預約單已中止，無法更新！");
                    return false;
                }
            }

            DateTime headStartDate = default;
            DateTime headEndDate = default;

            // 主預約單
            bool isHeadValid = await input.StartValidate()
                .Validate(i => isAdd ? i.RHID == 0 : i.RHID.IsZeroOrAbove(), () => AddError(WrongFormat("預約單 ID")))
                .ValidateAsync(
                    async i => isAdd || await DC.Resver_Head.ValidateIdExists(i.RHID, nameof(Resver_Head.RHID)),
                    () => AddError(NotFound("預約單 ID")))
                .ValidateAsync(async i => await SubmitValidateStaticCode(i.BSCID12, StaticCodeType.ResverStatus),
                    () => AddError(NotFound("預約狀態 ID")))
                .ValidateAsync(async i => await SubmitValidateStaticCode(i.BSCID11, StaticCodeType.ResverSource),
                    () => AddError(NotFound("預約來源 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("預約單名稱")))
                .Validate(i => i.SDate.TryParseDateTime(out headStartDate), () => AddError(WrongFormat("預約單起始日")))
                .Validate(i => i.EDate.TryParseDateTime(out headEndDate), () => AddError(WrongFormat("預約單結束日")))
                .Validate(i => headEndDate.Date >= headStartDate.Date,
                    () => AddError(MinLargerThanMax("預約單起始日", "預約單結束日")))
                .ValidateAsync(async i => await SubmitValidateCustomerId(i.CID), () => AddError(NotFound("客戶")))
                .Validate(i => i.CustomerTitle.HasContent(), () => AddError(EmptyNotAllowed("客戶名稱")))
                .Validate(i => i.ContactName.HasContent(), () => AddError(EmptyNotAllowed("聯絡人名稱")))
                .Validate(i => i.Title.HasLengthBetween(1, 100), () => AddError(LengthOutOfRange("預約單名稱", 1, 100)))
                .Validate(i => i.CustomerTitle.HasLengthBetween(1, 100),
                    () => AddError(LengthOutOfRange("客戶名稱", 1, 100)))
                .Validate(i => i.ContactName.HasLengthBetween(1, 50), () => AddError(LengthOutOfRange("聯絡人名稱", 1, 50)))
                .Validate(i => i.MK_Phone.HasLengthBetween(0, 50), () => AddError(LengthOutOfRange("MK 業務電話", 0, 50)))
                .Validate(i => i.OP_Phone.HasLengthBetween(0, 50), () => AddError(LengthOutOfRange("OP 業務電話", 0, 50)))
                .Validate(i => i.Note.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("備註", 0, 10)))
                .ValidateAsync(async i => await SubmitValidateMKBusinessUser(i.MK_BUID),
                    () => AddError(NotFound("MK 業務")))
                .ValidateAsync(async i => await SubmitValidateOPBusinessUser(i.OP_BUID),
                    () => AddError(NotFound("OP 業務")))
                .IsValid();

            // 驗證預約單狀態調整
            B_StaticCode resverStatusCode = await DC.B_StaticCode
                .FirstOrDefaultAsync(bsc => bsc.BSCID == input.BSCID12);

            // (1) 預約草稿：CheckFlag 為 true 時報錯
            // (2) 已付訂金：Resver_Bill 已付未刪除的資料數量為 0 時報錯
            // (3) 已結帳：Resver_Bill 已付總額不等於 QuotedPrice 時報錯。

            isHeadValid = isHeadValid && input.StartValidate()
                .Validate(i => resverStatusCode?.Code != ReserveHeadState.Draft || !dataCheckFlag,
                    () => AddError("預約已確認，無法設置為預約草稿狀態！"))
                .Validate(
                    i => resverStatusCode?.Code != ReserveHeadState.DepositPaid ||
                         input.BillItems.Any(bi => bi.PayFlag),
                    () => AddError("無已繳費紀錄，無法設置為已付訂金狀態！"))
                .Validate(i => resverStatusCode?.Code != ReserveHeadState.FullyPaid
                               || input.BillItems.Where(bi => bi.PayFlag).Sum(bi => bi.Price) == input.QuotedPrice,
                    () => AddError("繳費紀錄中已繳總額不等於預約單總價，無法設置為已結帳狀態！"))
                .IsValid();

            // short-circuit
            if (!isHeadValid)
                return false;

            // 主預約單 -> 聯絡方式
            string headTableName = DC.GetTableName<Resver_Head>();
            bool isContactItemValid = input.ContactItems.All(item =>
                item.StartValidate()
                    .Validate(ci => isAdd ? ci.MID == 0 : ci.MID.IsZeroOrAbove(),
                        () => AddError(WrongFormat($"聯絡方式對應 ID（{item.MID}）")))
                    .Validate(
                        ci => ci.MID == 0 || DC.M_Contect.Any(mc =>
                            mc.MID == ci.MID && mc.TargetTable == headTableName && mc.TargetID == input.RHID),
                        () => AddError(NotFound($"聯絡方式對應 ID（{item.MID}）")))
                    .Validate(ci => SubmitValidateContactType(ci.ContactType),
                        () => AddError(NotFound($"聯絡方式編號（{item.ContactType}）")))
                    .Validate(ci => ci.ContactData.HasContent(), () => AddError(EmptyNotAllowed($"聯絡方式內容")))
                    .Validate(ci => ci.ContactData.HasLengthBetween(1, 30),
                        () => AddError(LengthOutOfRange("聯絡方式內容", 1, 30)))
                    .IsValid());

            // 主預約單 -> 場地列表
            bool isSiteItemsValid = input.SiteItems.All(item =>
                item.StartValidate()
                    .Validate(si => isAdd ? si.RSID == 0 : si.RSID.IsZeroOrAbove(),
                        () => AddError(WrongFormat($"場地預約單 ID（{item.RSID}）")))
                    .Validate(
                        si => si.RSID == 0 || Task
                            .Run(() => DC.Resver_Site.ValidateIdExists(si.RSID, nameof(Resver_Site.RSID))).Result,
                        () => AddError(NotFound($"場地預約單 ID（{item.RSID}）")))
                    // 檢查所有場地的目標日期都位於 head 的日期範圍
                    .Validate(si => si.TargetDate.TryParseDateTime(out DateTime siteTargetDate)
                                    && headStartDate.Date <= siteTargetDate.Date &&
                                    siteTargetDate.Date <= headEndDate.Date,
                        () => AddError(OutOfRange($"場地使用日期（{item.TargetDate}）", headStartDate.ToFormattedStringDate(),
                            headEndDate.ToFormattedStringDate())))
                    .Validate(si => Task.Run(() => SubmitValidateSiteData(si.BSID)).Result,
                        () => AddError(NotFound($"場地 ID（{item.BSID}）")))
                    .Validate(si => Task.Run(() => SubmitValidateOrderCode(si.BOCID, OrderCodeType.Site)).Result,
                        () => AddError(NotFound($"預約場地的入帳代號 ID（{item.BOCID}）")))
                    .Validate(si => Task.Run(() => SubmitValidateStaticCode(si.BSCID, StaticCodeType.SiteTable)).Result,
                        () => AddError(NotFound($"預約場地的桌型 ID（{item.BSCID}）")))
                    .Validate(si => si.PrintTitle.HasLengthBetween(0, 100),
                        () => AddError(LengthOutOfRange("帳單列印名稱", 0, 100)))
                    .Validate(si => si.PrintNote.HasLengthBetween(0, 100),
                        () => AddError(LengthOutOfRange("帳單列印說明", 0, 100)))
                    .IsValid());

            // 檢查場地的總可容納人數大於等於預約單要求人數
            IEnumerable<int> siteItemIds = input.SiteItems.Select(si => si.BSID);
            int totalSize = await DC.B_SiteData.Where(sd => siteItemIds.Contains(sd.BSID)).SumAsync(sd => sd.MaxSize);

            isSiteItemsValid = isSiteItemsValid &&
                               input.SiteItems.StartValidate()
                                   .Validate(si => totalSize >= input.PeopleCt,
                                       () => AddError(TooLarge($"預約人數（{input.PeopleCt}）", totalSize)))
                                   .IsValid();

            // 主預約單 -> 場地列表 -> 時段列表
            bool isSiteItemTimeSpanItemValid = isSiteItemsValid
                                               && SubmitValidateTimeSpanItems(
                                                   input.SiteItems.SelectMany(si => si.TimeSpanItems), null)
                                               && await SubmitValidateSiteItemsAllTimeSpanFree(input)
                ;

            // 主預約單 -> 場地列表 -> 行程列表
            bool isSiteItemThrowItemValid = isSiteItemsValid &&
                                            input.SiteItems
                                                .All(si => si.ThrowItems.All(item =>
                                                    item.StartValidate()
                                                        .Validate(ti => isAdd ? ti.RTID == 0 : ti.RTID.IsZeroOrAbove(),
                                                            () => AddError(WrongFormat($"行程預約單 ID（{item.RTID}）")))
                                                        .Validate(
                                                            ti => ti.RTID == 0 || Task.Run(() =>
                                                                DC.Resver_Throw.ValidateIdExists(ti.RTID,
                                                                    nameof(Resver_Throw.RTID))).Result,
                                                            () => AddError(NotFound($"行程預約單 ID（{item.RTID}）")))
                                                        // 檢查所有行程的日期都等於場地的日期
                                                        .Validate(ti =>
                                                                ti.TargetDate.TryParseDateTime(
                                                                    out DateTime throwTargetDate)
                                                                && si.TargetDate.TryParseDateTime(
                                                                    out DateTime siteTargetDate)
                                                                && throwTargetDate.Date == siteTargetDate.Date,
                                                            () => AddError(OutOfRange($"預約行程的預計使用日期（{item.TargetDate}）",
                                                                si.TargetDate, si.TargetDate)))
                                                        .Validate(
                                                            ti => Task.Run(() =>
                                                                SubmitValidateStaticCode(ti.BSCID,
                                                                    StaticCodeType.ResverThrow)).Result,
                                                            () => AddError(WrongFormat($"預約類型（{item.BSCID}）")))
                                                        .Validate(
                                                            ti => Task.Run(() =>
                                                                    SubmitValidateOrderCode(ti.BOCID,
                                                                        OrderCodeType.Throw))
                                                                .Result,
                                                            () => AddError(NotFound($"預約行程的入帳代號 ID（{item.BOCID}）")))
                                                        .Validate(ti => ti.Title.HasLengthBetween(0, 100),
                                                            () => AddError(LengthOutOfRange("行程名稱", 0, 100)))
                                                        .Validate(ti => ti.PrintTitle.HasLengthBetween(0, 100),
                                                            () => AddError(LengthOutOfRange("行程的帳單列印名稱", 0, 100)))
                                                        .Validate(ti => ti.PrintNote.HasLengthBetween(0, 100),
                                                            () => AddError(LengthOutOfRange("行程的帳單列印說明", 0, 100)))
                                                        .IsValid()));

            // 主預約單 -> 場地列表 -> 行程列表 -> 時段列表
            bool isSiteItemThrowItemTimeSpanItemValid = isSiteItemThrowItemValid &&
                                                        input.SiteItems.StartValidateElements()
                                                            .Validate(si =>
                                                                SubmitValidateTimeSpanItems(
                                                                    si.ThrowItems.SelectMany(ti => ti.TimeSpanItems),
                                                                    SubmitValidateGetTimeSpans(
                                                                        si.TimeSpanItems.Select(tsi => tsi.DTSID))
                                                                )
                                                            )
                                                            .IsValid();

            // 主預約單 -> 場地列表 -> 行程列表 -> 餐飲補充列表
            bool isSiteItemThrowItemFoodItemValid = isSiteItemsValid &&
                                                    input.SiteItems
                                                        .SelectMany(si => si.ThrowItems)
                                                        .SelectMany(ti => ti.FoodItems)
                                                        .StartValidateElements()
                                                        .Validate(
                                                            fi => isAdd
                                                                ? fi.RTFID == 0
                                                                : fi.RTFID.IsZeroOrAbove(),
                                                            fi => AddError(
                                                                WrongFormat($"行程餐飲預約單 ID（{fi.RTFID}）")))
                                                        .Validate(
                                                            fi => fi.RTFID == 0 || Task.Run(() =>
                                                                DC.Resver_Throw_Food.ValidateIdExists(fi.RTFID,
                                                                    nameof(Resver_Throw_Food.RTFID))).Result,
                                                            fi => AddError(
                                                                NotFound($"行程餐飲預約單 ID（{fi.RTFID}）")))
                                                        .Validate(fi => SubmitValidateFoodCategory(fi.DFCID),
                                                            fi => AddError(
                                                                NotFound($"預約行程的餐種 ID（{fi.DFCID}）")))
                                                        .Validate(
                                                            fi => Task.Run(() =>
                                                                SubmitValidateStaticCode(fi.BSCID,
                                                                    StaticCodeType.Cuisine)).Result,
                                                            fi => AddError(
                                                                NotFound($"預約行程的餐別 ID（{fi.BSCID}）")))
                                                        .Validate(fi => SubmitValidatePartner(fi.BPID),
                                                            fi => AddError(
                                                                NotFound($"預約行程的廠商 ID（{fi.BPID}）")))
                                                        .IsValid()
                ;

            // 主預約單 -> 場地列表 -> 設備列表
            bool isSiteItemDeviceItemValid = isSiteItemsValid &&
                                             input.SiteItems.All(si => si.DeviceItems.All(item =>
                                                 item.StartValidate()
                                                     .Validate(di => isAdd ? di.RDID == 0 : di.RDID.IsZeroOrAbove(),
                                                         () => AddError(WrongFormat($"設備預約單 ID（{item.RDID}）")))
                                                     .Validate(di => di.RDID == 0 || Task.Run(() =>
                                                             DC.Resver_Device.ValidateIdExists(item.RDID,
                                                                 nameof(Resver_Device.RDID))).Result
                                                         , () => AddError(NotFound($"設備預約單 ID（{item.RDID}）")))
                                                     // 檢查所有設備的預約日期都與場地日期相符
                                                     .Validate(di =>
                                                             di.TargetDate.TryParseDateTime(
                                                                 out DateTime deviceTargetDate)
                                                             && si.TargetDate.TryParseDateTime(
                                                                 out DateTime siteTargetDate)
                                                             && deviceTargetDate.Date == siteTargetDate.Date,
                                                         () => AddError(OutOfRange($"預約設備的預計使用日期（{item.TargetDate}）",
                                                             si.TargetDate, si.TargetDate)))
                                                     .Validate(di => SubmitValidateDevice(di.BDID),
                                                         () => AddError(NotFound($"預約設備 ID（{item.BDID}）")))
                                                     .Validate(
                                                         di => Task.Run(() =>
                                                                 SubmitValidateOrderCode(di.BOCID,
                                                                     OrderCodeType.Device))
                                                             .Result,
                                                         () => AddError(NotFound($"預約設備的入帳代號 ID（{item.BOCID}）")))
                                                     .Validate(di => di.PrintTitle.HasLengthBetween(0, 100),
                                                         () => AddError(LengthOutOfRange("預約設備的帳單列印名稱")))
                                                     .Validate(di => di.PrintNote.HasLengthBetween(0, 100),
                                                         () => AddError(LengthOutOfRange("預約設備的帳單列印說明")))
                                                     .IsValid()
                                             ));

            // 主預約單 -> 場地列表 -> 設備列表 -> 時段列表
            bool isSiteItemDeviceItemTimeSpanItemValid = isSiteItemDeviceItemValid
                                                         && input.SiteItems.StartValidateElements()
                                                             .Validate(si =>
                                                                 SubmitValidateTimeSpanItems(
                                                                     si.DeviceItems.SelectMany(di => di.TimeSpanItems),
                                                                     SubmitValidateGetTimeSpans(
                                                                         si.TimeSpanItems.Select(tsi => tsi.DTSID))
                                                                 )
                                                             )
                                                             .IsValid();

            // 確認設備預約時段的數量足不足夠
            isSiteItemDeviceItemTimeSpanItemValid = isSiteItemDeviceItemTimeSpanItemValid &&
                                                    await SubmitValidateSiteItemDeviceItemsAllTimeSpanEnough(input);

            // 主預約單 -> 其他收費項目列表
            bool isOtherItemValid = await
                input.OtherItems.StartValidateElements()
                    .Validate(oi => isAdd ? oi.ROID == 0 : oi.ROID.IsZeroOrAbove(),
                        item => AddError(WrongFormat($"其他收費項目預約單 ID（{item.ROID}）")))
                    .Validate(
                        oi => oi.ROID == 0 || Task.Run(() =>
                            DC.Resver_Other.ValidateIdExists(oi.ROID, nameof(Resver_Other.ROID))).Result,
                        item => AddError(NotFound($"其他收費項目預約單 ID（{item.ROID}）")))
                    // 檢查所有項目的日期都與主預約單相符
                    .Validate(oi => oi.TargetDate.TryParseDateTime(out DateTime otherItemDate)
                                    && headStartDate.Date <= otherItemDate.Date
                                    && otherItemDate.Date <= headEndDate.Date,
                        item => AddError(OutOfRange($"其他收費項目的預計使用日期（{item.TargetDate}）",
                            headStartDate.ToFormattedStringDate(), headEndDate.ToFormattedStringDate()))
                    )
                    .Validate(oi => SubmitValidateOtherPayItem(oi.DOPIID),
                        item => AddError(NotFound($"其他收費項目 ID（{item.DOPIID}）")))
                    .ValidateAsync(
                        async oi => await SubmitValidateOrderCode(oi.BOCID, OrderCodeType.OtherPayItem),
                        item => AddError(NotFound($"其他收費項目的入帳代號 ID（{item.BOCID}）")))
                    .ValidateAsync(
                        async oi => await DC.B_StaticCode.ValidateStaticCodeExists(oi.BSCID, StaticCodeType.Unit),
                        item => AddError(NotFound($"其他收費項目的單位別 ID（{item.BSCID}）")))
                    .Validate(oi => oi.PrintTitle.HasLengthBetween(0, 100),
                        item => AddError(LengthOutOfRange("其他收費項目的帳單列印名稱", 0, 100)))
                    .Validate(oi => oi.PrintNote.HasLengthBetween(0, 100),
                        item => AddError(LengthOutOfRange("其他收費項目的帳單列印說明", 0, 100)))
                    .IsValid();

            // 主預約單 -> 繳費紀錄列表
            bool isBillItemValid =
                input.BillItems.All(item => item.StartValidate()
                    .Validate(bi => isAdd ? bi.RBID == 0 : bi.RBID.IsZeroOrAbove(),
                        () => AddError(WrongFormat($"繳費紀錄預約單 ID（{item.RBID}）")))
                    .Validate(
                        bi => bi.RBID == 0 || Task
                            .Run(() => DC.Resver_Bill.ValidateIdExists(bi.RBID, nameof(Resver_Bill.RBID))).Result,
                        () => AddError(NotFound($"繳費紀錄預約單 ID（{item.RBID}）")))
                    .Validate(bi => SubmitValidateCategory(bi.BCID, CategoryType.PayType),
                        () => AddError(NotFound($"繳費類別 ID（{item.BCID}）")))
                    .Validate(bi => SubmitValidatePayType(bi.DPTID),
                        () => AddError(NotFound($"繳費紀錄的付款方式 ID（{item.DPTID}）")))
                    .Validate(
                        bi => bi.PayDate.IsNullOrWhiteSpace() ||
                              bi.PayDate.TryParseDateTime(out _, DateTimeParseType.DateTime),
                        () => AddError(WrongFormat($"付款時間（{item.PayDate}）")))
                    .IsValid());

            // 已付總額不得超過 head 總價
            isBillItemValid = isBillItemValid &&
                              input.BillItems.StartValidate()
                                  .Validate(billItems => billItems
                                                             .Where(bi => bi.PayFlag)
                                                             .Sum(bi => bi.Price)
                                                         <= input.QuotedPrice,
                                      () => AddError(TooLarge("繳費紀錄的已繳總額", input.QuotedPrice)))
                                  .IsValid();

            // 主預約單 -> 預約回饋紀錄列表
            bool isGiveBackItemValid = await
                input.GiveBackItems.StartValidateElements()
                    .Validate(gbi => isAdd ? gbi.RGBID == 0 : gbi.RGBID.IsZeroOrAbove(),
                        gbi => AddError(WrongFormat($"預約回饋預約單 ID（{gbi.RGBID}）")))
                    .ValidateAsync(
                        async gbi =>
                            gbi.RGBID == 0 ||
                            await DC.Resver_GiveBack.ValidateIdExists(gbi.RGBID, nameof(Resver_GiveBack.RGBID)),
                        gbi => AddError(NotFound($"預約回饋預約單 ID（{gbi.RGBID}）")))
                    .ValidateAsync(
                        async gbi =>
                            await DC.B_StaticCode.ValidateStaticCodeExists(gbi.BSCID16, StaticCodeType.GiveBackScore),
                        gbi => AddError(NotFound($"回饋分數 ID（{gbi.BSCID16}）")))
                    .Validate(gbi => gbi.Title.HasLengthBetween(0, 100),
                        () => AddError(LengthOutOfRange("預約回饋的標題", 0, 100)))
                    .Validate(gbi => gbi.Description.HasLengthBetween(0, 100),
                        () => AddError(LengthOutOfRange("預約回饋的內容", 0, 100)))
                    .IsValid();

            // 輸入都正確後，才計算各項目價格
            bool isEveryPriceValid = isContactItemValid
                                     && isSiteItemsValid
                                     && isSiteItemTimeSpanItemValid
                                     && isSiteItemThrowItemValid
                                     && isSiteItemThrowItemTimeSpanItemValid
                                     && isSiteItemThrowItemFoodItemValid
                                     && isSiteItemDeviceItemValid
                                     && isSiteItemDeviceItemTimeSpanItemValid
                                     && isOtherItemValid
                                     && isBillItemValid
                                     && isGiveBackItemValid
                                     && SubmitValidateAllPrices(input);

            return await Task.FromResult(isEveryPriceValid);
        }

        private bool SubmitValidateAllPrices(Resver_Submit_Input_APIItem input)
        {
            // 子項目的價格都能夠修改，所以只簡單計算總定價、總報價是否符合輸入的總和

            // 子項目所有總定價
            int subItemsFixedPriceTotal =
                // 場地
                input.SiteItems.Sum(si => si.FixedPrice) +
                // 行程
                input.SiteItems.SelectMany(si => si.ThrowItems).Sum(ti => ti.FixedPrice) +
                // 設備
                input.SiteItems.SelectMany(si => si.DeviceItems).Sum(di => di.FixedPrice) +
                // 其他項目
                input.OtherItems.Sum(oi => oi.FixedPrice);

            // 子項目所有總報價
            int subItemsQuotedPriceTotal =
                // 場地
                input.SiteItems.Sum(si => si.QuotedPrice) +
                // 行程
                input.SiteItems.SelectMany(si => si.ThrowItems).Sum(ti => ti.QuotedPrice) +
                // 設備
                input.SiteItems.SelectMany(si => si.DeviceItems).Sum(di => di.QuotedPrice) +
                // 其他項目
                input.OtherItems.Sum(oi => oi.QuotedPrice);

            bool isValid = input.StartValidate()
                .Validate(i => i.FixedPrice == subItemsFixedPriceTotal,
                    i => AddError(NotEqual("預約單總定價", subItemsFixedPriceTotal)))
                .Validate(i => i.QuotedPrice == subItemsQuotedPriceTotal,
                    i => AddError(NotEqual("預約單總報價", subItemsQuotedPriceTotal)))
                .IsValid();

            return isValid;
        }

        private async Task<bool> SubmitValidateSiteItemDeviceItemsAllTimeSpanEnough(Resver_Submit_Input_APIItem input)
        {
            bool result = true;
            string resverDeviceTableName = DC.GetTableName<Resver_Device>();

            // 先一次查完所有 BDID
            IEnumerable<Resver_Submit_DeviceItem_Input_APIItem> allDeviceItems =
                input.SiteItems.SelectMany(si => si.DeviceItems).ToArray();
            IEnumerable<int> inputDeviceIds = allDeviceItems
                .Select(di => di.BDID)
                .AsEnumerable();

            Dictionary<int, B_Device> devices = await DC.B_Device
                .Include(bd => bd.Resver_Device)
                .Include(bd => bd.Resver_Device.Select(rd => rd.Resver_Site))
                .Include(bd => bd.M_Site_Device)
                .Where(bd =>
                    bd.ActiveFlag && !bd.DeleteFlag &&
                    inputDeviceIds.Any(id => id == bd.BDID))
                .ToDictionaryAsync(bd => bd.BDID, bd => bd);

            // 每個場地
            foreach (Resver_Submit_SiteItem_Input_APIItem siteItem in input.SiteItems)
            {
                B_SiteData siteData = await DC.B_SiteData.FirstOrDefaultAsync(sd => sd.BSID == siteItem.BSID);
                // 每個設備
                foreach (Resver_Submit_DeviceItem_Input_APIItem deviceItem in siteItem.DeviceItems)
                {
                    if (!devices.ContainsKey(deviceItem.BDID))
                    {
                        AddError(NotFound($"欲預約的設備 ID {deviceItem.BDID}"));
                        result = false;
                        continue;
                    }

                    // 查出所有對應 deviceItem.DTSID 的 DTS
                    var allInputDtsIds = deviceItem.TimeSpanItems.Select(tsi => tsi.DTSID);
                    D_TimeSpan[] wantedTimeSpans = await DC.D_TimeSpan
                        .Where(dts => dts.ActiveFlag)
                        .Where(dts => !dts.DeleteFlag)
                        .Where(dts => allInputDtsIds.Any(id => id == dts.DTSID))
                        .ToArrayAsync();

                    DateTime targetDate = deviceItem.TargetDate.ParseDateTime();

                    // 每個時段
                    foreach (D_TimeSpan timeSpan in wantedTimeSpans)
                    {
                        // 計算此設備預約單以外的預約單中，在同一場地預約了同一設備的總數量
                        Resver_Device[] otherResverDevices = await DC.Resver_Device
                            .Include(rs => rs.Resver_Site)
                            .Include(rs => rs.Resver_Site.B_SiteData)
                            .Include(rs => rs.Resver_Site.B_SiteData.M_SiteGroup1)
                            // 選出所有不是這張設備預約單的設備預約單，同場地或者其子場地，並且是同一天、未刪除
                            .Where(rd => !rd.DeleteFlag)
                            .Where(rd => DbFunctions.TruncateTime(rd.TargetDate) == targetDate.Date)
                            .Where(rd => rd.RDID != deviceItem.RDID)
                            .Where(rd => rd.Resver_Site.BSID == siteItem.BSID
                                         || rd.Resver_Site.B_SiteData.M_SiteGroup1.Any(child =>
                                             child.MasterID == siteItem.BSID))
                            .Where(rd => rd.Resver_Site.B_SiteData.ActiveFlag && !rd.Resver_Site.B_SiteData.DeleteFlag)
                            // 存到記憶體，因為接下來又要查 DB 了
                            .ToArrayAsync();

                        IEnumerable<int> otherResverDeviceIds = otherResverDevices
                            .Select(ord => ord.RDID)
                            .AsEnumerable();

                        // 選出它們的 RTS，當發現重疊時段時，計入 reservedCount
                        IEnumerable<int> crossingResverDeviceIds = DC.M_Resver_TimeSpan
                            .Include(rts => rts.D_TimeSpan)
                            .Where(rts => rts.TargetTable == resverDeviceTableName)
                            .Where(rts => otherResverDeviceIds.Contains(rts.TargetID))
                            .AsEnumerable()
                            .Where(rts => rts.D_TimeSpan.IsCrossingWith(timeSpan))
                            .Select(rts => rts.TargetID);

                        int reservedCount = DC.Resver_Device
                            .Where(rd => crossingResverDeviceIds.Contains(rd.RDID))
                            .Sum(rd => (int?)rd.Ct) ?? 0;

                        // 總可用數量，取用
                        // 1. 該設備在此場地的數量
                        // 2. 該設備在此場地之子場地的數量
                        // 之總和

                        int totalCt = devices[deviceItem.BDID].M_Site_Device
                            .Where(msd => msd.BSID == siteItem.BSID)
                            .Sum(msd => (int?)msd.Ct) ?? 0;

                        // 所有子場地此設備的庫存
                        int implicitCt = await DC.M_SiteGroup
                            .Include(msg => msg.B_SiteData1)
                            .Include(msg => msg.B_SiteData1.M_Site_Device)
                            .Where(msg => msg.MasterID == siteItem.BSID)
                            .Where(msg => msg.ActiveFlag && !msg.DeleteFlag)
                            .Select(msg => msg.B_SiteData1)
                            .Where(sd => sd.ActiveFlag && !sd.DeleteFlag)
                            .SelectMany(sd => sd.M_Site_Device)
                            .Where(msd => msd.BDID == deviceItem.BDID)
                            .SumAsync(msd => (int?)msd.Ct) ?? 0;

                        totalCt += implicitCt;

                        if (totalCt - reservedCount >= deviceItem.Ct) continue;

                        AddError(
                            $"{siteData?.Title ?? $"場地 ID {siteItem.BSID}"} 欲預約的設備 {devices[deviceItem.BDID].Title} 在 {timeSpan.GetTimeRangeFormattedString()} 的可用數量不足（總數：{totalCt}，欲預約數量：{deviceItem.Ct}，已預約數量：{reservedCount}）！");
                        result = false;
                    }
                }
            }

            return result;
        }

        private async Task<bool> SubmitValidateSiteItemsAllTimeSpanFree(Resver_Submit_Input_APIItem input)
        {
            bool isValid = true;
            foreach (Resver_Submit_SiteItem_Input_APIItem si in input.SiteItems)
            {
                // 1. 取得這個場地當天所有 RTS，包括場地本身、場地的父場地、場地的子場地
                B_SiteData siteData =
                    await DC.B_SiteData.FirstOrDefaultAsync(sd =>
                        sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == si.BSID);
                M_Resver_TimeSpan[] allResverTimeSpans = SubmitGetAllResverTimeSpanFromSiteItem(input, si)
                    .ToArray();

                // 2. RTS 的 DTSID = 當天已被占用的 DTSID，從輸入中抓出此類 DTSID
                isValid &= allResverTimeSpans
                    .StartValidateElements()
                    .Validate(rts => si.TimeSpanItems.All(tsi => tsi.DTSID != rts.DTSID),
                        rts => AddError(
                            $"{siteData?.Title ?? $"場地 ID {si.BSID}"} 欲預約的時段（{rts.D_TimeSpan.GetTimeRangeFormattedString()}）當天已被預約了！")
                    )
                    .IsValid();


                // 3. 所有 TimeSpanItem 的 DTS 時段不可與 allResverTimeSpans 任一者的 DTS 時段重疊
                // 先查出所有輸入 DTSID 的 DTS 資料
                var inputDtsIds = si.TimeSpanItems.Select(tsi => tsi.DTSID);
                List<D_TimeSpan> allInputDts = await DC.D_TimeSpan
                    .Where(dts =>
                        dts.ActiveFlag
                        && !dts.DeleteFlag
                        && inputDtsIds.Any(id => id == dts.DTSID)
                    )
                    .ToListAsync();

                // 每個 DTS 和 RTS 比對一次，看是否有重疊的部分
                foreach (D_TimeSpan dts in allInputDts)
                {
                    isValid &= allResverTimeSpans
                        // 排除同一場地預約單
                        .Where(rts => rts.TargetID != si.RSID)
                        .Aggregate(true, (result, rts) => result & rts.StartValidate()
                            .Validate(_ => rts.DTSID == dts.DTSID || !rts.D_TimeSpan.IsCrossingWith(dts),
                                () => AddError(
                                    $"{siteData?.Title ?? $"場地 ID {si.BSID}"} 欲預約的時段（{dts.GetTimeRangeFormattedString()}）與當天另一個已被預約的時段（{rts.D_TimeSpan.GetTimeRangeFormattedString()}）部分重疊！")
                            )
                            .IsValid());
                }
            }

            return isValid;
        }

        private IEnumerable<M_Resver_TimeSpan> SubmitGetAllResverTimeSpanFromSiteItem(Resver_Submit_Input_APIItem input,
            Resver_Submit_SiteItem_Input_APIItem si)
        {
            string resverSiteTableName = DC.GetTableName<Resver_Site>();
            DateTime targetDate = si.TargetDate.ParseDateTime();
            return DC.B_SiteData.Where(sd =>
                        sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == si.BSID)
                    .AsEnumerable()
                    .Concat(DC.M_SiteGroup
                        .Where(sg =>
                            sg.ActiveFlag && !sg.DeleteFlag &&
                            sg.MasterID == si.BSID)
                        .Select(sg => sg.B_SiteData1)
                        .AsEnumerable())
                    .Concat(DC.M_SiteGroup
                        .Where(sg =>
                            sg.ActiveFlag && !sg.DeleteFlag &&
                            sg.GroupID == si.BSID)
                        .Select(sg => sg.B_SiteData)
                        .AsEnumerable())
                    // 取得除去本預約單以外，每個場地在指定日期當天的預約
                    .SelectMany(sd => sd.Resver_Site.Where(rs => rs.RHID != input.RHID)
                        .Where(rs => !rs.DeleteFlag)
                        .Where(rs => rs.TargetDate.Date == targetDate.Date))
                    // 取得每個場地的預約時段
                    .SelectMany(rs => DC.M_Resver_TimeSpan
                        .Include(rts => rts.D_TimeSpan)
                        // 這裡的 TargetID == rs.RSID 處理的對象是「本預約單以外的 RS」，所以不會造成只搜到這張場地預約單自己的情況
                        .Where(rts =>
                            rts.TargetTable == resverSiteTableName &&
                            rts.TargetID == rs.RSID)
                    )
                ;
        }

        private bool SubmitValidatePayType(int payTypeId)
        {
            return payTypeId.IsAboveZero() &&
                   DC.D_PayType.Any(pt => pt.ActiveFlag && !pt.DeleteFlag && pt.DPTID == payTypeId);
        }

        private bool SubmitValidateCategory(int categoryId, CategoryType categoryType)
        {
            return Task.Run(() => DC.B_Category.ValidateCategoryExists(categoryId, categoryType)).Result;
        }

        private bool SubmitValidateOtherPayItem(int otherPayItemId)
        {
            return otherPayItemId.IsAboveZero() &&
                   DC.D_OtherPayItem.Any(opi => opi.ActiveFlag && !opi.DeleteFlag && opi.DOPIID == otherPayItemId);
        }

        private bool SubmitValidateDevice(int deviceId)
        {
            return deviceId.IsAboveZero() &&
                   DC.B_Device.Any(bd => bd.ActiveFlag && !bd.DeleteFlag && bd.BDID == deviceId);
        }

        private bool SubmitValidatePartner(int partnerId)
        {
            return Task.Run(() => DC.B_Partner.ValidatePartnerExists(partnerId)).Result;
        }

        private bool SubmitValidateFoodCategory(int foodCategoryId)
        {
            return foodCategoryId.IsAboveZero() && DC.D_FoodCategory.Any(dfc =>
                dfc.ActiveFlag && !dfc.DeleteFlag && dfc.DFCID == foodCategoryId);
        }

        private Dictionary<int, D_TimeSpan> SubmitValidateGetTimeSpansDictionary(IEnumerable<int> DtsIds)
        {
            return DC.D_TimeSpan.Where(dts => DtsIds.Contains(dts.DTSID)).ToDictionary(dts => dts.DTSID, dts => dts);
        }

        private IEnumerable<D_TimeSpan> SubmitValidateGetTimeSpans(IEnumerable<int> DtsIds)
        {
            return DC.D_TimeSpan.Where(dts => DtsIds.Contains(dts.DTSID)).ToArray();
        }

        /// <summary>
        /// 驗證輸入的 TimeSpanItem 是否格式正確。<br/>
        /// 當 parentTimeSpan 不為 null 時，驗證時段是否都包含於 parentTimeSpan 中的時段（考慮 DTSID 與實際時間）
        /// </summary>
        /// <param name="items">輸入</param>
        /// <param name="parentTimeSpan">用於檢查的上層項目預約時段</param>
        /// <returns>
        /// true：時段皆正確。<br/>
        /// false：有時段格式錯誤，或是不存在於上層項目預約的時段中。
        /// </returns>
        private bool SubmitValidateTimeSpanItems(IEnumerable<Resver_Submit_TimeSpanItem_Input_APIItem> items,
            IEnumerable<D_TimeSpan> parentTimeSpan)
        {
            Resver_Submit_TimeSpanItem_Input_APIItem[] itemsArray = items.ToArray();

            int[] inputDtsIds = itemsArray.Select(i => i.DTSID).ToArray();

            // 查出輸入的 TimeSpanItem 的所有對應的 DTS
            Dictionary<int, D_TimeSpan> dtsData = SubmitValidateGetTimeSpansDictionary(inputDtsIds);

            // 驗證所有 DTSID 都存在
            bool isInputValid = inputDtsIds.StartValidateElements()
                .Validate(id => dtsData.ContainsKey(id), id => AddError(NotFound($"預約時段 ID {id}")))
                .IsValid();

            if (!isInputValid)
                return false;

            // 驗證所有 items 的區間都存在於 parentTimeSpan 中
            // 如果沒有傳入 parentTimeSpan, 表示沒有限制

            if (parentTimeSpan == null)
                return true;

            bool isValid = dtsData.Values.StartValidateElements()
                .Validate(dts => parentTimeSpan.Any(parent => parent.DTSID == dts.DTSID || parent.IsIncluding(dts))
                    , dts => AddError($"欲預約的時段（{dts.GetTimeRangeFormattedString()}）並不存在於上層項目的預約時段！"))
                .IsValid();
            return isValid;
        }

        private async Task<bool> SubmitValidateOrderCode(int orderCodeId, OrderCodeType codeType)
        {
            return await DC.B_OrderCode.ValidateOrderCodeExists(orderCodeId, codeType);
        }

        private async Task<bool> SubmitValidateSiteData(int siteDataId)
        {
            return siteDataId.IsAboveZero() &&
                   await DC.B_SiteData.AnyAsync(sd => sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == siteDataId);
        }

        private static bool SubmitValidateContactType(int contactType)
        {
            return ContactTypeController.GetContactTypeList().Any(ct => ct.ID == contactType);
        }

        private async Task<bool> SubmitValidateOPBusinessUser(int businessUserId)
        {
            return businessUserId.IsAboveZero() && await DC.BusinessUser.AnyAsync(bu =>
                bu.ActiveFlag && !bu.DeleteFlag && bu.OPsalesFlag && bu.BUID == businessUserId);
        }

        private async Task<bool> SubmitValidateMKBusinessUser(int businessUserId)
        {
            return businessUserId.IsAboveZero() && await DC.BusinessUser.AnyAsync(bu =>
                bu.ActiveFlag && !bu.DeleteFlag && bu.MKsalesFlag && bu.BUID == businessUserId);
        }

        private async Task<bool> SubmitValidateCustomerId(int customerId)
        {
            return customerId.IsAboveZero() &&
                   await DC.Customer.AnyAsync(c => c.ActiveFlag && !c.DeleteFlag && c.CID == customerId);
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

            // 已結帳時，只允許處理預約回饋紀錄的值
            if (isAdd || head.B_StaticCode1.Code != ReserveHeadState.FullyPaid)
            {
                SubmitPopulateHeadValues(input, head);
                // 為新資料時, 先寫入 DB, 這樣才有 RHID 可以提供給後面的功能用
                if (head.RHID == 0)
                {
                    await DC.AddAsync(head);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                // 清理所有跟這張預約單有關的 ResverTimeSpan
                DC.M_Resver_TimeSpan.RemoveRange(head.M_Resver_TimeSpan);

                // 開始寫入值
                SubmitPopulateHeadContactItems(input, head, entitiesToAdd, isAdd);
                await SubmitPopulateHeadSiteItems(input, head, entitiesToAdd);
                SubmitPopulateHeadOtherItems(input, head, entitiesToAdd);
                SubmitPopulateHeadBillItems(input, head, entitiesToAdd);
            }

            SubmitPopulateHeadGiveBackItems(input, head, entitiesToAdd);

            // 寫入 Db
            await DC.AddRangeAsync(entitiesToAdd);
            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            return head;
        }

        private void AddErrorNotThisHead(int itemId, string itemName, int dataHeadId)
        {
            if (itemId != 0)
                AddError($"欲更新的{itemName}（ID {itemId}）並不屬於此預約單（該{itemName}對應預約單 ID：{dataHeadId}）！");
            else
                AddError($"欲新增的{itemName}並不屬於此預約單（該{itemName}對應預約單 ID：{dataHeadId}）！");
        }

        private void AddErrorNotThisThrow(int itemId, string itemName, int dataThrowId)
        {
            if (itemId != 0)
                AddError($"欲更新的{itemName}（ID {itemId}）並不屬於此預約行程（該預約行程 ID：{dataThrowId}）！");
            else
                AddError($"欲新增的{itemName}並不屬於此預約行程（該預約行程 ID：{dataThrowId}）！");
        }

        private void SubmitPopulateHeadGiveBackItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            // 刪除沒有在輸入中的 giveback
            var inputIds = input.GiveBackItems.Select(gbi => gbi.RGBID);
            DC.Resver_GiveBack.RemoveRange(head.Resver_GiveBack.Where(rgb => !inputIds.Contains(rgb.RGBID)));

            foreach (Resver_Submit_GiveBackItem_Input_APIItem item in input.GiveBackItems)
            {
                Resver_GiveBack giveBack = SubmitFindOrCreateNew<Resver_GiveBack>(item.RGBID, entitiesToAdd);
                if (giveBack.RHID != 0 && giveBack.RHID != head.RHID)
                {
                    AddErrorNotThisHead(item.RGBID, "預約回饋紀錄", giveBack.RHID);
                    continue;
                }

                giveBack.RHID = head.RHID;
                giveBack.Title = item.Title;
                giveBack.Description = item.Description;
                giveBack.BSCID16 = item.BSCID16;
            }
        }

        private void SubmitPopulateHeadBillItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            var inputIds = input.BillItems.Select(bi => bi.RBID);
            DC.Resver_Bill.RemoveRange(head.Resver_Bill.Where(rb => !inputIds.Contains(rb.RBID)));
            foreach (var item in input.BillItems)
            {
                Resver_Bill bill = SubmitFindOrCreateNew<Resver_Bill>(item.RBID, entitiesToAdd);
                if (bill.RHID != 0 && bill.RHID != head.RHID)
                {
                    AddErrorNotThisHead(bill.RBID, "繳費紀錄", bill.RHID);
                    continue;
                }

                bill.RHID = head.RHID;
                bill.BCID = item.BCID;
                bill.DPTID = item.DPTID;
                bill.Price = item.Price;
                bill.Note = item.Note;
                bill.PayFlag = item.PayFlag;
                bill.PayDate = item.PayDate.HasContent() ? item.PayDate.ParseDateTime() : SqlDateTime.MinValue.Value;
                bill.CheckUID = head.UpdUID;
            }
        }

        private void SubmitPopulateHeadOtherItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            var inputIds = input.OtherItems.Select(oi => oi.ROID);
            DC.Resver_Other.RemoveRange(head.Resver_Other.Where(ro => !inputIds.Contains(ro.ROID)));

            foreach (var item in input.OtherItems)
            {
                Resver_Other other = SubmitFindOrCreateNew<Resver_Other>(item.ROID, entitiesToAdd);
                if (other.RHID != 0 && other.RHID != head.RHID)
                {
                    AddErrorNotThisHead(other.ROID, "其他收費項目", other.RHID);
                    continue;
                }

                other.TargetDate = item.TargetDate.ParseDateTime().Date;
                other.RHID = head.RHID;
                other.DOPIID = item.DOPIID;
                other.BOCID = item.BOCID;
                other.BSCID = item.BSCID;
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

        private async Task SubmitPopulateHeadSiteItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            IList<object> entitiesToAdd)
        {
            var inputIds = input.SiteItems.Select(si => si.RSID);
            DC.Resver_Site.RemoveRange(head.Resver_Site.Where(rs => !inputIds.Contains(rs.RSID)));

            foreach (var item in input.SiteItems)
            {
                Resver_Site site = SubmitFindOrCreateNew<Resver_Site>(item.RSID);
                if (site.RHID != 0 && site.RHID != head.RHID)
                {
                    AddErrorNotThisHead(site.RSID, "場地", site.RHID);
                    continue;
                }

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
                if (site.RSID == 0)
                {
                    await DC.AddAsync(site);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                SubmitPopulateSiteItemTimeSpanItems(item, head, site);
                await SubmitPopulateSiteItemThrowItems(item, head, site, entitiesToAdd);
                await SubmitPopulateSiteItemDeviceItems(item, head, site);
            }
        }

        private async Task SubmitPopulateSiteItemDeviceItems(Resver_Submit_SiteItem_Input_APIItem item,
            Resver_Head head, Resver_Site site)
        {
            var inputIds = item.DeviceItems.Select(di => di.RDID);
            DC.Resver_Device.RemoveRange(site.Resver_Device.Where(rd => !inputIds.Contains(rd.RDID)));
            foreach (var deviceItem in item.DeviceItems)
            {
                Resver_Device device = SubmitFindOrCreateNew<Resver_Device>(deviceItem.RDID);
                if (device.Resver_Site != null && device.Resver_Site.RHID != 0 && device.Resver_Site.RHID != head.RHID)
                {
                    AddErrorNotThisHead(device.RDID, "場地設備", device.Resver_Site.RHID);
                    continue;
                }

                device.TargetDate = deviceItem.TargetDate.ParseDateTime().Date;
                device.RSID = site.RSID;
                device.BDID = deviceItem.BDID;
                device.Ct = deviceItem.Ct;
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
                if (device.RDID == 0)
                {
                    await DC.Resver_Device.AddAsync(device);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                // 寫入 device 的 TimeSpan
                int sortNo = 0;
                string deviceTableName = DC.GetTableName<Resver_Device>();
                foreach (var timeSpanItem in deviceItem.TimeSpanItems)
                {
                    M_Resver_TimeSpan ts = new M_Resver_TimeSpan
                    {
                        RHID = head.RHID,
                        TargetTable = deviceTableName,
                        TargetID = device.RDID,
                        DTSID = timeSpanItem.DTSID,
                        SortNo = ++sortNo
                    };

                    head.M_Resver_TimeSpan.Add(ts);
                }
            }
        }

        private async Task SubmitPopulateSiteItemThrowItems(Resver_Submit_SiteItem_Input_APIItem item,
            Resver_Head head, Resver_Site site, IList<object> entitiesToAdd)
        {
            DC.Resver_Throw.RemoveRange(site.Resver_Throw.Where(rt => item.ThrowItems.All(ti => ti.RTID != rt.RTID)));
            foreach (var throwItem in item.ThrowItems)
            {
                Resver_Throw throwData = SubmitFindOrCreateNew<Resver_Throw>(throwItem.RTID);
                if (throwData.Resver_Site != null && throwData.Resver_Site.RHID != 0 &&
                    throwData.Resver_Site.RHID != head.RHID)
                {
                    AddErrorNotThisHead(throwData.RTID, "場地行程", throwData.Resver_Site.RHID);
                    continue;
                }

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
                if (throwData.RTID == 0)
                {
                    await DC.AddAsync(throwData);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                SubmitPopulateSiteItemThrowItemTimeSpanItems(throwData, throwItem, head);
                SubmitPopulateSiteItemThrowItemThrowFoodItems(throwData, throwItem, entitiesToAdd);
            }
        }

        private void SubmitPopulateSiteItemThrowItemThrowFoodItems(Resver_Throw throwData,
            Resver_Submit_ThrowItem_Input_APIItem throwItem, IList<object> entitiesToAdd)
        {
            DC.Resver_Throw_Food.RemoveRange(
                throwData.Resver_Throw_Food.Where(rtf => throwItem.FoodItems.All(fi => fi.RTFID != rtf.RTFID)));

            foreach (Resver_Submit_FoodItem_Input_APIItem foodItem in throwItem.FoodItems)
            {
                Resver_Throw_Food throwFood = SubmitFindOrCreateNew<Resver_Throw_Food>(foodItem.RTFID, entitiesToAdd);
                if (throwFood.Resver_Throw != null && throwFood.Resver_Throw.RTID != 0 &&
                    throwFood.Resver_Throw.RTID != throwData.RTID)
                {
                    AddErrorNotThisThrow(throwFood.RTFID, "餐飲補充資料", throwFood.Resver_Throw.RTID);
                    continue;
                }

                throwFood.RTID = throwData.RTID;
                throwFood.DFCID = foodItem.DFCID;
                throwFood.BSCID = foodItem.BSCID;
                throwFood.BPID = foodItem.BPID;
                throwFood.Ct = foodItem.Ct;
                throwFood.UnitPrice = foodItem.UnitPrice;
                throwFood.Price = foodItem.Price;
            }
        }

        private void SubmitPopulateSiteItemThrowItemTimeSpanItems(Resver_Throw throwData,
            Resver_Submit_ThrowItem_Input_APIItem throwItem,
            Resver_Head head)
        {
            string throwTableName = DC.GetTableName<Resver_Throw>();
            int sortNo = 0;
            foreach (var timeSpanItem in throwItem.TimeSpanItems)
            {
                M_Resver_TimeSpan resverTimeSpan = new M_Resver_TimeSpan
                {
                    RHID = head.RHID,
                    TargetTable = throwTableName,
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
            string siteTableName = DC.GetTableName<Resver_Site>();

            foreach (var timeSpanItem in item.TimeSpanItems)
            {
                M_Resver_TimeSpan resverTimeSpan = new M_Resver_TimeSpan
                {
                    RHID = head.RHID,
                    TargetTable = siteTableName,
                    TargetID = site.RSID,
                    DTSID = timeSpanItem.DTSID,
                    SortNo = ++sortNo
                };

                head.M_Resver_TimeSpan.Add(resverTimeSpan);
            }
        }

        private void SubmitPopulateHeadContactItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd, bool isAdd)
        {
            string tableName = DC.GetTableName<Resver_Head>();
            if (!isAdd)
            {
                IEnumerable<int> inputIds = input.ContactItems.Select(ci => ci.MID);

                // 先清除所有原本有，但不存在於本次輸入的 M_Contect
                var originalContacts = DC.M_Contect
                    .Where(c => c.TargetTable == tableName && c.TargetID == input.RHID)
                    .Where(c => !inputIds.Contains(c.MID))
                    .AsEnumerable();

                DC.M_Contect.RemoveRange(originalContacts);
            }

            int sortNo = 0;
            foreach (var contactItem in input.ContactItems)
            {
                M_Contect contact = SubmitFindOrCreateNew<M_Contect>(contactItem.MID, entitiesToAdd);
                contact.MID = contactItem.MID;
                contact.ContectType = contactItem.ContactType;
                contact.TargetTable = tableName;
                contact.TargetID = head.RHID;
                contact.ContectData = contactItem.ContactData;
                contact.SortNo = ++sortNo;
            }
        }

        private static void SubmitPopulateHeadValues(Resver_Submit_Input_APIItem input, Resver_Head head)
        {
            head.BSCID12 = input.BSCID12;
            head.BSCID11 = input.BSCID11;
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
                t = DC.Set<T>().Find(id);
            if (t != null
                && (t.GetIfHasProperty<T, bool?>(DbConstants.ActiveFlag) ?? true)
                && (t.GetIfHasProperty<T, bool?>(DbConstants.DeleteFlag) ?? false) == false)
            {
                // 取得這個物件的 navigationProperties
                var objectContext = ((IObjectContextAdapter)DC).ObjectContext;

                var entityType = objectContext
                    .MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.CSpace)
                    .FirstOrDefault(et => et.Name == t.GetType().Name);

                if (entityType == null)
                    return t;

                var navigationProperties = entityType
                    .NavigationProperties;

                // 讀取所有 FK property
                foreach (string propertyName in navigationProperties.Select(navigationProperty =>
                             navigationProperty.Name))
                {
                    objectContext.LoadProperty(t, propertyName);
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
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Head))
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