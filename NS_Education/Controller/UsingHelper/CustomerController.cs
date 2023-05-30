using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Customer.GetInfoById;
using NS_Education.Models.APIItems.Controller.Customer.GetList;
using NS_Education.Models.APIItems.Controller.Customer.Submit;
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
    public class CustomerController : PublicClass,
        IGetListPaged<Customer, Customer_GetList_Input_APIItem, Customer_GetList_Output_Row_APIItem>,
        IGetInfoById<Customer, Customer_GetInfoById_Output_APIItem>,
        IChangeActive<Customer>,
        IDeleteItem<Customer>,
        ISubmit<Customer, Customer_Submit_Input_APIItem>
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

        private readonly IGetListPagedHelper<Customer_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<Customer_Submit_Input_APIItem> _submitHelper;

        public CustomerController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CustomerController, Customer, Customer_GetList_Input_APIItem,
                    Customer_GetList_Output_Row_APIItem>(this);
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
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Customer_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BSCID6.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選的行業別")))
                .Validate(i => i.BSCID4.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選的區域別")))
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
                .Include(c => c.CustomerGift)
                .Include(c => c.M_Customer_BusinessUser)
                .Include(c => c.M_Customer_BusinessUser.Select(cbu => cbu.BusinessUser))
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(c =>
                    c.TitleC.Contains(input.Keyword)
                    || c.TitleE.Contains(input.Keyword)
                    || c.Compilation.Contains(input.Keyword)
                    || c.Code.Contains(input.Keyword));

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

        public async Task<Customer_GetList_Output_Row_APIItem> GetListPagedEntityToRow(Customer entity)
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
                ContactPhone = entity.ContectPhone ?? "",
                Website = entity.Website ?? "",
                Note = entity.Note ?? "",
                BillFlag = entity.BillFlag,
                InFlag = entity.InFlag,
                PotentialFlag = entity.PotentialFlag,
                ResverCt = entity.Resver_Head.Count(rh =>
                    !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft),
                VisitCt = entity.CustomerVisit.Count(cv => !cv.DeleteFlag),
                QuestionCt = entity.CustomerQuestion.Count(cq => !cq.DeleteFlag),
                GiftCt = entity.CustomerGift.Count(cg => !cg.DeleteFlag),
                Items = GetBusinessUserListFromEntity(entity)
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
                .Where(c => c.CID == id);
        }

        public async Task<Customer_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(Customer entity)
        {
            M_Address address = (await GetAddresses(entity.CID)).FirstOrDefault();

            return await Task.FromResult(new Customer_GetInfoById_Output_APIItem
            {
                CID = entity.CID,
                BSCID6 = entity.BSCID6,
                BSC6_Title = entity.B_StaticCode1?.Title ?? "",
                BSC6_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode1?.CodeType,
                    entity.BSCID6),
                BSCID4 = entity.BSCID4,
                BSC4_Title = entity.B_StaticCode?.Title ?? "",
                BSC4_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode?.CodeType,
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
                ContactPhone = entity.ContectPhone ?? "",
                Website = entity.Website ?? "",
                Note = entity.Note ?? "",
                BillFlag = entity.BillFlag,
                InFlag = entity.InFlag,
                PotentialFlag = entity.PotentialFlag,
                ResverCt = entity.Resver_Head.Count(rh =>
                    !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft),
                VisitCt = entity.CustomerVisit.Count(cv => !cv.DeleteFlag),
                QuestionCt = entity.CustomerQuestion.Count(cq => !cq.DeleteFlag),
                GiftCt = entity.CustomerGift.Count(cg => !cg.DeleteFlag),
                Items = GetBusinessUserListFromEntity(entity)
            });
        }

        private static List<Customer_GetList_BusinessUser_APIItem> GetBusinessUserListFromEntity(Customer entity)
        {
            return entity.M_Customer_BusinessUser
                .Where(cbu =>
                    cbu.ActiveFlag && !cbu.DeleteFlag && cbu.BusinessUser.ActiveFlag && !cbu.BusinessUser.DeleteFlag)
                .Select(cbu => new Customer_GetList_BusinessUser_APIItem
                {
                    BUID = cbu.BUID,
                    Name = cbu.BusinessUser?.Name ?? ""
                }).ToList();
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
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
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<Customer> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.Customer.Where(c => ids.Contains(c.CID));
        }

        #endregion

        #region Submit

        private const string SubmitBuIdNotFound = "其中一筆或多筆業務 ID 查無資料！";
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

        private async Task<bool> SubmitCheckAllBuIdExists(IEnumerable<Customer_Submit_BUID_APIItem> items)
        {
            HashSet<int> buIdSet = items.Select(item => item.BUID).ToHashSet();
            return await DC.BusinessUser
                .Where(bu => bu.ActiveFlag && !bu.DeleteFlag && buIdSet.Contains(bu.BUID))
                .CountAsync() == buIdSet.Count;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Customer_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                // 驗證輸入
                .Validate(i => i.CID == 0, () => AddError(WrongFormat("客戶 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID6, StaticCodeType.Industry),
                    () => AddError(NotFound("行業別 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID4, StaticCodeType.Region),
                    () => AddError(NotFound("區域別 ID")))
                .Validate(i => i.TitleC.HasContent(), () => AddError(EmptyNotAllowed("客戶名稱（中文）")))
                .Validate(i => i.DZID.IsZeroOrAbove(), () => AddError(WrongFormat("國籍與郵遞區號 ID")))
                .ForceSkipIf(i => i.DZID <= 0)
                .ValidateAsync(async i => await DC.D_Zip.ValidateIdExists(i.DZID, nameof(D_Zip.DZID)),
                    () => AddError(NotFound("國籍與郵遞區號 ID")))
                .StopForceSkipping()

                // 當前面輸入都正確時，繼續驗證所有 BUID 都是實際存在的 BU 資料
                .SkipIfAlreadyInvalid()
                .ValidateAsync(async i => await SubmitCheckAllBuIdExists(i.Items), () => AddError(SubmitBuIdNotFound))
                .IsValid();

            return await Task.FromResult(isValid);
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
                ContectPhone = input.ContactPhone,
                Website = input.Website,
                Note = input.Note,
                BillFlag = input.BillFlag,
                InFlag = input.InFlag,
                PotentialFlag = input.PotentialFlag,
                M_Customer_BusinessUser = input.Items.Select(
                    (item, index) => new M_Customer_BusinessUser
                    {
                        BUID = item.BUID,
                        MappingType = GetBusinessUserMappingType(item.BUID), SortNo = index + 1,
                        ActiveFlag = true
                    }).ToList()
            };

            // 先寫進 DB, 不然沒有 TargetId 可以用

            await DC.AddAsync(newData);
            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            // 寫一筆 M_Address

            var address = new M_Address();
            SetAddressValues(input, newData, address);

            await DC.AddAsync(address);
            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            return newData;
        }

        private void SetAddressValues(Customer_Submit_Input_APIItem input, Customer customer, M_Address addressToEdit)
        {
            addressToEdit.Title = AddressTitle;
            addressToEdit.TargetTable = DC.GetTableName<Customer>();
            addressToEdit.TargetID = customer.CID;
            addressToEdit.DZID = input.DZID;
            addressToEdit.ZipCode = DC.D_Zip
                .Where(z => z.ActiveFlag && !z.DeleteFlag && z.DZID == input.DZID)
                .Select(z => z.Code)
                .FirstOrDefault();
            addressToEdit.Address = input.Address;
            addressToEdit.SortNo = 1;
        }

        private int GetBusinessUserMappingType(int buId)
        {
            return DC.BusinessUser
                .Where(bu => bu.BUID == buId && bu.ActiveFlag && !bu.DeleteFlag)
                .Select(bu => bu.OPsalesFlag ? 2 : bu.MKsalesFlag ? 1 : 0)
                .FirstOrDefault();
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Customer_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                // 驗證輸入
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("客戶 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID6, StaticCodeType.Industry),
                    () => AddError(NotFound("行業別 ID")))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID4, StaticCodeType.Region),
                    () => AddError(NotFound("區域別 ID")))
                .Validate(i => i.TitleC.HasContent(), () => AddError(EmptyNotAllowed("客戶名稱（中文）")))
                .Validate(i => i.DZID.IsZeroOrAbove(), () => AddError(WrongFormat("國籍與郵遞區號 ID")))
                .ForceSkipIf(i => i.DZID <= 0)
                .ValidateAsync(async i => await DC.D_Zip.ValidateIdExists(i.DZID, nameof(D_Zip.DZID)),
                    () => AddError(NotFound("國籍與郵遞區號 ID")))
                .StopForceSkipping()

                // 當前面輸入都正確時，繼續驗證所有 BUID 都是實際存在的 BU 資料
                .SkipIfAlreadyInvalid()
                .ValidateAsync(async i => await SubmitCheckAllBuIdExists(i.Items), () => AddError(SubmitBuIdNotFound))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<Customer> SubmitEditQuery(Customer_Submit_Input_APIItem input)
        {
            return DC.Customer.Where(c => c.CID == input.CID);
        }

        public void SubmitEditUpdateDataFields(Customer data, Customer_Submit_Input_APIItem input)
        {
            // 先刪除所有舊有的 M_Customer_BusinessUser
            var allInputBuIds = input.Items.ToDictionary(item => item.BUID, item => item);
            var allAlreadyExistingCustomerBusinessUser = data.M_Customer_BusinessUser
                .Where(cbu => cbu.ActiveFlag && !cbu.DeleteFlag)
                .Where(cbu => allInputBuIds.ContainsKey(cbu.BUID))
                .ToArray();

            DC.M_Customer_BusinessUser.RemoveRange(
                data.M_Customer_BusinessUser.Except(allAlreadyExistingCustomerBusinessUser));

            var inputBuIdsToCreate =
                allInputBuIds.Keys.Except(allAlreadyExistingCustomerBusinessUser.Select(item => item.BUID));

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
            data.ContectPhone = input.ContactPhone ?? data.ContectPhone;
            data.Website = input.Website ?? data.Website;
            data.Note = input.Note ?? data.Note;
            data.BillFlag = input.BillFlag;
            data.InFlag = input.InFlag;
            data.PotentialFlag = input.PotentialFlag;
            data.M_Customer_BusinessUser = data.M_Customer_BusinessUser.Concat(inputBuIdsToCreate.Select(
                (id, index) => new M_Customer_BusinessUser
                {
                    CID = data.CID,
                    BUID = id,
                    MappingType = GetBusinessUserMappingType(id),
                    SortNo = index + 1,
                    ActiveFlag = true
                })).ToList();

            // 更新 M_Address
            string targetTableName = DC.GetTableName<Customer>();

            M_Address address = Task.Run(() => GetAddresses(data.CID)).Result.FirstOrDefault() ?? new M_Address();
            SetAddressValues(input, data, address);

            if (!address.MID.IsAboveZero())
                DC.M_Address.Add(address);
        }

        #endregion

        #endregion
    }
}