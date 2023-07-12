using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.Resver.GetUniqueIds;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public class ResverUniqueIdController : PublicClass,
        IGetListUniqueField<Resver_Head, Resver_GetUniqueIds_Output_Row_APItem>
    {
        #region Initialization

        private IGetListAllHelper<CommonRequestForUniqueField> _helper;

        public ResverUniqueIdController()
        {
            _helper =
                new GetListUniqueFieldHelper<ResverUniqueIdController, Resver_Head,
                    Resver_GetUniqueIds_Output_Row_APItem>(this);
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
        public IOrderedQueryable<Resver_Head> GetListUniqueFieldsOrderQuery(IQueryable<Resver_Head> query)
        {
            return query.OrderBy(rh => rh.RHID);
        }

        public IQueryable<Resver_GetUniqueIds_Output_Row_APItem> GetListUniqueFieldsApplyKeywordFilter(
            IQueryable<Resver_GetUniqueIds_Output_Row_APItem> query, string keyword)
        {
            return query.Where(rh => rh.Title.Contains(keyword) || rh.RHID.ToString().Contains(keyword));
        }

        public Expression<Func<Resver_Head, Resver_GetUniqueIds_Output_Row_APItem>> GetListUniqueFieldsQueryExpression()
        {
            return rh => new Resver_GetUniqueIds_Output_Row_APItem
            {
                RHID = rh.RHID,
                Title = rh.Title
            };
        }

        #endregion
    }
}