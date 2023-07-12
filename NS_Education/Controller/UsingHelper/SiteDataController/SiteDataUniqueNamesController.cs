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
    public class SiteDataUniqueNamesController : PublicClass, IGetListUniqueField<B_SiteData>
    {
        #region Initialization

        private readonly IGetListAllHelper<CommonRequestForUniqueField> _getListUniqueFieldHelper;

        public SiteDataUniqueNamesController()
        {
            _getListUniqueFieldHelper = new GetListUniqueFieldHelper<SiteDataUniqueNamesController, B_SiteData>(this);
        }

        #endregion

        #region GetList

        /// <inheritdoc />
        public async Task<string> GetList(CommonRequestForUniqueField input)
        {
            // 確切的 Route 請參考 RouteConfig
            return await _getListUniqueFieldHelper.GetAllList(input);
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