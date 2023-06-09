using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.CategoryController
{
    /// <summary>
    /// 處理 Category 的 GetTypeList 功能。
    /// </summary>
    public class CategoryTypeListController : PublicClass,
        IGetListLocal<CommonResponseRowIdTitle>
    {
        #region Initialization

        private readonly IGetListLocalHelper _getListLocalHelper;

        public CategoryTypeListController()
        {
            _getListLocalHelper = new GetListLocalHelper<CategoryTypeListController, CommonResponseRowIdTitle>(this);
        }

        #endregion

        #region GetTypeList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            return await _getListLocalHelper.GetListLocal();
        }

        public async Task<ICollection<CommonResponseRowIdTitle>> GetListLocalResults()
        {
            return await Task.FromResult(GetCategoryTypeList(CategoryTypes));
        }

        public static IList<CommonResponseRowIdTitle> GetCategoryTypeList(string[] sCategoryTypes)
        {
            return sCategoryTypes.Select((s, i) => new CommonResponseRowIdTitle
            {
                ID = i,
                Title = s
            }).ToList();
        }

        #endregion
    }
}