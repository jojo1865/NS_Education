using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.MenuData.GetInfoById;
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
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper
{
    public class MenuDataController : PublicClass
        , IGetListAll<MenuData, MenuData_GetList_Input_APIItem, MenuData_GetList_Output_Row_APIItem>
        , IGetInfoById<MenuData, MenuData_GetInfoById_Output_APIItem>
    {
        #region Common

        private static string UpdateDbError(Exception e) => $"更新資料庫時失敗，請確認伺服器狀態：{e.Message}";

        #endregion

        #region Initialization

        private readonly IGetListAllHelper<MenuData_GetList_Input_APIItem> _getListAllHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public MenuDataController()
        {
            _getListAllHelper =
                new GetListAllHelper<MenuDataController, MenuData, MenuData_GetList_Input_APIItem,
                    MenuData_GetList_Output_Row_APIItem>(
                    this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<MenuDataController, MenuData, MenuData_GetInfoById_Output_APIItem>(this);
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
            IQueryable<M_Group_Menu> mGroupMenus = DC.UserData
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
                .SelectMany(g => g.M_Group_Menu);

            // 檢查是否有任何最高權限
            bool hasRootPrivilege = mGroupMenus
                .Where(mgm => mgm.AddFlag || mgm.DeleteFlag || mgm.EditFlag || mgm.PringFlag || mgm.ShowFlag)
                .Select(mgm => mgm.MD)
                .Any(md => md.ActiveFlag && !md.DeleteFlag && md.URL == PrivilegeConstants.RootAccessUrl);

            var finalQuery = mGroupMenus
                // 有具備任一種 flag 的最高權限時，或有具備任一種 flag 的權限才予以回傳
                // 這邊的 flag 條件要寫兩次，有兩個原因：
                // 1. 寫成函數會被 EF 當成記憶體函數執行
                // 2. 有可能有一般權限沒有 flag，但最高權限有 flag 的情況，提早篩就會把這類的 MenuData 篩掉
                .Where(mgm => hasRootPrivilege || mgm.AddFlag || mgm.DeleteFlag || mgm.EditFlag || mgm.PringFlag ||
                              mgm.ShowFlag)
                // MenuData
                .Select(mgm => mgm.MD);

            return finalQuery;
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

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<MenuData> GetInfoByIdQuery(int id)
        {
            return DC.MenuData.Where(md => md.MDID == id);
        }

        public async Task<MenuData_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(MenuData entity)
        {
            return await Task.FromResult(new MenuData_GetInfoById_Output_APIItem
            {
                MDID = entity.MDID,
                Title = entity.Title ?? "",
                URL = entity.URL ?? "",
                SortNo = entity.SortNo
            });
        }

        #endregion

        #region ChangeActive

        /// <summary>
        /// 更新單一選單的啟用 / 關閉狀態。設為關閉時，將連同下層選單一同設定。
        /// </summary>
        /// <param name="id">對象資料 ID</param>
        /// <param name="activeFlag">
        /// true：設為啟用<br/>
        /// false：設為關閉
        /// </param>
        /// <returns>通用回傳格式訊息</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            // 有特殊邏輯：關閉時，需要同步關閉下層所有選單，所以這裡不用 helper。
            await _changeActive(id, activeFlag);

            return GetResponseJson();
        }

        private async Task _changeActive(int id, bool? activeFlag)
        {
            // 1. 驗證輸入
            bool isValid = this.StartValidate()
                .Validate(_ => id.IsValidId(), () => AddError(EmptyNotAllowed("欲更新的預約 ID")))
                .Validate(_ => activeFlag != null, () => AddError(EmptyNotAllowed("ActiveFlag")))
                .IsValid();

            if (!isValid)
                return;

            // 2. 執行更新
            List<MenuData> menuData = await DC.MenuData
                .Where(md => md.ActiveFlag && !md.DeleteFlag)
                // 僅在關閉時才連同下層一同關閉
                .Where(md => md.MDID == id || activeFlag == false && md.ParentID == id)
                .ToListAsync();

            foreach (MenuData data in menuData)
            {
                data.ActiveFlag = activeFlag ?? throw new ArgumentNullException(nameof(activeFlag));
            }

            // 3. 寫入 DB
            try
            {
                await DC.SaveChangesWithLogAsync(GetUid());
            }
            catch (Exception e)
            {
                AddError(UpdateDbError(e));
            }
        }

        #endregion
    }
}