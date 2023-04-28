using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.MenuData.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    public class MenuDataController : PublicClass
        , IGetListAll<MenuData, MenuData_GetList_Input_APIItem, MenuData_GetList_Output_Row_APIItem>
    {
        #region Initialization
        
        private readonly IGetListAllHelper<MenuData_GetList_Input_APIItem> _getListAllHelper;

        public MenuDataController()
        {
            _getListAllHelper =
                new GetListAllHelper<MenuDataController, MenuData, MenuData_GetList_Input_APIItem,
                    MenuData_GetList_Output_Row_APIItem>(
                    this);
        }

        #endregion
        
        #region GetList
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(MenuData_GetList_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(MenuData_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.ParentID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之選單上層 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<MenuData> GetListAllOrderedQuery(MenuData_GetList_Input_APIItem input)
        {
            // 如果沒有 Admin role, 只回傳使用者有權限的選單
            bool hasAdminRole = FilterStaticTools.HasRoleInRequest(Request, AuthorizeBy.Admin);
            IQueryable<MenuData> query = hasAdminRole ? GetListQueryAdmin(input) : GetListQueryUser(input);
            
            if (input.ParentID.IsValidId())
                query = query.Where(md => md.ParentID == input.ParentID);

            return query.OrderBy(md => md.SortNo)
                .ThenBy(md => md.URL)
                .ThenBy(md => md.Title)
                .ThenBy(md => md.MDID);
        }

        private IQueryable<MenuData> GetListQueryUser(MenuData_GetList_Input_APIItem input)
        {
            // 從 UserData 開始
            var query = DC.UserData
                .Include(ud => ud.M_Group_User)
                .ThenInclude(mgu => mgu.G)
                .ThenInclude(g => g.M_Group_Menu)
                .ThenInclude(mgm => mgm.MD)
                .Where(ud => ud.UID == GetUid())
                .Where(ud => ud.ActiveFlag && !ud.DeleteFlag)
                // M_Group_User
                .SelectMany(ud => ud.M_Group_User)
                // Group
                .Select(mgu => mgu.G)
                .Where(g => g.ActiveFlag && !g.DeleteFlag)
                // M_Group_Menu
                .SelectMany(g => g.M_Group_Menu)
                // 有任何權限才予以回傳
                .Where(mgm => mgm.AddFlag || mgm.DeleteFlag || mgm.EditFlag || mgm.PringFlag || mgm.ShowFlag)
                // MenuData
                .Select(mgm => mgm.MD);

            return query;
        }

        private IQueryable<MenuData> GetListQueryAdmin(MenuData_GetList_Input_APIItem input)
        {
            var query = DC.MenuData.AsQueryable();

            return query;
        }

        public async Task<MenuData_GetList_Output_Row_APIItem> GetListAllEntityToRow(MenuData entity)
        {
            return await Task.FromResult(new MenuData_GetList_Output_Row_APIItem
            {
                MDID = entity.MDID,
                Title = entity.Title ?? "",
                URL = entity.URL ?? "",
                SortNo = entity.SortNo
            });
        }
        #endregion
    }
}