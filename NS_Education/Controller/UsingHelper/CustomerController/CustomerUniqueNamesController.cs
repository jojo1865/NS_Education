using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.Customer.GetUniqueNames;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.CustomerController
{
    public class CustomerUniqueNamesController : PublicClass,
        IGetListAll<Customer, Customer_GetUniqueNames_Input_APIItem, Customer_GetUniqueNames_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<Customer_GetUniqueNames_Input_APIItem> _getListAllHelper;

        public CustomerUniqueNamesController()
        {
            _getListAllHelper =
                new GetListAllHelper<CustomerUniqueNamesController, Customer, Customer_GetUniqueNames_Input_APIItem,
                    Customer_GetUniqueNames_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetUniqueNames

        /// <inheritdoc />
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Customer_GetUniqueNames_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        /// <inheritdoc />
        public async Task<bool> GetListAllValidateInput(Customer_GetUniqueNames_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MaxRow.IsInBetween(1, 1000)
                    , () => OutOfRange("最大筆數", 1, 1000))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        /// <inheritdoc />
        public IOrderedQueryable<Customer> GetListAllOrderedQuery(Customer_GetUniqueNames_Input_APIItem input)
        {
            var query = DC.Customer.AsQueryable();

            // 不考慮空白或 null 的名稱
            query = query.Where(c => c.TitleC != null && c.TitleC.Trim().Length > 0);

            // 因為需要做 Take, 所以這裡手動先判定 ActiveFlag/DeleteFlag
            if (input.ActiveFlag.IsInBetween(0, 1))
                query = query.Where(c => c.ActiveFlag == (input.ActiveFlag == 1));

            query = query.Where(c => c.DeleteFlag == (input.DeleteFlag == 1));

            // Keyword
            if (input.Keyword.HasContent())
                query = query.Where(c => c.TitleC.Contains(input.Keyword));

            // UNIQUE TitleC
            query = query.GroupBy(c => c.TitleC)
                .Where(c => c.Any())
                .Select(c => c.FirstOrDefault());

            query = query.OrderBy(c => c.CID).Take(input.MaxRow);

            return query.OrderBy(c => c.CID);
        }

        /// <inheritdoc />
        public async Task<Customer_GetUniqueNames_Output_Row_APIItem> GetListAllEntityToRow(Customer entity)
        {
            return await Task.FromResult(new Customer_GetUniqueNames_Output_Row_APIItem
            {
                CID = entity.CID,
                TitleC = entity.TitleC ?? ""
            });
        }

        #endregion
    }
}