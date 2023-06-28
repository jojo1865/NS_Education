using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.CustomerQuestionController
{
    public class CustomerQuestionUniqueAreasController : PublicClass, IGetListUniqueNames<CustomerQuestion>
    {
        #region Initialization

        private readonly IGetListAllHelper<CommonRequestForUniqueNames> _getListUniqueNamesHelper;

        public CustomerQuestionUniqueAreasController()
        {
            _getListUniqueNamesHelper =
                new GetListUniqueNamesHelper<CustomerQuestionUniqueAreasController, CustomerQuestion>(this);
        }

        #endregion

        #region GetUniqueAreas

        /// <inheritdoc />
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CommonRequestForUniqueNames input)
        {
            return await _getListUniqueNamesHelper.GetAllList(input);
        }

        /// <inheritdoc />
        public IOrderedQueryable<CustomerQuestion> GetListUniqueNamesOrderQuery(IQueryable<CustomerQuestion> query)
        {
            return query.OrderBy(q => q.CQID);
        }

        /// <inheritdoc />
        public Expression<Func<CustomerQuestion, string>> GetListUniqueNamesQueryExpression()
        {
            return question => question.AskArea;
        }

        #endregion
    }
}