using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.SiteData.GetUniqueNames;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.SiteDataController
{
    public class SiteDataUniqueNamesController : PublicClass,
        IGetListUniqueField<B_SiteData, SiteData_GetUniqueNames_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<CommonRequestForUniqueField> _getListUniqueFieldHelper;

        public SiteDataUniqueNamesController()
        {
            _getListUniqueFieldHelper =
                new GetListUniqueFieldHelper<SiteDataUniqueNamesController, B_SiteData,
                    SiteData_GetUniqueNames_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetList

        /// <inheritdoc />
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CommonRequestForUniqueField input)
        {
            // 確切的 Route 請參考 RouteConfig
            return await _getListUniqueFieldHelper.GetAllList(input);
        }

        /// <inheritdoc />
        public IOrderedQueryable<B_SiteData> GetListUniqueFieldsOrderQuery(IQueryable<B_SiteData> query)
        {
            return query.OrderBy(sd => sd.BSID);
        }

        public IQueryable<SiteData_GetUniqueNames_Output_Row_APIItem> GetListUniqueFieldsApplyKeywordFilter(
            IQueryable<SiteData_GetUniqueNames_Output_Row_APIItem> query, string keyword)
        {
            return query.Where(q => q.Title.Contains(keyword));
        }

        /// <inheritdoc />
        public Expression<Func<B_SiteData, SiteData_GetUniqueNames_Output_Row_APIItem>>
            GetListUniqueFieldsQueryExpression()
        {
            return sd => new SiteData_GetUniqueNames_Output_Row_APIItem
            {
                BSID = sd.BSID,
                Title = sd.Title ?? ""
            };
        }

        #endregion
    }
}