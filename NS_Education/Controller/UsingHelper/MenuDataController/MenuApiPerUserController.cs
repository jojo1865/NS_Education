using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.MenuData.MenuApi.GetListByUid;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.MenuDataController
{
    /// <summary>
    /// 處理單個用戶所擁有的子選單查詢的 API。<br/>
    /// 因為目前開的 Route 為 MenuData，因此還是歸類在 MenuDataController。
    /// </summary>
    public class MenuApiPerUserController : PublicClass
    {
        private static readonly string[] ApiTypes = { "瀏覽", "新增", "修改", "刪除", "匯出", "登入/登出" };

        #region GetList

        private bool UserHasRootAccess = false;

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.None)]
        public async Task<string> GetList()
        {
            // 1. 取得所有權限
            IOrderedQueryable<MenuData> query = GetListAllOrderedQuery();

            // 所有有權限的 MDID : MenuData
            IDictionary<int, MenuData> menuDataDict = await query.ToDictionaryAsync(md => md.MDID, md => md);

            // 加入需要特殊處理的 MDID
            MenuData[] specialMenus = await DC.MenuData
                .Where(md => md.ActiveFlag && !md.DeleteFlag)
                .Where(md =>
                    md.AlwaysAllowShow || md.AlwaysAllowAdd || md.AlwaysAllowEdit || md.AlwaysAllowDelete ||
                    md.AlwaysAllowPring)
                .ToArrayAsync();

            foreach (MenuData specialMenu in specialMenus)
            {
                menuDataDict[specialMenu.MDID] = specialMenu;
            }

            // ParentID : MenuData
            ILookup<int, MenuData> parentToChildrenLookUp =
                menuDataDict.ToLookup(kvp => kvp.Value.ParentID, kvp => kvp.Value);

            // 2. 建立回傳物件
            // |- a. 選擇所有 LookUp key，對應的所有 MenuData 即為父 MenuData
            // |- b. 第二層為 LookUp 中此父親的 Values (子 MenuData)
            // +- c. 第三層為各子 MenuData 的 MenuAPI

            // Response 物件
            CommonResponseForList<MenuData_GetListByUid_Output_Node_APIItem> response =
                new CommonResponseForList<MenuData_GetListByUid_Output_Node_APIItem>();

            // 遞迴產生 nodes 時的暫存區
            Dictionary<int, MenuData_GetListByUid_Output_Node_APIItem> createdNodes =
                new Dictionary<int, MenuData_GetListByUid_Output_Node_APIItem>();

            foreach (IGrouping<int, MenuData> grouping in parentToChildrenLookUp)
            {
                CreateNode(grouping.Key, menuDataDict, parentToChildrenLookUp, createdNodes);
            }

            // createdNodes 的 key 是所有人的 parentId
            // 所以 0 會有一筆空資料 (MDID = 0, 其他欄位為預設)
            // 而 createdNodes[0].Items 就是實際的樹
            response.Items = createdNodes[0].Items
                .OrderBy(item => item.SortNo)
                .ToList();

            return GetResponseJson(response);
        }

        [SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
        private MenuData_GetListByUid_Output_Node_APIItem CreateNode(int thisNodeId,
            IDictionary<int, MenuData> menuDataDict,
            ILookup<int, MenuData> parentToChildrenLookUp,
            IDictionary<int, MenuData_GetListByUid_Output_Node_APIItem> createdNodes)
        {
            if (createdNodes == null)
                createdNodes = new Dictionary<int, MenuData_GetListByUid_Output_Node_APIItem>();

            // Parent 可能是 null - 表示所擁有的權限中，沒有父層選單的權限，只有子層選單權限。
            menuDataDict.TryGetValue(thisNodeId, out MenuData thisNodeOrNull);

            var children = parentToChildrenLookUp.FirstOrDefault(g => g.Key == thisNodeId);

            // 如果 createdNodes 已經有這個 nodeId，表示已經建立過，回傳原有資料
            if (createdNodes.TryGetValue(thisNodeId,
                    out MenuData_GetListByUid_Output_Node_APIItem result))
                return result;

            int uid = GetUid();

            bool rootShow = false;
            bool rootAdd = false;
            bool rootEdit = false;
            bool rootDelete = false;
            bool rootPrint = false;

            if (UserHasRootAccess)
            {
                // aggregate 所有屬於這個 user 的 root privileges, 作為 boolean 判斷用
                UserData user = DC.UserData
                    .Include(ud => ud.M_Group_User)
                    .Include(ud => ud.M_Group_User.Select(mgu => mgu.GroupData))
                    .Include(ud => ud.M_Group_User.Select(mgu => mgu.GroupData.M_Group_Menu))
                    .Include(
                        ud => ud.M_Group_User.Select(mgu => mgu.GroupData.M_Group_Menu.Select(mgm => mgm.MenuData)))
                    .Single(ud => ud.ActiveFlag && !ud.DeleteFlag && ud.UID == uid);

                IEnumerable<M_Group_Menu> groupMenus = user.M_Group_User
                    .Select(mgu => mgu.GroupData)
                    .Where(g => g.ActiveFlag && !g.DeleteFlag)
                    .SelectMany(g => g.M_Group_Menu)
                    .Where(mgm => mgm.MenuData.ActiveFlag && !mgm.MenuData.DeleteFlag)
                    .Where(mgm => mgm.MenuData.URL == PrivilegeConstants.RootAccessUrl);

                foreach (var mgm in groupMenus)
                {
                    rootShow = rootShow || mgm.ShowFlag;
                    rootAdd = rootAdd || mgm.AddFlag;
                    rootEdit = rootEdit || mgm.EditFlag;
                    rootDelete = rootDelete || mgm.DeleteFlag;
                    rootPrint = rootPrint || mgm.PringFlag;
                }
            }

            bool isASpecialMenu = thisNodeOrNull != null
                                  && (thisNodeOrNull.AlwaysAllowShow
                                      || thisNodeOrNull.AlwaysAllowAdd
                                      || thisNodeOrNull.AlwaysAllowEdit
                                      || thisNodeOrNull.AlwaysAllowDelete
                                      || thisNodeOrNull.AlwaysAllowPring);

            // 建立新的 Node
            MenuData_GetListByUid_Output_Node_APIItem newNode = new MenuData_GetListByUid_Output_Node_APIItem
            {
                MDID = thisNodeId,
                Title = thisNodeOrNull?.Title ?? "",
                URL = thisNodeOrNull?.URL.HasContent() ?? false
                    ? thisNodeOrNull.URL
                    : children?
                          .OrderBy(child => child.SortNo)
                          .Select(child => child.URL)
                          .FirstOrDefault()
                      ?? "",
                SortNo = thisNodeOrNull?.SortNo ?? 0,
                // 這裡在做遞迴
                Items = children?.Select(child =>
                            CreateNode(child.MDID, menuDataDict, parentToChildrenLookUp, createdNodes)).ToList() ??
                        new List<MenuData_GetListByUid_Output_Node_APIItem>(),
                Apis = thisNodeOrNull?.MenuAPI
                           .Where(api =>
                               // 如果是特殊的選單（如預約管理），必定有瀏覽新增修改
                               (isASpecialMenu
                                && (api.APIType != (int)MenuApiType.Show || thisNodeOrNull.AlwaysAllowShow)
                                && (api.APIType != (int)MenuApiType.Add || thisNodeOrNull.AlwaysAllowAdd)
                                && (api.APIType != (int)MenuApiType.Edit || thisNodeOrNull.AlwaysAllowEdit)
                                && (api.APIType != (int)MenuApiType.Delete || thisNodeOrNull.AlwaysAllowDelete)
                                && (api.APIType != (int)MenuApiType.Print || thisNodeOrNull.AlwaysAllowPring))
                               ||
                               // 判斷管理員權限
                               (UserHasRootAccess && (api.APIType != (int)MenuApiType.Add || rootAdd)
                                                  && (api.APIType != (int)MenuApiType.Delete || rootDelete)
                                                  && (api.APIType != (int)MenuApiType.Edit || rootEdit)
                                                  && (api.APIType != (int)MenuApiType.Print || rootPrint)
                                                  && (api.APIType != (int)MenuApiType.Show || rootShow))
                               ||
                               // 如果沒有管理員權限,再用 MenuData 回溯到 M_Group_Menu 確認權限
                               (
                                   api.MenuData.ActiveFlag
                                   && !api.MenuData.DeleteFlag
                                   && api.MenuData.M_Group_Menu.Any(mgm =>
                                       mgm.GroupData.ActiveFlag
                                       && !mgm.GroupData.DeleteFlag
                                       && mgm.GroupData.M_Group_User.Any(mgu => mgu.UID == uid)
                                       && (api.APIType != (int)MenuApiType.Add || mgm.AddFlag)
                                       && (api.APIType != (int)MenuApiType.Delete || mgm.DeleteFlag)
                                       && (api.APIType != (int)MenuApiType.Edit || mgm.EditFlag)
                                       && (api.APIType != (int)MenuApiType.Print || mgm.PringFlag)
                                       && (api.APIType != (int)MenuApiType.Show || mgm.ShowFlag)
                                   )
                               )
                           )
                           .Select(menuApi => new MenuData_GetListByUid_Output_MenuApi_APIItem
                           {
                               ApiUrl = menuApi.APIURL ?? "",
                               ApiType = menuApi.APIType < ApiTypes.Length
                                   ? ApiTypes[menuApi.APIType]
                                   : ""
                           }).ToList()
                       ?? new List<MenuData_GetListByUid_Output_MenuApi_APIItem>()
            };

            // 綁定父層關係
            foreach (var childNode in newNode.Items)
            {
                childNode.Parent = newNode;
            }

            // 放到 createdNodes
            createdNodes.Add(thisNodeId, newNode);

            return newNode;
        }

        private IOrderedQueryable<MenuData> GetListAllOrderedQuery()
        {
            // 如果是 Admin 或擁有 / 權限的人，回傳所有選單。否則才進行篩選
            int uid = GetUid();
            var menuData = DC.UserData
                .Include(ud => ud.M_Group_User)
                .Include(ud => ud.M_Group_User.Select(mgu => mgu.GroupData))
                .Include(ud => ud.M_Group_User.Select(mgu => mgu.GroupData.M_Group_Menu))
                .Include(ud => ud.M_Group_User.Select(mgu => mgu.GroupData.M_Group_Menu.Select(mgm => mgm.MenuData)))
                // UserData
                .Where(ud => ud.ActiveFlag && !ud.DeleteFlag)
                .Where(ud => ud.UID == uid)
                .SelectMany(ud => ud.M_Group_User)
                // Group
                .Select(mgu => mgu.GroupData)
                .Where(g => g.ActiveFlag && !g.DeleteFlag)
                // M_Group_Menu
                .SelectMany(g => g.M_Group_Menu)
                // 初步篩選有任何 flag 才列入考量，API type 對照 flag 的部分之後在記憶體做
                .Where(mgm => mgm.AddFlag || mgm.DeleteFlag || mgm.EditFlag || mgm.PringFlag || mgm.ShowFlag)
                // MenuData
                .Select(mgm => mgm.MenuData)
                .Where(md => md.ActiveFlag && !md.DeleteFlag);

            UserHasRootAccess = menuData.Any(md => md.URL == PrivilegeConstants.RootAccessUrl);

            var query = UserHasRootAccess
                ? DC.MenuData.AsQueryable()
                : menuData;

            query = query.Include(md => md.MenuAPI)
                .Include(md => md.MenuAPI.Select(api => api.MenuData))
                .Include(md => md.MenuAPI.Select(api => api.MenuData.M_Group_Menu))
                .Include(md => md.MenuAPI.Select(api => api.MenuData.M_Group_Menu.Select(mgm => mgm.GroupData)))
                .Include(md => md.MenuAPI.Select(api =>
                    api.MenuData.M_Group_Menu.Select(mgm => mgm.GroupData).Select(gd => gd.M_Group_User)));

            return query.OrderBy(md => md.SortNo)
                .ThenBy(md => md.URL)
                .ThenBy(md => md.Title)
                .ThenBy(md => md.MDID);
        }

        #endregion
    }
}