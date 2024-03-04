using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
using NS_Education.Tools.Filters;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public partial class ResverController : PublicClass,
        IGetListPaged<Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>,
        IGetInfoById<Resver_Head, Resver_GetAllInfoById_Output_APIItem>,
        IDeleteItem<Resver_Head>,
        ISubmit<Resver_Head, Resver_Submit_Input_APIItem>
    {
        #region WriteResverHeadLog

        private void WriteResverHeadLog(int headId, int state, DateTime? creDate = null)
        {
            Resver_Head_Log newEntity = new Resver_Head_Log
            {
                RHID = headId,
                Type = state,
                CreUID = GetUid(),
                CreDate = creDate ?? DateTime.Now
            };

            DC.Resver_Head_Log.Add(newEntity);
        }

        #endregion

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
            if (!FilterStaticTools.HasRoleInRequest(Request, AuthorizeBy.Admin))
                input.DeleteFlag = 0;

            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Resver_GetHeadList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.State.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選之預約狀態 ID", nameof(input.State))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head> GetListPagedOrderedQuery(Resver_GetHeadList_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.Resver_Bill)
                .Include(rh => rh.Customer)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(rh => (rh.RHID.ToString().Contains(input.Keyword))
                                          || (rh.Title != null && rh.Title.Contains(input.Keyword)));

            if (input.TargetDate.TryParseDateTime(out DateTime targetDate))
                query = query.Where(rh => DbFunctions.TruncateTime(rh.SDate) == targetDate.Date);

            if (input.CustomerName.HasContent())
                query = query.Where(rh => rh.Customer.TitleC.Contains(input.CustomerName));

            // 刪除的資料 State 不會變，所以要做特別處理
            if (input.State == (int)ReserveHeadGetListState.Deleted)
                query = query.Where(rh => rh.DeleteFlag);
            else if (input.State.IsAboveZero())
                query = query.Where(rh => rh.State == input.State);

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
                CustomerTitle = entity.Customer?.TitleC ?? "",
                CustomerCode = entity.Customer?.Code ?? "",
                PeopleCt = entity.PeopleCt,
                State = GetState(entity)
            });
        }

        private static ReserveHeadGetListState GetState(Resver_Head entity)
        {
            if (entity.DeleteFlag)
                return ReserveHeadGetListState.Deleted;

            return (ReserveHeadGetListState)(ReserveHeadGetListState?)entity.State;
        }

        #endregion

        #region GetInfoById

        private bool CopyMode = false;

        // 確切 route 請參照 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        // 確切 route 請參照 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> Copy(int id)
        {
            CopyMode = true;
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<Resver_Head> GetInfoByIdQuery(int id)
        {
            return DC.Resver_Head
                .Include(rh => rh.Customer)
                .Include(rh => rh.B_StaticCode)
                .Include(rh => rh.BusinessUser)
                .Include(rh => rh.BusinessUser1)
                .Where(rh => rh.RHID == id);
        }

        public async Task<Resver_GetAllInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(Resver_Head entity)
        {
            // 關聯表在這裡分次查詢, 減少 DB 壓力...

            // site
            entity.Resver_Site = await DC.Resver_Site
                .Where(rs => rs.RHID == entity.RHID)
                .Include(rs => rs.B_SiteData)
                .Include(rs => rs.B_OrderCode)
                .Include(rs => rs.B_StaticCode)
                .Include(rs => rs.Resver_Head)
                // site -> throw
                .Include(rs => rs.Resver_Throw)
                .Include(rs => rs.Resver_Throw.Select(rt => rt.B_StaticCode))
                .Include(rs => rs.Resver_Throw.Select(rt => rt.B_OrderCode))
                // site -> throw -> throw_food
                .Include(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food))
                .Include(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.D_FoodCategory)))
                .Include(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_StaticCode)))
                .Include(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.B_Partner)))
                // site -> device
                .Include(rs => rs.Resver_Device)
                .Include(rs => rs.Resver_Device.Select(rd => rd.B_Device))
                .Include(rs => rs.Resver_Device.Select(rd => rd.B_OrderCode))
                .ToArrayAsync();

            // resver_timespan
            entity.M_Resver_TimeSpan = await DC.M_Resver_TimeSpan
                .Where(rts => rts.RHID == entity.RHID)
                .Include(rts => rts.D_TimeSpan)
                .ToArrayAsync();

            // otherItem
            entity.Resver_Other = await DC.Resver_Other
                .Where(ro => ro.RHID == entity.RHID)
                .Include(ro => ro.D_OtherPayItem)
                .Include(ro => ro.D_OtherPayItem.B_StaticCode)
                .Include(ro => ro.D_OtherPayItem.B_OrderCode)
                .ToArrayAsync();

            // bill
            entity.Resver_Bill = await DC.Resver_Bill
                .Where(rb => rb.RHID == entity.RHID)
                .Include(rb => rb.B_Category)
                .Include(rb => rb.D_PayType)
                .ToArrayAsync();

            // GiveBack
            entity.Resver_GiveBack = await DC.Resver_GiveBack
                .Where(rgb => rgb.RHID == entity.RHID)
                .Include(rgb => rgb.B_StaticCode)
                .ToArrayAsync();

            // Questionnaire
            entity.Resver_Questionnaire = await DC.Resver_Questionnaire
                .Where(rq => rq.RHID == entity.RHID)
                .ToArrayAsync();

            (M_Contect contact1, M_Contect contact2) = await GetInfoByIdGetContacts(entity);

            var result = new Resver_GetAllInfoById_Output_APIItem
            {
                State = GetState(entity),
                RHID = entity.RHID,
                BSCID11 = entity.BSCID11,
                BSC11_Title = entity.B_StaticCode?.Title ?? "",
                BSC11_List =
                    await DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.ResverSource,
                        entity.BSCID11),
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                PeopleCt = entity.PeopleCt,
                CID = entity.CID,
                CustomerTitle = entity.Customer.TitleC ?? "",
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
                ContactType1 = contact1?.ContectType,
                ContactData1 = contact1?.ContectData,
                ContactType2 = contact2?.ContectType,
                ContactData2 = contact2?.ContectData,
                Note = entity.Note,
                FixedPrice = entity.FixedPrice,
                QuotedPrice = entity.QuotedPrice,
                MKT = entity.MKT ?? "",
                Owner = entity.Owner ?? "",
                ParkingNote = entity.ParkingNote ?? "",
                SiteItems = GetAllInfoByIdPopulateSiteItems(entity),
                OtherItems = GetAllInfoByIdPopulateOtherItems(entity),
                BillItems = GetAllInfoByIdPopulateBillItems(entity),
                GiveBackItems = GetAllInfoByIdPopulateGiveBackItems(entity),
                QuestionnaireItems = GetAllInfoByIdPopulateQuestionnaireItems(entity)
            };

            // 複製模式的處理：把 ID 都清成 0;

            if (CopyMode)
            {
                GetInfoByIdClearIds(result);
            }

            return await Task.FromResult(result);
        }

        private void GetInfoByIdClearIds(Resver_GetAllInfoById_Output_APIItem result)
        {
            result.RHID = 0;
            result.State = ReserveHeadGetListState.Draft;
            result.SiteItems
                .ForEach(i => i.RSID = 0);
            result.SiteItems
                .SelectMany(si => si.DeviceItems)
                .ForEach(i => i.RDID = 0);
            result.SiteItems
                .SelectMany(si => si.ThrowItems)
                .ForEach(i => i.RTID = 0);
            result.SiteItems
                .SelectMany(si => si.ThrowItems)
                .SelectMany(t => t.FoodItems)
                .ForEach(i => i.RTFID = 0);
            result.QuestionnaireItems.Clear();
            result.BillItems.Clear();
            result.OtherItems
                .ForEach(oi => oi.ROID = 0);
        }

        private static IDictionary<string, object> GetAllInfoByIdPopulateQuestionnaireItems(Resver_Head entity)
        {
            return entity.Resver_Questionnaire
                .GroupBy(q => q.QuestionKey)
                .ToDictionary(grouping => grouping.Key, grouping =>
                {
                    Resver_Questionnaire item = grouping.First();
                    return (object)item.NumberContent ?? item.TextContent;
                });
        }

        private async Task<(M_Contect contact1, M_Contect contact2)> GetInfoByIdGetContacts(Resver_Head entity)
        {
            string tableName = DC.GetTableName<Resver_Head>();

            M_Contect[] contacts = await DC.M_Contect
                .Where(c => c.TargetTable == tableName)
                .Where(c => c.TargetID == entity.RHID)
                .Take(2)
                .ToArrayAsync();

            return (contacts.FirstOrDefault(c => c.SortNo == 1), contacts.FirstOrDefault(c => c.SortNo == 2));
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
                BC_List = Task.Run(() => DC.B_Category.GetCategorySelectable(CategoryType.PayType, rb.BCID))
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
                BSC_List = Task.Run(() => DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.Unit, ro.BSCID))
                    .Result,
                BOCID = ro.BOCID,
                BOC_Code = ro.B_OrderCode?.Code ?? "",
                BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(OrderCodeType.OtherPayItem, ro.BOCID))
                    .Result
                    .Concat(Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(OrderCodeType.General, ro.BOCID))
                        .Result)
                    .ToList(),
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
                    BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(OrderCodeType.Site, rs.BOCID))
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
                            DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.SiteTable, rs.BSCID))
                        .Result,
                    ArriveTimeStart = rs.ArriveTimeStart.ToFormattedStringTime(),
                    ArriveTimeEnd = rs.ArriveTimeEnd.ToFormattedStringTime(),
                    ArriveDescription = rs.ArriveDescription ?? "",
                    TableDescription = rs.TableDescription ?? "",
                    SeatImage = rs.SeatImage != null ? Convert.ToBase64String(rs.SeatImage) : null,
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
                BOC_List = Task.Run(() => DC.B_OrderCode.GetOrderCodeSelectable(OrderCodeType.Device, rd.BOCID))
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
                .Select((rt, index) => new Resver_GetAllInfoById_Output_ThrowItem_APIItem
                {
                    OnlyID = index,
                    RTID = rt.RTID,
                    TargetDate = rt.TargetDate.ToFormattedStringDate(),
                    BSCID = rt.BSCID,
                    BSC_Title = rt.B_StaticCode?.Title ?? "",
                    BSC_List = Task.Run(() =>
                            DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.Unit, rt.BSCID))
                        .Result,
                    Title = rt.Title ?? "",
                    BOCID = rt.BOCID,
                    BOC_Title = rt.B_OrderCode?.Title ?? "",
                    BOC_List = Task.Run(() =>
                            DC.B_OrderCode.GetOrderCodeSelectable(OrderCodeType.Throw, rt.BOCID))
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
                        : GetALlInfoByIdPopulateFoodItems(rt, index)
                })
                .OrderBy(rt => rt.SortNo)
                .ToList();
        }

        private List<Resver_GetAllInfoById_Output_FoodItem_APIItem> GetALlInfoByIdPopulateFoodItems(Resver_Throw rt,
            int parentId)
        {
            return rt.Resver_Throw_Food
                .Select(rtf => new Resver_GetAllInfoById_Output_FoodItem_APIItem
                {
                    ParentID = parentId,
                    RTFID = rtf.RTFID,
                    DFCID = rtf.DFCID,
                    DFC_Title = rtf.D_FoodCategory?.Title,
                    DFC_List = Task.Run(() => DC.D_FoodCategory.GetFoodCategorySelectable(rtf.DFCID)).Result,
                    BSCID = rtf.BSCID,
                    BSC_Title = rtf.B_StaticCode?.Title,
                    BSC_List = Task.Run(() =>
                            DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.Cuisine, rtf.BSCID))
                        .Result,
                    BPID = rtf.BPID,
                    BP_Title = rtf.B_Partner?.Title,
                    BP_List = Task.Run(() => DC.B_Partner.GetPartnerSelectable(rtf.BPID)).Result,
                    Ct = rtf.Ct,
                    UnitPrice = rtf.UnitPrice,
                    Price = rtf.Price,
                    ArriveTime = rtf.ArriveTime.ToFormattedStringTime()
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
            // 非管理員只允許刪除

            bool isValid = input.Items.StartValidateElements()
                .Validate(i => i.DeleteFlag == true || FilterStaticTools.HasRoleInRequest(Request, AuthorizeBy.Admin),
                    () => AddError(NoPrivilege()))
                .IsValid();

            if (!isValid)
                return GetResponseJson();

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

            ReserveHeadGetListState? state = (ReserveHeadGetListState?)entity.State;
            if (state == ReserveHeadGetListState.CheckedIn ||
                state == ReserveHeadGetListState.Terminated ||
                state == ReserveHeadGetListState.FullyPaid)
            {
                AddError(1, "已報到、已結帳或已中止的預約單無法修改確認狀態！");
                return GetResponseJson();
            }

            // 3. 修改 DB
            WriteResverHeadLog(entity.RHID, ReserveHeadGetListState.Checked);
            entity.State = (int)ReserveHeadGetListState.Checked;
            entity.CheckFlag = checkFlag ?? throw new ArgumentNullException(nameof(checkFlag));
            await ChangeCheckUpdateDb();

            // 4. 回傳
            return GetResponseJson();
        }

        private void WriteResverHeadLog(int entityRhid, ReserveHeadGetListState state)
        {
            WriteResverHeadLog(entityRhid, (int)state);
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
                // 已結帳或已中止時，不允許修改確認狀態。
                .Where(rh => !rh.DeleteFlag && rh.RHID == id)
                .FirstOrDefaultAsync();
        }

        private bool ChangeCheckValidateInput(int? id, bool? checkFlag)
        {
            bool isValid = this.StartValidate()
                .Validate(_ => id > 0, () => AddError(EmptyNotAllowed("欲更新的預約 ID", nameof(id))))
                .Validate(_ => checkFlag != null, () => AddError(EmptyNotAllowed("確認狀態", nameof(checkFlag))))
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

            if (entity.State == (int)ReserveHeadGetListState.Terminated)
            {
                AddError(1, "已中止的預約單無法修改報到狀態！");
                return GetResponseJson();
            }

            // 3. 修改 DB
            WriteResverHeadLog(entity.RHID, ReserveHeadGetListState.CheckedIn);
            entity.CheckInFlag = checkInFlag ?? throw new ArgumentNullException(nameof(checkInFlag));
            entity.State = (int)ReserveHeadGetListState.CheckedIn;
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
                // 已中止時，不允許修改報到狀態。
                .Where(rh => !rh.DeleteFlag && rh.RHID == id)
                .FirstOrDefaultAsync();
        }

        private bool ChangeCheckInValidateInput(int? id, bool? checkInFlag)
        {
            bool isValid = this.StartValidate()
                .Validate(_ => id > 0, () => AddError(EmptyNotAllowed("欲更新的預約 ID", nameof(id))))
                .Validate(_ => checkInFlag != null, () => AddError(EmptyNotAllowed("報到狀態", nameof(checkInFlag))))
                .Validate(_ => checkInFlag != false,
                    () => AddError(NotSupportedValue("報到狀態", nameof(checkInFlag), "不支援取消報到！")))
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

        public async Task<Resver_Head> SubmitCreateData(Resver_Submit_Input_APIItem input)
        {
            return await _SubmitCreateData(input);
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