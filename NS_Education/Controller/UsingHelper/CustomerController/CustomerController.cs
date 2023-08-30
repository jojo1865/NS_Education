using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Customer.GetInfoById;
using NS_Education.Models.APIItems.Controller.Customer.GetList;
using NS_Education.Models.APIItems.Controller.Customer.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.CustomerController
{
    public class CustomerController : PublicClass,
        IGetInfoById<Customer, Customer_GetInfoById_Output_APIItem>,
        IChangeActive<Customer>,
        IDeleteItem<Customer>,
        ISubmit<Customer, Customer_Submit_Input_APIItem>,
        IDeleteItemValidateReservation<Resver_Head>
    {
        #region 通用

        public async Task<IEnumerable<M_Address>> GetAddresses(int customerId)
        {
            string targetTableName = DC.GetTableName<Customer>();

            M_Address[] addresses = await DC.M_Address
                .Where(a => a.TargetTable == targetTableName)
                .Where(a => a.TargetID == customerId)
                .ToArrayAsync();

            return addresses;
        }

        #endregion

        #region Initialization

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<Customer_Submit_Input_APIItem> _submitHelper;

        public CustomerController()
        {
            _getInfoByIdHelper =
                new GetInfoByIdHelper<CustomerController, Customer, Customer_GetInfoById_Output_APIItem>(this);
            _changeActiveHelper =
                new ChangeActiveHelper<CustomerController, Customer>(this);
            _deleteItemHelper =
                new DeleteItemHelper<CustomerController, Customer>(this);
            _submitHelper =
                new SubmitHelper<CustomerController, Customer, Customer_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Customer_GetList_Input_APIItem input)
        {
            // 這個端點需要用 M_Contect，因此無法用 helper

            // 1. 輸入驗證
            if (!await GetListPagedValidateInput(input))
                return GetResponseJson();

            // 2. 查資料
            IOrderedQueryable<Customer> query = GetListPagedOrderedQuery(input);

            // join M_Contect
            string targetTableName = DC.GetTableName<Customer>();
            var joinedQuery = query.GroupJoin(DC.M_Contect.Where(mc => mc.TargetTable == targetTableName),
                c => c.CID,
                mc => mc.TargetID,
                (c, mc) => new { Customer = c, Contacts = mc }
            );

            if (input.ContactData.HasContent())
                joinedQuery = joinedQuery.Where(j => j.Contacts.Any(c => c.ContectData.Contains(input.ContactData)));

            var response = new CommonResponseForPagedList<Customer_GetList_Output_Row_APIItem>();
            response.SetByInput(input);
            response.AllItemCt = await joinedQuery.CountAsync();

            var queryResult = await joinedQuery
                .ToArrayAsync();

            var list = new List<Customer_GetList_Output_Row_APIItem>();

            int index = 0;
            foreach (var result in queryResult)
            {
                Customer_GetList_Output_Row_APIItem row =
                    await GetListPagedEntityToRow(result.Customer, result.Contacts);
                row.SetIndex(index++);
                await row.SetInfoFromEntity(result.Customer, this);

                if (input.ReverseOrder)
                    list.Insert(0, row);
                else
                    list.Add(row);
            }

            (int skip, int take) = input.CalculateSkipAndTake(response.AllItemCt);

            response.Items = list
                .AsEnumerable()
                .SortWithInput(input.Sorting)
                .Skip(skip)
                .Take(take)
                .ToList();

            return GetResponseJson(response);
        }

        public async Task<bool> GetListPagedValidateInput(Customer_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BSCID6.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選的行業別", nameof(Customer_GetList_Input_APIItem.BSCID6))))
                .Validate(i => i.BSCID4.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選的區域別", nameof(Customer_GetList_Input_APIItem.BSCID4))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Customer> GetListPagedOrderedQuery(Customer_GetList_Input_APIItem input)
        {
            var query = DC.Customer
                .Include(c => c.Resver_Head)
                .Include(c => c.B_StaticCode)
                .Include(c => c.B_StaticCode1)
                .Include(c => c.CustomerVisit)
                .Include(c => c.CustomerQuestion)
                .Include(c => c.M_Customer_Gift)
                .Include(c => c.M_Customer_Gift.Select(mcg => mcg.CustomerVisit))
                .Include(c => c.M_Customer_BusinessUser)
                .Include(c => c.M_Customer_BusinessUser.Select(cbu => cbu.BusinessUser))
                .AsQueryable();

            if (input.ActiveFlag.IsInBetween(0, 1))
                query = query.Where(c => c.ActiveFlag == (input.ActiveFlag == 1));

            query = query.Where(c => c.DeleteFlag == (input.DeleteFlag == 1));

            if (input.Keyword.HasContent())
                query = query.Where(c =>
                    c.TitleC.Contains(input.Keyword)
                    || c.TitleE.Contains(input.Keyword)
                    || c.Code.Contains(input.Keyword));

            if (input.ContactName.HasContent())
                query = query.Where(c => c.ContectName.Contains(input.ContactName));

            if (input.BSCID6.IsAboveZero())
                query = query.Where(c => c.BSCID6 == input.BSCID6);

            if (input.BSCID4.IsAboveZero())
                query = query.Where(c => c.BSCID4 == input.BSCID4);

            if (input.BUID.IsAboveZero())
                query = query.Where(c => c.M_Customer_BusinessUser.Any(cbu => cbu.BUID == input.BUID));

            // ResverType 為 0 時，只找沒有任何預約紀錄的客戶
            // ResverType 為 1 時，只找有預約過的客戶
            if (input.ResverType.IsInBetween(0, 1))
                query = query.Where(c =>
                    c.Resver_Head.Any(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft) ==
                    (input.ResverType == 1));

            return query.OrderBy(c => c.CID);
        }

        public async Task<Customer_GetList_Output_Row_APIItem> GetListPagedEntityToRow(Customer entity,
            IEnumerable<M_Contect> contacts)
        {
            return await Task.FromResult(new Customer_GetList_Output_Row_APIItem
            {
                CID = entity.CID,
                BSCID6 = entity.BSCID6,
                BSC6_Title = entity.B_StaticCode1?.Title ?? "",
                BSCID4 = entity.BSCID4,
                BSC4_Title = entity.B_StaticCode?.Title ?? "",
                Code = entity.Code ?? "",
                Compilation = entity.Compilation ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                Email = entity.Email ?? "",
                InvoiceTitle = entity.InvoiceTitle ?? "",
                ContactName = entity.ContectName ?? "",
                Website = entity.Website ?? "",
                Note = entity.Note ?? "",
                BillFlag = entity.BillFlag,
                InFlag = entity.InFlag,
                PotentialFlag = entity.PotentialFlag,
                ResverCt = entity.GetDealtReservationCount(),
                VisitCt = entity.CustomerVisit.Count(cv => !cv.DeleteFlag),
                QuestionCt = entity.CustomerQuestion.Count(cq => !cq.DeleteFlag),
                GiftCt = entity.M_Customer_Gift.Count(cg => !cg.GiftSending.DeleteFlag),
                BusinessUsers = GetBusinessUserListFromEntity(entity),
                Contacts = contacts
                    .Select(c => new Customer_GetList_Contact_APIItem
                    {
                        ContactType = ContactTypeController.GetContactTypeTitle(c.ContectType) ?? "",
                        ContactData = c.ContectData
                    }).ToList()
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

        public IQueryable<Customer> GetInfoByIdQuery(int id)
        {
            return DC.Customer
                .Include(c => c.B_StaticCode)
                .Include(c => c.B_StaticCode1)
                .Include(c => c.CustomerVisit)
                .Include(c => c.CustomerQuestion)
                .Include(c => c.M_Customer_Gift)
                .Include(c => c.M_Customer_Gift.Select(mcg => mcg.CustomerVisit))
                .Where(c => c.CID == id);
        }

        public async Task<Customer_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(Customer entity)
        {
            M_Address address = (await GetAddresses(entity.CID)).FirstOrDefault();

            string tableName = DC.GetTableName<Customer>();
            var contacts = DC.M_Contect
                .Where(c => c.TargetTable == tableName)
                .Where(c => c.TargetID == entity.CID);
            M_Contect contact1 = contacts.FirstOrDefault(c => c.SortNo == 1);
            M_Contect contact2 = contacts.FirstOrDefault(c => c.SortNo == 2);

            // M_Contect 有可能會有 0, 1, 2 筆或以上, 最多只處理兩筆

            return await Task.FromResult(new Customer_GetInfoById_Output_APIItem
            {
                CID = entity.CID,
                BSCID6 = entity.BSCID6,
                BSC6_Title = entity.B_StaticCode1?.Title ?? "",
                BSC6_List = await DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.Industry,
                    entity.BSCID6),
                BSCID4 = entity.BSCID4,
                BSC4_Title = entity.B_StaticCode?.Title ?? "",
                BSC4_List = await DC.B_StaticCode.GetStaticCodeSelectable(StaticCodeType.Region,
                    entity.BSCID4),
                Code = entity.Code ?? "",
                Compilation = entity.Compilation ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                DZID = address?.DZID ?? 0,
                Address = address?.Address ?? "",
                Email = entity.Email ?? "",
                InvoiceTitle = entity.InvoiceTitle ?? "",
                ContactName = entity.ContectName ?? "",
                ContactType1 = contact1?.ContectType ?? -1,
                ContactData1 = contact1?.ContectData ?? "",
                ContactType2 = contact2?.ContectType ?? -1,
                ContactData2 = contact2?.ContectData ?? "",
                Website = entity.Website ?? "",
                Note = entity.Note ?? "",
                BillFlag = entity.BillFlag,
                InFlag = entity.InFlag,
                PotentialFlag = entity.PotentialFlag,
                ResverCt = entity.GetDealtReservationCount(),
                VisitCt = entity.CustomerVisit.Count(cv => !cv.DeleteFlag),
                QuestionCt = entity.CustomerQuestion.Count(cq => !cq.DeleteFlag),
                GiftCt = entity.M_Customer_Gift.Count(cg => !cg.GiftSending.DeleteFlag),
                Items = GetBusinessUserListFromEntity(entity)
            });
        }

        private List<Customer_GetList_BusinessUser_APIItem> GetBusinessUserListFromEntity(Customer entity)
        {
            List<Customer_GetList_BusinessUser_APIItem> result = new List<Customer_GetList_BusinessUser_APIItem>();

            foreach (M_Customer_BusinessUser cbu in entity.M_Customer_BusinessUser
                         .Where(cbu =>
                             cbu.ActiveFlag && !cbu.DeleteFlag && cbu.BusinessUser.ActiveFlag &&
                             !cbu.BusinessUser.DeleteFlag))
            {
                // 取得 M_Contact, 否則用預設的聯絡方式
                string targetTableName = DC.GetTableName<M_Customer_BusinessUser>();
                M_Contect contact = DC.M_Contect
                    .Where(c => c.TargetTable == targetTableName)
                    .FirstOrDefault(c => c.TargetID == cbu.MID);

                Customer_GetList_BusinessUser_APIItem newItem = new Customer_GetList_BusinessUser_APIItem
                {
                    BUID = cbu.BUID,
                    Name = cbu.BusinessUser?.Name ?? "",
                    ContactType = contact?.ContectType ?? (int)ContactType.Phone,
                    // ContectData 是 string, 有可能有 contact 但 contactData 卻是 null, 所以這裡不能用 elvis
                    ContactData = contact != null ? contact.ContectData : cbu.BusinessUser?.Phone ?? "",
                    MKSalesFlag = cbu.MappingType == DbConstants.MkSalesMappingType ||
                                  cbu.MappingType == DbConstants.OpAndMkSalesMappingType,
                    OPSalesFlag = cbu.MappingType == DbConstants.OpSalesMappingType ||
                                  cbu.MappingType == DbConstants.OpAndMkSalesMappingType,
                };

                result.Add(newItem);
            }

            return result;
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            if (activeFlag == false && !await ChangeActiveValidateReservation(id))
                return GetResponseJson();

            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        private async Task<bool> ChangeActiveValidateReservation(int id)
        {
            int[] cantDeleteData = await DC.Resver_Head
                .Where(ResverHeadExpression.IsOngoingExpression)
                .Where(rh => rh.CID == id)
                .Select(rh => rh.RHID)
                .Distinct()
                .ToArrayAsync();

            foreach (int headId in cantDeleteData)
            {
                AddError(NotSupportedValue("欲停用的 ID", nameof(id), $"已有進行中的預約單（單號 {headId}）"));
            }

            return !HasError();
        }

        public IQueryable<Customer> ChangeActiveQuery(int id)
        {
            return DC.Customer.Where(c => c.CID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            if (!await DeleteItemValidateNoSameCodeExistingForRevive(input))
                return GetResponseJson();

            if (!await _deleteItemHelper.DeleteItemValidateReservation(input, this))
                return GetResponseJson();

            return await _deleteItemHelper.DeleteItem(input);
        }

        /// <inheritdoc />
        public IQueryable<Resver_Head> SupplyQueryWithInputIdCondition(IQueryable<Resver_Head> basicQuery,
            HashSet<int> uniqueDeleteId)
        {
            return basicQuery
                .Where(rh => uniqueDeleteId.Contains(rh.CID));
        }

        /// <inheritdoc />
        public object GetInputId(Resver_Head cantDelete)
        {
            return cantDelete.CID;
        }

        /// <inheritdoc />
        public int GetHeadId(Resver_Head cantDelete)
        {
            return cantDelete.RHID;
        }

        private async Task<bool> DeleteItemValidateNoSameCodeExistingForRevive(DeleteItem_Input_APIItem input)
        {
            // 針對復活的資料，檢查是否有任何同樣代號的資料正處於非刪除狀態
            // 如果有，不允許復活

            IEnumerable<int?> reviveIds = input.Items
                .Where(i => i.DeleteFlag == false)
                .Select(i => i.Id)
                .Distinct();

            Dictionary<string, int> reviveData = await DC.Customer
                .Where(c => c.DeleteFlag)
                .Where(c => reviveIds.Contains(c.CID))
                .ToDictionaryAsync(c => c.Code, c => c.CID);

            HashSet<string> reviveCodes = reviveData.Keys.ToHashSet();

            Customer[] duplicateCodeData = await DC.Customer
                .Where(c => !c.DeleteFlag && reviveCodes.Contains(c.Code))
                .ToArrayAsync();

            foreach (Customer dupe in duplicateCodeData)
            {
                AddError(AlreadyExists($"客戶代號（{dupe.Code}）", nameof(Customer.Code)));
            }

            return !HasError();
        }

        public IQueryable<Customer> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.Customer.Where(c => ids.Contains(c.CID));
        }

        #endregion

        #region Submit

        private const string AddressTitle = "客戶地址";

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Customer_Submit_Input_APIItem.CID))]
        public async Task<string> Submit(Customer_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Customer_Submit_Input_APIItem input)
        {
            return input.CID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Customer_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                // 驗證輸入
                .Validate(i => i.CID == 0,
                    () => AddError(WrongFormat("客戶 ID", nameof(Customer_Submit_Input_APIItem.CID))))
                .Validate(i => i.Code.HasLengthBetween(1, 10),
                    () => AddError(LengthOutOfRange("客戶代號", nameof(Customer_Submit_Input_APIItem.Code), 1, 10)))
                .ValidateAsync(async i => !await DC.Customer.AnyAsync(c => c.Code == i.Code && !c.DeleteFlag),
                    () => AddError(AlreadyExists("客戶代號", nameof(input.Code))))
                .Validate(i => i.Compilation.IsNullOrWhiteSpace() || i.Compilation.Length == 8,
                    () => AddError(WrongFormat("統一編號", nameof(Customer_Submit_Input_APIItem.Compilation))))
                .Validate(i => i.TitleC.HasContent(),
                    () => AddError(EmptyNotAllowed("客戶名稱（中文）", nameof(Customer_Submit_Input_APIItem.TitleC))))
                .Validate(i => i.TitleC.HasLengthBetween(1, 50),
                    () => AddError(LengthOutOfRange("客戶名稱（中文）", nameof(Customer_Submit_Input_APIItem.TitleC), 1, 50)))
                .Validate(i => i.TitleE.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("客戶名稱（英文）", nameof(Customer_Submit_Input_APIItem.TitleE), 0, 100)))
                .Validate(i => i.Address.HasLengthBetween(0, 200),
                    () => AddError(LengthOutOfRange("地址", nameof(Customer_Submit_Input_APIItem.Address), 0, 200)))
                .Validate(i => i.Email.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("Email", nameof(Customer_Submit_Input_APIItem.Email), 0, 100)))
                .Validate(i => i.InvoiceTitle.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("發票抬頭", nameof(Customer_Submit_Input_APIItem.InvoiceTitle), 0, 50)))
                .Validate(i => i.ContactName.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("聯絡人名稱", nameof(Customer_Submit_Input_APIItem.ContactName), 0, 50)))
                .Validate(i => i.ContactData1.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("聯絡方式 1 的內容", nameof(Customer_Submit_Input_APIItem.ContactData1), 0,
                        30)))
                .Validate(i => i.ContactData2.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("聯絡方式 2 的內容", nameof(Customer_Submit_Input_APIItem.ContactData2), 0,
                        30)))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID6, StaticCodeType.Industry),
                    () => AddError(NotFound("行業別 ID", nameof(input.BSCID6))))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID4, StaticCodeType.Region),
                    () => AddError(NotFound("區域別 ID", nameof(input.BSCID4))))
                .Validate(i => i.DZID.IsZeroOrAbove(),
                    () => AddError(WrongFormat("國籍與郵遞區號 ID", nameof(Customer_Submit_Input_APIItem.DZID))))
                .ForceSkipIf(i => i.ContactType1 == -1)
                .Validate(i => i.ContactType1.IsInBetween(0, 3),
                    () => AddError(NotSupportedValue("聯絡方式 1 的類型", nameof(input.ContactType1), null)))
                .Validate(i => i.ContactData1.HasContent(),
                    () => AddError(EmptyNotAllowed("聯絡方式 1 的內容", nameof(Customer_Submit_Input_APIItem.ContactData1))))
                .StopForceSkipping()
                .ForceSkipIf(i => i.ContactType2 == -1)
                .Validate(i => i.ContactType2.IsInBetween(0, 3),
                    () => AddError(NotSupportedValue("聯絡方式 2 的類型", nameof(input.ContactType2), null)))
                .Validate(i => i.ContactData2.HasContent(),
                    () => AddError(EmptyNotAllowed("聯絡方式 2 的內容", nameof(Customer_Submit_Input_APIItem.ContactData2))))
                .StopForceSkipping()
                .ForceSkipIf(i => i.DZID <= 0)
                .ValidateAsync(async i => await DC.D_Zip.ValidateIdExists(i.DZID, nameof(D_Zip.DZID)),
                    () => AddError(NotFound("國籍與郵遞區號 ID", nameof(input.DZID))))
                .StopForceSkipping()
                .IsValid();

            isValid = isValid &&
                      // 當前面輸入都正確時，繼續驗證所有 BUID 都是實際存在的 BU 資料
                      await input.Items.StartValidateElements()
                          .SkipIfAlreadyInvalid()
                          .Validate(i => i.BUID.IsAboveZero(),
                              () => AddError(EmptyNotAllowed("業務負責人", nameof(Customer_Submit_BUID_APIItem.BUID))))
                          .ValidateAsync(
                              async i => await DC.BusinessUser.ValidateIdExists(i.BUID, nameof(BusinessUser.BUID)),
                              () => AddError(NotFound("業務負責人", nameof(Customer_Submit_BUID_APIItem.BUID))))
                          .IsValid();

            // 驗證業務如果有輸入聯絡方式時，輸入欄位格式正確
            bool isBusinessUserContactValid = input.Items.StartValidateElements()
                .Validate(i => i.Phone.HasLengthBetween(0, 50),
                    i => AddError(LengthOutOfRange($"業務負責人（ID {i.BUID}）的聯絡電話", nameof(i.Phone), 0, 50)))
                .Validate(i => i.Phone == null || i.Phone.All(Char.IsDigit),
                    i => AddError(WrongFormat($"業務負責人（ID {i.BUID}）的聯絡電話", nameof(i.Phone))))
                .IsValid();
            return await Task.FromResult(isValid && isBusinessUserContactValid);
        }

        public async Task<Customer> SubmitCreateData(Customer_Submit_Input_APIItem input)
        {
            Customer newData = new Customer
            {
                BSCID6 = input.BSCID6,
                BSCID4 = input.BSCID4,
                Code = input.Code,
                Compilation = input.Compilation,
                TitleC = input.TitleC,
                TitleE = input.TitleE,
                Email = input.Email,
                InvoiceTitle = input.InvoiceTitle,
                ContectName = input.ContactName,
                ContectPhone = input.ContactType1 == (int)ContactType.Phone
                    ? input.ContactData1
                    : input.ContactType2 == (int)ContactType.Phone
                        ? input.ContactData2
                        : null,
                Website = input.Website,
                Note = input.Note,
                BillFlag = input.BillFlag,
                InFlag = input.InFlag,
                PotentialFlag = input.PotentialFlag,
                M_Customer_BusinessUser = input.Items.Select(
                    (item, index) => new M_Customer_BusinessUser
                    {
                        BUID = item.BUID,
                        MappingType = GetBusinessUserMappingType(item), SortNo = index + 1,
                        ActiveFlag = true
                    }).ToList()
            };

            // 先寫進 DB, 不然沒有 TargetId 可以用

            await DC.AddAsync(newData);
            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            // 寫 M_Contect
            await EditContactAndAddIfNew(newData, input.ContactType1, input.ContactData1, 1);
            await EditContactAndAddIfNew(newData, input.ContactType2, input.ContactData2, 2);

            // 寫業務的 M_Contect
            foreach (M_Customer_BusinessUser cbu in newData.M_Customer_BusinessUser)
            {
                Customer_Submit_BUID_APIItem thisInput = input.Items.FirstOrDefault(i => i.BUID == cbu.BUID);

                if (thisInput is null)
                    continue;

                await EditContactAndAddIfNew(cbu, (int)ContactType.Phone, thisInput.Phone, 1);
            }

            // 寫一筆 M_Address

            if (input.Address.HasContent())
            {
                var address = new M_Address();
                SetAddressValues(input, newData, address);
                await DC.AddAsync(address);
            }

            return newData;
        }

        private async Task EditContactAndAddIfNew(Customer customer, int contactType, string contactData, int sortNo)
        {
            await EditContactAndAddIfNew(DC.GetTableName<Customer>(), customer.CID, contactType, contactData, sortNo);
        }

        private async Task EditContactAndAddIfNew(M_Customer_BusinessUser customerBusinessUser, int? contactType,
            string contactData, int sortNo)
        {
            // 當沒有正確的資訊時，不做任何事
            if (contactType is null || !contactType.Value.IsInBetween(0, 3) || contactData.IsNullOrWhiteSpace())
                return;

            await EditContactAndAddIfNew(DC.GetTableName<M_Customer_BusinessUser>(), customerBusinessUser.MID,
                contactType.Value, contactData, sortNo);
        }

        private async Task EditContactAndAddIfNew(string tableName, int targetId, int contactType, string contactData,
            int sortNo)
        {
            if (!Enum.GetValues(typeof(ContactType)).Cast<int>().Contains(contactType))
                return;

            if (contactData.IsNullOrWhiteSpace())
                return;

            M_Contect contact = null;

            if (targetId.IsAboveZero())
            {
                contact = DC.M_Contect
                    .Where(c => c.TargetTable == tableName)
                    .Where(c => c.TargetID == targetId)
                    .FirstOrDefault(c => c.SortNo == sortNo);
            }

            contact = contact ?? new M_Contect();
            contact.ContectType = contactType;
            contact.TargetTable = tableName;
            contact.TargetID = targetId;
            contact.ContectData = contactData;
            contact.SortNo = sortNo;

            if (contact.MID == 0)
                await DC.M_Contect.AddAsync(contact);
        }

        private void SetAddressValues(Customer_Submit_Input_APIItem input, Customer customer, M_Address addressToEdit)
        {
            addressToEdit.Title = AddressTitle;
            addressToEdit.TargetTable = DC.GetTableName<Customer>();
            addressToEdit.TargetID = customer.CID;

            // 只在有給值時帶入，否則存為 null
            if (input.DZID.IsAboveZero())
            {
                addressToEdit.DZID = input.DZID;

                addressToEdit.ZipCode = DC.D_Zip
                    .Where(z => z.ActiveFlag && !z.DeleteFlag && z.DZID == input.DZID)
                    .Select(z => z.Code)
                    .FirstOrDefault();
            }

            addressToEdit.Address = input.Address;
            addressToEdit.SortNo = 1;
        }

        private int GetBusinessUserMappingType(Customer_Submit_BUID_APIItem input)
        {
            if (input.MKSalesFlag && input.OPSalesFlag)
                return 3;
            if (input.OPSalesFlag)
                return 2;
            if (input.MKSalesFlag)
                return 1;

            return 0;
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Customer_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                // 驗證輸入
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("客戶 ID", nameof(input.CID))))
                .Validate(i => i.Code.HasLengthBetween(1, 10),
                    () => AddError(LengthOutOfRange("客戶代號", nameof(input.Code), 1, 10)))
                .ValidateAsync(
                    async i => !await DC.Customer.AnyAsync(c => c.Code == i.Code && !c.DeleteFlag && c.CID != i.CID),
                    () => AddError(AlreadyExists("客戶代號", nameof(input.Code))))
                .Validate(i => i.Compilation.IsNullOrWhiteSpace() || i.Compilation.Length == 8,
                    () => AddError(WrongFormat("統一編號", nameof(input.Compilation))))
                .Validate(i => i.TitleC.HasContent(), () => AddError(EmptyNotAllowed("客戶名稱（中文）", nameof(input.TitleC))))
                .Validate(i => i.TitleC.HasLengthBetween(1, 50),
                    () => AddError(LengthOutOfRange("客戶名稱（中文）", nameof(input.TitleC), 1, 50)))
                .Validate(i => i.TitleE.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("客戶名稱（英文）", nameof(input.TitleE), 0, 100)))
                .Validate(i => i.Address.HasLengthBetween(0, 200),
                    () => AddError(LengthOutOfRange("地址", nameof(input.Address), 0, 200)))
                .Validate(i => i.Email.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("Email", nameof(input.Email), 0, 100)))
                .Validate(i => i.InvoiceTitle.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("發票抬頭", nameof(input.InvoiceTitle), 0, 50)))
                .Validate(i => i.ContactName.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("聯絡人名稱", nameof(input.ContactName), 0, 50)))
                .Validate(i => i.ContactData1.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("聯絡方式 1 的內容", nameof(input.ContactData1), 0, 30)))
                .Validate(i => i.ContactData2.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("聯絡方式 2 的內容", nameof(input.ContactData2), 0, 30)))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID6, StaticCodeType.Industry),
                    () => AddError(NotFound("行業別 ID", nameof(input.BSCID6))))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID4, StaticCodeType.Region),
                    () => AddError(NotFound("區域別 ID", nameof(input.BSCID4))))
                .Validate(i => i.DZID.IsZeroOrAbove(), () => AddError(WrongFormat("國籍與郵遞區號 ID", nameof(input.DZID))))
                .ForceSkipIf(i => i.ContactType1 == -1)
                .Validate(i => i.ContactType1.IsInBetween(0, 3),
                    () => AddError(NotSupportedValue("聯絡方式 1 的類型", nameof(input.ContactType1), null)))
                .Validate(i => i.ContactData1.HasContent(),
                    () => AddError(EmptyNotAllowed("聯絡方式 1 的內容", nameof(input.ContactData1))))
                .StopForceSkipping()
                .ForceSkipIf(i => i.ContactType2 == -1)
                .Validate(i => i.ContactType2.IsInBetween(0, 3),
                    () => AddError(NotSupportedValue("聯絡方式 2 的類型", nameof(input.ContactType2), null)))
                .Validate(i => i.ContactData2.HasContent(),
                    () => AddError(EmptyNotAllowed("聯絡方式 2 的內容", nameof(input.ContactData2))))
                .StopForceSkipping()
                .ForceSkipIf(i => i.DZID <= 0)
                .ValidateAsync(async i => await DC.D_Zip.ValidateIdExists(i.DZID, nameof(D_Zip.DZID)),
                    () => AddError(NotFound("國籍與郵遞區號 ID", nameof(input.DZID))))
                .StopForceSkipping()
                .IsValid();

            isValid = isValid &&
                      // 當前面輸入都正確時，繼續驗證所有 BUID 都是實際存在的 BU 資料
                      await input.Items.StartValidateElements()
                          .SkipIfAlreadyInvalid()
                          .Validate(i => i.BUID.IsAboveZero(),
                              () => AddError(EmptyNotAllowed("業務負責人", nameof(Customer_Submit_BUID_APIItem.BUID))))
                          .ValidateAsync(
                              async i => await DC.BusinessUser.ValidateIdExists(i.BUID, nameof(BusinessUser.BUID)),
                              () => AddError(NotFound("業務負責人", nameof(Customer_Submit_BUID_APIItem.BUID))))
                          .IsValid();

            if (input.ActiveFlag == false && !await ChangeActiveValidateReservation(input.CID))
                return false;

            // 驗證業務如果有輸入聯絡方式種類時，輸入欄位格式正確
            bool isBusinessUserContactValid = input.Items.StartValidateElements()
                .Validate(i => i.Phone.HasLengthBetween(0, 50),
                    i => AddError(LengthOutOfRange($"業務負責人（ID {i.BUID}）的聯絡電話", nameof(i.Phone), 0, 50)))
                .Validate(i => i.Phone.All(Char.IsDigit),
                    i => AddError(WrongFormat($"業務負責人（ID {i.BUID}）的聯絡電話", nameof(i.Phone))))
                .IsValid();


            return await Task.FromResult(isValid && isBusinessUserContactValid);
        }

        public IQueryable<Customer> SubmitEditQuery(Customer_Submit_Input_APIItem input)
        {
            return DC.Customer
                .Include(c => c.M_Customer_BusinessUser)
                .Where(c => c.CID == input.CID);
        }

        public void SubmitEditUpdateDataFields(Customer data, Customer_Submit_Input_APIItem input)
        {
            // 先刪除所有舊有的 M_Customer_BusinessUser
            var allInputBuIds = input.Items.ToDictionary(item => item.BUID, item => item);
            var allAlreadyExistingCustomerBusinessUser = data.M_Customer_BusinessUser
                .Where(cbu => cbu.ActiveFlag && !cbu.DeleteFlag)
                .Where(cbu => allInputBuIds.ContainsKey(cbu.BUID))
                .ToDictionary(cbu => cbu.BUID, cbu => cbu);

            DC.M_Customer_BusinessUser.RemoveRange(
                data.M_Customer_BusinessUser.Except(allAlreadyExistingCustomerBusinessUser.Values));

            var inputBuIdsToCreate =
                allInputBuIds.Where(kvp => !allAlreadyExistingCustomerBusinessUser.ContainsKey(kvp.Key));

            // 更新資料
            data.BSCID6 = input.BSCID6;
            data.BSCID4 = input.BSCID4;
            data.Code = input.Code ?? data.Code;
            data.Compilation = input.Compilation ?? data.Compilation;
            data.TitleC = input.TitleC ?? data.TitleC;
            data.TitleE = input.TitleE ?? data.TitleE;
            data.Email = input.Email ?? data.Email;
            data.InvoiceTitle = input.InvoiceTitle ?? data.InvoiceTitle;
            data.ContectName = input.ContactName ?? data.ContectName;
            data.ContectPhone = input.ContactType1 == (int)ContactType.Phone
                ? input.ContactData1
                : input.ContactType2 == (int)ContactType.Phone
                    ? input.ContactData2
                    : data.ContectPhone;
            data.Website = input.Website ?? data.Website;
            data.Note = input.Note ?? data.Note;
            data.BillFlag = input.BillFlag;
            data.InFlag = input.InFlag;
            data.PotentialFlag = input.PotentialFlag;

            // 修改舊資料
            foreach (M_Customer_BusinessUser alreadyExisting in data.M_Customer_BusinessUser)
            {
                alreadyExisting.MappingType = GetBusinessUserMappingType(allInputBuIds[alreadyExisting.BUID]);
            }

            int originalMaxSortNo = data.M_Customer_BusinessUser
                .Select(cbu => cbu.SortNo)
                .OrderBy(sortNo => sortNo)
                .FirstOrDefault();

            // 增加新資料
            data.M_Customer_BusinessUser = data.M_Customer_BusinessUser.Concat(inputBuIdsToCreate.Select(
                (kvp, index) => new M_Customer_BusinessUser
                {
                    CID = data.CID,
                    BUID = kvp.Key,
                    MappingType = GetBusinessUserMappingType(kvp.Value),
                    SortNo = originalMaxSortNo + 1 + index,
                    ActiveFlag = true
                })).ToList();

            // 寫 M_Contect
            Task.Run(() => EditContactAndAddIfNew(data, input.ContactType1, input.ContactData1, 1)).GetAwaiter()
                .GetResult();
            Task.Run(() => EditContactAndAddIfNew(data, input.ContactType2, input.ContactData2, 2)).GetAwaiter()
                .GetResult();

            // 寫業務的 M_Contect
            foreach (M_Customer_BusinessUser cbu in data.M_Customer_BusinessUser)
            {
                Customer_Submit_BUID_APIItem thisInput = input.Items.FirstOrDefault(i => i.BUID == cbu.BUID);

                if (thisInput is null)
                    continue;

                Task.Run(() => EditContactAndAddIfNew(cbu, (int)ContactType.Phone, thisInput.Phone, 1))
                    .GetAwaiter().GetResult();
            }

            // 更新 M_Address
            if (input.Address.HasContent())
            {
                M_Address address = Task.Run(() => GetAddresses(data.CID)).Result.FirstOrDefault() ?? new M_Address();
                SetAddressValues(input, data, address);

                if (!address.MID.IsAboveZero())
                    DC.M_Address.Add(address);
            }
        }

        #endregion

        #endregion
    }
}