using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Controller.UsingHelper.SiteDataController
{
    public class SiteDataUniqueNamesController : PublicClass, IGetListUniqueNames<B_SiteData>
    {
        #region Initialization

        private readonly IGetListAllHelper<CommonRequestForUniqueNames> _getListUniqueNamesHelper;

        public SiteDataUniqueNamesController()
        {
            _getListUniqueNamesHelper = new GetListUniqueNamesHelper<SiteDataUniqueNamesController, B_SiteData>(this);
        }

        #endregion

        #region GetList

        /// <inheritdoc />
        public async Task<string> GetList(CommonRequestForUniqueNames input)
        {
            // 確切的 Route 請參考 RouteConfig
            return await _getListUniqueNamesHelper.GetAllList(input);
        }

        /// <inheritdoc />
        public IOrderedQueryable<B_SiteData> GetListUniqueNamesOrderQuery(IQueryable<B_SiteData> query)
        {
            return query.OrderBy(sd => sd.BSID);
        }

        /// <inheritdoc />
        public Expression<Func<B_SiteData, string>> GetListUniqueNamesQueryExpression()
        {
            return sd => sd.Title;
        }

        #endregion
    }
}