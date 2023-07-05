using System;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.Customer.GetRankings;
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
    public class CustomerRankingController : PublicClass,
        IGetListPaged<Customer, Customer_GetRankings_Input_APIItem, Customer_GetRankings_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Customer_GetRankings_Input_APIItem> helper;
        private DateTime _startDate = SqlDateTime.MinValue.Value;
        private DateTime _endDate = SqlDateTime.MaxValue.Value;

        public CustomerRankingController()
        {
            helper =
                new GetListPagedHelper<CustomerRankingController,
                    Customer,
                    Customer_GetRankings_Input_APIItem,
                    Customer_GetRankings_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetRankings

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Customer_GetRankings_Input_APIItem input)
        {
            return await helper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Customer_GetRankings_Input_APIItem input)
        {
            DateTime startDate = default, endDate = default;

            bool isValid = input.StartValidate()
                .Validate(i => Enum.IsDefined(typeof(Customer_GetRankings_OrderBy), input.OrderBy),
                    () => AddError(NotSupportedValue("排序方式", nameof(input.OrderBy), null)))
                .Validate(i => i.DateS.IsNullOrWhiteSpace() || i.DateS.TryParseDateTime(out startDate),
                    () => AddError(WrongFormat("欲篩選的起始日", nameof(input.DateS))))
                .Validate(i => i.DateE.IsNullOrWhiteSpace() || i.DateE.TryParseDateTime(out endDate),
                    () => AddError(WrongFormat("欲篩選的結束日", nameof(input.DateE))))
                .Validate(_ => startDate <= endDate,
                    () => AddError(MinLargerThanMax("起始日", nameof(input.DateS), "結束日", nameof(input.DateE))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Customer> GetListPagedOrderedQuery(Customer_GetRankings_Input_APIItem input)
        {
            IQueryable<Customer> query = DC.Customer.AsQueryable()
                .Include(c => c.Resver_Head)
                .Include(c => c.Resver_Head.Select(rh => rh.B_StaticCode))
                .Include(c => c.B_StaticCode1);

            // _startDate 和 _endDate 在將資料轉換為 response 時也會用到，
            // 所以作為控制器的參數儲存

            GetListRememberInputDates(input);

            query = query.Where(c => c.Resver_Head
                .Where(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft)
                .Any(rh => DbFunctions.TruncateTime(rh.SDate) >= _startDate));
            query = query.Where(c => c.Resver_Head
                .Where(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft)
                .Any(rh => DbFunctions.TruncateTime(rh.EDate) <= _endDate));

            Customer_GetRankings_OrderBy orderBy =
                (Customer_GetRankings_OrderBy)Enum.Parse(typeof(Customer_GetRankings_OrderBy),
                    input.OrderBy.ToString());

            IOrderedQueryable<Customer> orderedQuery;
            switch (orderBy)
            {
                case Customer_GetRankings_OrderBy.ResverCt:
                    orderedQuery = query.OrderByDescending(c =>
                        c.Resver_Head
                            .Where(rh => DbFunctions.TruncateTime(rh.SDate) >= _startDate)
                            .Where(rh => DbFunctions.TruncateTime(rh.EDate) <= _endDate)
                            .Count(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft));
                    break;
                case Customer_GetRankings_OrderBy.QuotedPrice:
                    orderedQuery = query.OrderByDescending(c => c.Resver_Head
                        .Where(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft)
                        .Where(rh => DbFunctions.TruncateTime(rh.SDate) >= _startDate)
                        .Where(rh => DbFunctions.TruncateTime(rh.EDate) <= _endDate)
                        .Sum(rh => rh.QuotedPrice));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return orderedQuery;
        }

        private void GetListRememberInputDates(Customer_GetRankings_Input_APIItem input)
        {
            if (input.DateS.TryParseDateTime(out DateTime newStartDate))
                _startDate = newStartDate;

            if (input.DateE.TryParseDateTime(out DateTime newEndDate))
                _endDate = newEndDate;

            _startDate = _startDate.Date;
            _endDate = _endDate.Date;
        }

        public async Task<Customer_GetRankings_Output_Row_APIItem> GetListPagedEntityToRow(Customer entity)
        {
            string targetTableName = DC.GetTableName<Customer>();
            M_Contect[] contacts = await DC.M_Contect
                .Where(c => c.TargetTable == targetTableName)
                .Where(c => c.TargetID == entity.CID)
                .OrderBy(c => c.SortNo)
                .ToArrayAsync();

            return await Task.FromResult(new Customer_GetRankings_Output_Row_APIItem
            {
                RentCt = entity.Resver_Head
                    .Where(rh => rh.SDate.Date >= _startDate)
                    .Where(rh => rh.EDate.Date <= _endDate)
                    .Count(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft),
                QuotedTotal = entity.Resver_Head
                    .Where(rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft)
                    .Where(rh => rh.SDate.Date >= _startDate)
                    .Where(rh => rh.EDate.Date <= _endDate)
                    .Sum(rh => (decimal)rh.QuotedPrice)
                    .ToString("N0"),
                Code = entity.Code ?? "",
                Title = entity.TitleC ?? entity.TitleE ?? "",
                Industry = entity.B_StaticCode1?.Title ?? "",
                ContactName = entity.ContectName,
                Contacts = contacts.Select(contact => new Customer_GetRankings_Output_Contact_APIItem
                        {
                            ContactType = ContactTypeController.GetContactTypeTitle(contact.ContectType) ?? "",
                            ContactData = contact.ContectData ?? ""
                        }
                    )
                    .ToList()
            });
        }

        #endregion
    }
}