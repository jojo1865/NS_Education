using System;
using System.Data.Entity.SqlServer;
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

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public class ResverUniqueIdController : PublicClass, IGetListUniqueField<Resver_Head>
    {
        #region Initialization

        private IGetListAllHelper<CommonRequestForUniqueField> _helper;

        public ResverUniqueIdController()
        {
            _helper = new GetListUniqueFieldHelper<ResverUniqueIdController, Resver_Head>(this);
        }

        #endregion

        #region GetUniqueIds

        /// <inheritdoc />
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CommonRequestForUniqueField input)
        {
            // 實際 endpoint 請參考 RouteConfig
            return await _helper.GetAllList(input);
        }

        /// <inheritdoc />
        public IOrderedQueryable<Resver_Head> GetListUniqueNamesOrderQuery(IQueryable<Resver_Head> query)
        {
            return query.OrderBy(rh => rh.RHID);
        }

        /// <inheritdoc />
        public Expression<Func<Resver_Head, string>> GetListUniqueNamesQueryExpression()
        {
            int minLength = DC.Resver_Head.Select(rh => rh.RHID.ToString().Length).OrderByDescending(l => l)
                .FirstOrDefault();
            minLength = Math.Max(minLength, 3); // 3 comes from UI/UX design example.
            return rh => SqlFunctions.Replicate("0", minLength - rh.RHID.ToString().Length) + rh.RHID + "（" +
                         (rh.Title ?? "") + "）";
        }

        #endregion
    }
}