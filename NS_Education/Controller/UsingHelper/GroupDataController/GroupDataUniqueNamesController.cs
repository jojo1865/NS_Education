using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.GroupData.GetUniqueNames;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Controller.UsingHelper.GroupDataController
{
    /// <summary>
    /// 處理取得角色獨特名稱的控制器。
    /// </summary>
    public class GroupDataUniqueNamesController : PublicClass,
        IGetListUniqueField<GroupData, GroupData_GetUniqueNames_Output_APIItem>
    {
        #region Initialization

        private IGetListAllHelper<CommonRequestForUniqueField> _getUniqueNamesHelper;

        public GroupDataUniqueNamesController()
        {
            _getUniqueNamesHelper = new GetListUniqueFieldHelper<GroupDataUniqueNamesController, GroupData,
                GroupData_GetUniqueNames_Output_APIItem>(this);
        }

        #endregion

        #region GetUniqueNames

        /// <summary>
        /// 取得獨特的角色名稱列表。實際端點請參考 RouteConfig。
        /// </summary>
        /// <param name="input"><see cref="CommonRequestForUniqueField"/></param>
        /// <returns><see cref="GroupData_GetUniqueNames_Output_APIItem"/></returns>
        public async Task<string> GetList(CommonRequestForUniqueField input)
        {
            return await _getUniqueNamesHelper.GetAllList(input);
        }

        public IOrderedQueryable<GroupData> GetListUniqueFieldsOrderQuery(IQueryable<GroupData> query)
        {
            return query.OrderBy(gd => gd.GID);
        }

        public IQueryable<GroupData_GetUniqueNames_Output_APIItem> GetListUniqueFieldsApplyKeywordFilter(
            IQueryable<GroupData_GetUniqueNames_Output_APIItem> query, string keyword)
        {
            return query.Where(gd => gd.Title.Contains(keyword) || gd.GID.ToString().Contains(keyword));
        }

        public Expression<Func<GroupData, GroupData_GetUniqueNames_Output_APIItem>> GetListUniqueFieldsQueryExpression()
        {
            return g => new GroupData_GetUniqueNames_Output_APIItem
            {
                GID = g.GID,
                Title = g.Title ?? ""
            };
        }

        #endregion
    }
}