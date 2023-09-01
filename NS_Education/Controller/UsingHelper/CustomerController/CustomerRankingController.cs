using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.Customer.GetRankings;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.CustomerController
{
    public class CustomerRankingController : PublicClass
    {
        #region Initialization

        private DateTime _startDate = SqlDateTime.MinValue.Value;
        private DateTime _endDate = SqlDateTime.MaxValue.Value;

        #endregion

        #region GetRankings

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Customer_GetRankings_Input_APIItem input)
        {
            // 特例：無法使用 helper
            // 排行榜需要支援不同欄位做排序
            // 但是最後要依照金額計算排名
            // 這裡採用全部查回記憶體，再做排名與 paging 的策略

            if (!await GetListPagedValidateInput(input))
                return GetResponseJson();

            // 取得查詢結果，這裡的排序 = 排名
            Dictionary<Customer, int> customerToRank = GetListPagedOrderedQuery(input)
                .AsEnumerable()
                .Select((c, index) => new { Customer = c, Index = index })
                .ToDictionary(ci => ci.Customer, ci => ci.Index);

            // 依據輸入的 Sorting 做排序
            IEnumerable<Customer> customers = GetListOrderByInput(customerToRank, input.Sorting).ToArray();

            var response = new CommonResponseForPagedList<Customer_GetRankings_Output_Row_APIItem>();
            response.SetByInput(input);

            (int skip, int take) = input.CalculateSkipAndTake(customers.Count());

            customers = customers
                .Skip(skip)
                .Take(take);

            int itemIndex = 0;
            foreach (Customer customer in customers)
            {
                response.Items.Add(await GetListPagedEntityToRow(customer, itemIndex++, customerToRank[customer]));
            }

            return GetResponseJson(response);
        }

        private IEnumerable<Customer> GetListOrderByInput(IDictionary<Customer, int> customers,
            IEnumerable<ListSorting> sorts)
        {
            sorts = sorts?.ToArray();

            if (sorts == null || !sorts.Any())
                return customers.Keys;

            IOrderedEnumerable<Customer> ordering = customers.Keys.AsOrderedEnumerable(true);

            foreach (ListSorting sort in sorts)
            {
                switch (sort.PropertyName)
                {
                    case "Rank":
                        ordering = ordering.Order(c => customers[c], sort.IsAscending);
                        break;
                    case "RentCt":
                        ordering = ordering.Order(
                            c => c.Resver_Head
                                .Where(rh => rh.SDate.Date >= _startDate)
                                .Where(rh => rh.EDate.Date <= _endDate)
                                .Count(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft)
                            , sort.IsAscending);
                        break;
                    case "QuotedTotal":
                        ordering = ordering.Order(
                            c => c.Resver_Head
                                .Where(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft)
                                .Where(rh => rh.SDate.Date >= _startDate)
                                .Where(rh => rh.EDate.Date <= _endDate)
                                .Sum(rh => rh.QuotedPrice)
                            , sort.IsAscending);
                        break;
                    case "CustomerCode":
                        ordering = ordering.Order(c => c.Code ?? "", sort.IsAscending);
                        break;
                    case "CustomerName":
                        ordering = ordering.Order(c => c.TitleC ?? c.TitleE ?? "", sort.IsAscending);
                        break;
                    case "Industry":
                        ordering = ordering.Order(c => c.B_StaticCode1?.SortNo ?? 0, sort.IsAscending);
                        break;
                    case "Contact":
                        ordering = ordering.Order(c => c.ContectName ?? "", sort.IsAscending);
                        break;
                }
            }

            return ordering;
        }

        public async Task<bool> GetListPagedValidateInput(Customer_GetRankings_Input_APIItem input)
        {
            DateTime startDate = default, endDate = default;

            bool isValid = input.StartValidate()
                .Validate(i => Enum.IsDefined(typeof(Customer_GetRankings_RankBy), input.RankBy),
                    () => AddError(NotSupportedValue("排序方式", nameof(input.RankBy), null)))
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
            IQueryable<Customer> query = DC.Customer
                .AsNoTracking()
                .AsQueryable()
                .Include(c => c.Resver_Head)
                .Include(c => c.Resver_Head.Select(rh => rh.B_StaticCode))
                .Include(c => c.B_StaticCode1);

            // _startDate 和 _endDate 在將資料轉換為 response 時也會用到，
            // 所以作為控制器的參數儲存

            GetListRememberInputDates(input);

            query = query.Where(c => c.Resver_Head
                .Where(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft)
                .Any(rh => DbFunctions.TruncateTime(rh.SDate) >= _startDate &&
                           DbFunctions.TruncateTime(rh.EDate) <= _endDate));

            Customer_GetRankings_RankBy rankBy =
                (Customer_GetRankings_RankBy)Enum.Parse(typeof(Customer_GetRankings_RankBy),
                    input.RankBy.ToString());

            IOrderedQueryable<Customer> orderedQuery;
            switch (rankBy)
            {
                case Customer_GetRankings_RankBy.ResverCt:
                    orderedQuery = query.OrderByDescending(c =>
                        c.Resver_Head
                            .Where(rh => DbFunctions.TruncateTime(rh.SDate) >= _startDate)
                            .Where(rh => DbFunctions.TruncateTime(rh.EDate) <= _endDate)
                            .Count(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft));
                    break;
                case Customer_GetRankings_RankBy.QuotedPrice:
                    orderedQuery = query.OrderByDescending(c => c.Resver_Head
                        .Where(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft)
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

        public async Task<Customer_GetRankings_Output_Row_APIItem> GetListPagedEntityToRow(Customer entity, int index,
            int rank)
        {
            string targetTableName = DC.GetTableName<Customer>();
            M_Contect[] contacts = await DC.M_Contect
                .Where(c => c.TargetTable == targetTableName)
                .Where(c => c.TargetID == entity.CID)
                .OrderBy(c => c.SortNo)
                .ToArrayAsync();

            return await Task.FromResult(new Customer_GetRankings_Output_Row_APIItem
            {
                Index = index,
                Rank = rank,
                RentCt = entity.Resver_Head
                    .Where(rh => rh.SDate.Date >= _startDate)
                    .Where(rh => rh.EDate.Date <= _endDate)
                    .Count(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft),
                QuotedTotal = entity.Resver_Head
                    .Where(rh => !rh.DeleteFlag && rh.State != (int)ReserveHeadGetListState.Draft)
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