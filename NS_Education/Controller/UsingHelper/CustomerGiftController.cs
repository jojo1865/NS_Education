using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.CustomerGift.GetInfoById;
using NS_Education.Models.APIItems.Controller.CustomerGift.GetList;
using NS_Education.Models.APIItems.Controller.CustomerGift.Submit;
using NS_Education.Models.Entities;
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
    public class CustomerGiftController : PublicClass,
        IGetListPaged<M_Customer_Gift, CustomerGift_GetList_Input_APIItem, CustomerGift_GetList_Output_Row_APIItem>,
        IGetInfoById<GiftSending, CustomerGift_GetInfoById_Output_APIItem>,
        IDeleteItem<M_Customer_Gift>,
        ISubmit<GiftSending, CustomerGift_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<CustomerGift_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<CustomerGift_Submit_Input_APIItem> _submitHelper;

        public CustomerGiftController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CustomerGiftController, M_Customer_Gift, CustomerGift_GetList_Input_APIItem,
                    CustomerGift_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<CustomerGiftController, GiftSending, CustomerGift_GetInfoById_Output_APIItem>(
                    this);

            _deleteItemHelper =
                new DeleteItemHelper<CustomerGiftController, M_Customer_Gift>(this);

            _submitHelper =
                new SubmitHelper<CustomerGiftController, GiftSending, CustomerGift_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CustomerGift_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(CustomerGift_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate(true)
                .Validate(i => i.SendYear == 0 || i.SendYear.IsInBetween(1911, 9999),
                    () => AddError(OutOfRange("贈送年分", 1911, 9999)))
                .Validate(i =>
                        !i.SDate.TryParseDateTime(out DateTime startDate)
                        || !i.EDate.TryParseDateTime(out DateTime endDate)
                        || endDate >= startDate
                    , () => AddError(MinLargerThanMax("贈送日期起始日", "贈送日期結束日")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<M_Customer_Gift> GetListPagedOrderedQuery(CustomerGift_GetList_Input_APIItem input)
        {
            var query = DC.M_Customer_Gift
                .Include(mcg => mcg.Customer)
                .Include(mcg => mcg.GiftSending)
                .Include(mcg => mcg.GiftSending.B_StaticCode)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(mcg => mcg.GiftSending.B_StaticCode.Title.Contains(input.Keyword));

            if (input.CustomerTitleC.HasContent())
                query = query.Where(mcg => mcg.Customer.TitleC.Contains(input.CustomerTitleC));

            if (input.SendYear.IsAboveZero())
                query = query.Where(mcg => mcg.GiftSending.Year == input.SendYear);

            if (input.SDate.TryParseDateTime(out DateTime startDate))
                query = query.Where(mcg => DbFunctions.TruncateTime(mcg.GiftSending.SendDate) >= startDate.Date);

            if (input.EDate.TryParseDateTime(out DateTime endDate))
                query = query.Where(mcg => DbFunctions.TruncateTime(mcg.GiftSending.SendDate) <= endDate.Date);

            return query.OrderByDescending(mcg => mcg.GiftSending.SendDate)
                .ThenBy(mcg => mcg.CID)
                .ThenBy(mcg => mcg.GSID);
        }

        public async Task<CustomerGift_GetList_Output_Row_APIItem> GetListPagedEntityToRow(M_Customer_Gift entity)
        {
            return await Task.FromResult(new CustomerGift_GetList_Output_Row_APIItem
            {
                GSID = entity.GSID,
                MID = entity.MID,
                CID = entity.CID,
                C_Code = entity.Customer?.Code ?? "",
                C_TitleC = entity.Customer?.TitleC ?? "",
                C_TitleE = entity.Customer?.TitleE ?? "",
                Year = entity.GiftSending.Year,
                SendDate = entity.GiftSending.SendDate.ToFormattedStringDate(),
                BSCID = entity.GiftSending.BSCID,
                BSC_Code = entity.GiftSending.B_StaticCode?.Code ?? "",
                BSC_Title = entity.GiftSending.B_StaticCode?.Title ?? "",
                Title = entity.GiftSending.B_StaticCode?.Title ?? "",
                Ct = entity.Ct,
                Note = entity.Note ?? ""
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<GiftSending> GetInfoByIdQuery(int id)
        {
            return DC.GiftSending
                .Include(gs => gs.B_StaticCode)
                .Include(gs => gs.M_Customer_Gift)
                .Include(gs => gs.M_Customer_Gift.Select(mcg => mcg.Customer))
                .Where(gs => gs.GSID == id);
        }

        public async Task<CustomerGift_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            GiftSending entity)
        {
            return await Task.FromResult(new CustomerGift_GetInfoById_Output_APIItem
            {
                GSID = entity.GSID,
                Year = entity.Year,
                SendDate = entity.SendDate.ToFormattedStringDate(),
                BSCID = entity.BSCID,
                BSC_Code = entity.B_StaticCode?.Code ?? "",
                BSC_Title = entity.B_StaticCode?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode?.CodeType,
                    entity.BSCID),
                Title = entity.B_StaticCode?.Title ?? "",
                Note = entity.Note ?? "",
                Customers = entity.M_Customer_Gift.Select(mcg => new CustomerGift_GetInfoById_Customers_Row_APIItem
                {
                    MID = mcg.MID,
                    CID = mcg.CID,
                    C_Code = mcg.Customer?.Code ?? "",
                    C_Title = mcg.Customer?.TitleC ?? mcg.Customer?.TitleE ?? "",
                    Ct = mcg.Ct,
                    Note = mcg.Note ?? ""
                }).ToList()
            });
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<M_Customer_Gift> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.M_Customer_Gift.Where(mcg => ids.Contains(mcg.MID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null,
            nameof(CustomerGift_Submit_Input_APIItem.GSID))]
        public async Task<string> Submit(CustomerGift_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(CustomerGift_Submit_Input_APIItem input)
        {
            return input.GSID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(CustomerGift_Submit_Input_APIItem input)
        {
            input.SendDate.TryParseDateTime(out DateTime sendDate);
            sendDate = sendDate.Date;

            bool isValid = await input.StartValidate()
                .Validate(i => i.GSID == 0, () => AddError(WrongFormat("禮品贈與紀錄 ID")))
                .Validate(i => i.Year.IsInBetween(1911, 9999), () => AddError(WrongFormat("禮品贈送代表年分")))
                .Validate(i => i.SendDate.TryParseDateTime(out _), () => AddError(WrongFormat("禮品贈與日期")))
                .ValidateAsync(async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Gift),
                    () => AddError(NotFound("禮品 ID")))
                .Validate(i => i.Customers.Any(), () => AddError(EmptyNotAllowed("此紀錄之對應客戶")))
                .IsValid();


            // 驗證每個 CID 都是獨特的
            isValid = isValid && input.Customers.Select(i => i.CID).Distinct().Count() == input.Customers.Count;

            // 驗證每筆客戶輸入資料正確
            isValid = isValid && await input.Customers.StartValidateElements()
                .ValidateAsync(async i => await DC.Customer.ValidateIdExists(i.CID, nameof(Customer.CID)),
                    i => AddError(NotFound($"客戶 ID（{i.CID}）")))
                .Validate(i => i.Ct.IsAboveZero(),
                    i => AddError(OutOfRange($"贈送數量（客戶 ID {i.CID}）", 0)))
                .IsValid();

            // 驗證沒有其他贈送年份、贈與日期、禮品 ID 相同的資料
            bool isUnique = isValid && !await DC.GiftSending.AnyAsync(gs => !gs.DeleteFlag
                                                                            && gs.GSID != input.GSID
                                                                            && gs.Year == input.Year
                                                                            && DbFunctions.TruncateTime(gs.SendDate) ==
                                                                            sendDate
                                                                            && gs.BSCID == input.BSCID);

            if (isValid && !isUnique)
                AddError(AlreadyExists("贈送年份、贈與日期、禮品 ID"));

            return isValid && isUnique;
        }

        public async Task<GiftSending> SubmitCreateData(CustomerGift_Submit_Input_APIItem input)
        {
            input.SendDate.TryParseDateTime(out DateTime sendDate);
            sendDate = sendDate.Date;

            GiftSending newGiftSending = new GiftSending
            {
                Year = input.Year,
                Title = await DC.B_StaticCode
                    .Where(bsc => bsc.BSCID == input.BSCID)
                    .Select(bsc => bsc.Title)
                    .FirstOrDefaultAsync() ?? "",
                SendDate = sendDate,
                BSCID = input.BSCID,
                Note = input.Note,
                M_Customer_Gift = input.Customers.Select(i => new M_Customer_Gift
                {
                    CID = i.CID,
                    CVID = null,
                    Ct = i.Ct,
                    Note = i.Note
                }).ToList()
            };

            return await Task.FromResult(newGiftSending);
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(CustomerGift_Submit_Input_APIItem input)
        {
            input.SendDate.TryParseDateTime(out DateTime sendDate);
            sendDate = sendDate.Date;

            bool isValid = await input.StartValidate()
                .Validate(i => i.GSID.IsAboveZero(), () => AddError(EmptyNotAllowed("禮品贈與紀錄 ID")))
                .Validate(i => i.Year.IsInBetween(1911, 9999), () => AddError(WrongFormat("禮品贈送代表年分")))
                .Validate(i => i.SendDate.TryParseDateTime(out _), () => AddError(WrongFormat("禮品贈與日期")))
                .ValidateAsync(async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Gift),
                    () => AddError(NotFound("禮品 ID")))
                .Validate(i => i.Customers.Any(), () => AddError(EmptyNotAllowed("此紀錄之對應客戶")))
                .IsValid();


            // 驗證每個 CID 都是獨特的
            isValid = isValid && input.Customers.Select(i => i.CID).Distinct().Count() == input.Customers.Count;

            // 驗證每筆客戶輸入資料正確
            isValid = isValid && await input.Customers.StartValidateElements()
                .ValidateAsync(async i => await DC.Customer.ValidateIdExists(i.CID, nameof(Customer.CID)),
                    i => AddError(NotFound($"客戶 ID（{i.CID}）")))
                .Validate(i => i.Ct.IsAboveZero(),
                    i => AddError(OutOfRange($"贈送數量（客戶 ID {i.CID}）", 0)))
                .IsValid();

            // 驗證沒有其他贈送年份、贈與日期、禮品 ID 相同的資料
            bool isUnique = isValid && !await DC.GiftSending.AnyAsync(gs => !gs.DeleteFlag
                                                                            && gs.GSID != input.GSID
                                                                            && gs.Year == input.Year
                                                                            && DbFunctions.TruncateTime(gs.SendDate) ==
                                                                            sendDate
                                                                            && gs.BSCID == input.BSCID);

            if (isValid && !isUnique)
                AddError(AlreadyExists("贈送年份、贈與日期、禮品 ID"));

            return isValid && isUnique;
        }

        public IQueryable<GiftSending> SubmitEditQuery(CustomerGift_Submit_Input_APIItem input)
        {
            return DC.GiftSending
                .Include(gs => gs.M_Customer_Gift)
                .Where(gs => gs.GSID == input.GSID);
        }

        public void SubmitEditUpdateDataFields(GiftSending data, CustomerGift_Submit_Input_APIItem input)
        {
            // 刪除輸入中沒有的舊資料
            Dictionary<int, CustomerGift_Submit_Customers_Row_APIItem> customerIdToInput =
                input.Customers.ToDictionary(c => c.CID, c => c);

            IEnumerable<int> inputCustomerIds = customerIdToInput.Keys.AsEnumerable();
            M_Customer_Gift[] toUpdate =
                data.M_Customer_Gift.Where(mcg => inputCustomerIds.Contains(mcg.CID)).ToArray();
            DC.M_Customer_Gift.RemoveRange(data.M_Customer_Gift.Except(toUpdate));

            input.SendDate.TryParseDateTime(out DateTime sendDate);

            data.Year = input.Year;
            data.Title = DC.B_StaticCode
                .Where(bsc => bsc.BSCID == input.BSCID)
                .Select(bsc => bsc.Title)
                .FirstOrDefault() ?? "";
            data.SendDate = sendDate;
            data.BSCID = input.BSCID;
            data.Note = input.Note ?? data.Note;

            // 更新舊 M_Customer_Gift
            foreach (M_Customer_Gift mcg in toUpdate)
            {
                mcg.Ct = customerIdToInput[mcg.CID].Ct;
                mcg.Note = customerIdToInput[mcg.CID].Note;

                customerIdToInput.Remove(mcg.CID);
            }

            // 新增新 M_Customer_Gift
            foreach (CustomerGift_Submit_Customers_Row_APIItem c in customerIdToInput.Values)
            {
                data.M_Customer_Gift.Add(new M_Customer_Gift
                {
                    CID = c.CID,
                    GSID = data.GSID,
                    CVID = null,
                    Ct = c.Ct,
                    Note = c.Note
                });
            }
        }

        #endregion

        #endregion
    }
}