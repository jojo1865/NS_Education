using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.CustomerQuestion.GetUniqueAreas;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.CustomerQuestionController
{
    public class CustomerQuestionUniqueAreasController : PublicClass,
        IGetListAll<CustomerQuestion, CustomerQuestion_GetUniqueAreas_Input_APIItem, string>
    {
        #region Initialization

        private readonly IGetListAllHelper<CustomerQuestion_GetUniqueAreas_Input_APIItem> _getListAllHelper;

        public CustomerQuestionUniqueAreasController()
        {
            _getListAllHelper =
                new GetListAllHelper<CustomerQuestionUniqueAreasController, CustomerQuestion,
                    CustomerQuestion_GetUniqueAreas_Input_APIItem, string>(this);
        }

        #endregion

        #region GetUniqueAreas

        /// <inheritdoc />
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CustomerQuestion_GetUniqueAreas_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        /// <inheritdoc />
        public async Task<bool> GetListAllValidateInput(CustomerQuestion_GetUniqueAreas_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MaxRow.IsInBetween(1, 1000)
                    , () => OutOfRange("最大筆數", nameof(input.MaxRow), 1, 1000))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        /// <inheritdoc />
        public IOrderedQueryable<CustomerQuestion> GetListAllOrderedQuery(
            CustomerQuestion_GetUniqueAreas_Input_APIItem input)
        {
            var query = DC.CustomerQuestion.AsQueryable();

            // 不考慮空白或 null 的名稱
            query = query.Where(cq => cq.AskArea != null && cq.AskArea.Trim().Length > 0);

            // 因為需要做 Take, 所以這裡手動先判定 DeleteFlag
            query = query.Where(cq => cq.DeleteFlag == (input.DeleteFlag == 1));

            // Keyword
            if (input.Keyword.HasContent())
                query = query.Where(cq => cq.AskArea.Contains(input.Keyword));

            // UNIQUE
            query = query.GroupBy(cq => cq.AskArea)
                .Where(cq => cq.Any())
                .Select(cq => cq.FirstOrDefault());

            query = query.OrderBy(cq => cq.CQID).Take(input.MaxRow);

            return query.OrderBy(cq => cq.CQID);
        }

        /// <inheritdoc />
        public async Task<string> GetListAllEntityToRow(CustomerQuestion entity)
        {
            return await Task.FromResult(entity.AskArea);
        }

        #endregion
    }
}