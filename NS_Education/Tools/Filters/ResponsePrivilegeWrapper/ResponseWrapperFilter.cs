using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using NS_Education.Models.Entities;
using NS_Education.Variables;

namespace NS_Education.Tools.Filters.ResponsePrivilegeWrapper
{
    /// <summary>
    /// 在 Action 執行完之後，依據 JWT Token Claims 的 UID 與現在的選單，查詢所有子目錄節點權限，在 Response 外面多包一層後回傳。<br/>
    /// 同時，把 Response 轉成 200，列入欄位表示 Status 和錯誤訊息。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ResponseWrapperFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            WrapResponse(filterContext);

            base.OnActionExecuted(filterContext);
        }

        private static void WrapResponse(ActionExecutedContext filterContext)
        {
            ClaimsPrincipal claims;
            try
            {
                JwtAuthFilter.JwtAuthFilter.ValidateTokenDecryptable(filterContext, JwtConstants.Secret, out claims);
            }
            catch {
                // 解密失敗或取不到 JWT 時，不回傳任何權限資訊
                WrapResponse(filterContext, null);
                return;
            }

            db_NS_EducationEntities context = new db_NS_EducationEntities();
            // Query 1: 找出目前所在的 MenuData 底下所有 MenuAPI
            string contextUri = FilterStaticTools.GetContextUri(filterContext);
            var menuApis = context.MenuData
                .Include(menuData => menuData.MenuAPI)
                .Include(menuData => menuData.MenuAPI.Select(api => api.MenuData))
                .Where(mainMenu => mainMenu.ActiveFlag
                                   && !mainMenu.DeleteFlag
                                   && contextUri.Contains(mainMenu.URL))
                .SelectMany(mainMenu => mainMenu.MenuAPI)
                ;

            // Query 2: 將 menuAPI 對應回 m_group_menu, 確認 user 有無權限
            int uidInClaim = FilterStaticTools.GetUidInClaimInt(claims);
            var queryResult  = context.M_Group_Menu
                .Include(groupMenu => groupMenu.GroupData)
                .Include(groupMenu => groupMenu.GroupData.M_Group_User)
                .Include(groupMenu => groupMenu.GroupData.M_Group_User.Select(mgu => mgu.UserData))
                .Include(groupMenu => groupMenu.MenuData)
                // 選出所有此 user 擁有權限的 groupMenu
                .Where(groupMenu
                    => groupMenu.GroupData.M_Group_User
                        .Select(groupUser => groupUser.UserData)
                        .Any(user => user.ActiveFlag
                                     && !user.DeleteFlag
                                     && user.UID == uidInClaim)
                )
                .Join(menuApis,
                    groupMenu => groupMenu.MenuData.URL,
                    menuApi => menuApi.MenuData.URL,
                    (groupMenu, menuApi) => new Privilege
                    {
                        Url = menuApi.APIURL, 
                        ShowFlag = groupMenu.ShowFlag,
                        AddFlag = groupMenu.AddFlag,
                        EditFlag = groupMenu.EditFlag,
                        DeleteFlag = groupMenu.DeleteFlag,
                        PrintFlag = groupMenu.PringFlag
                    })
                .ToList();
            
            // 特殊處理：
            // 當擁有包含 / 為 URL 的權限時，讓所有其他權限也考量此權限的 flags
            var rootPrivileges = queryResult
                .Where(p => p.Url == PrivilegeConstants.RootAccessUrl)
            // 整合此 user 所有 / 為 URL 的權限
                .GroupBy(p => p.Url)
                .Select(g => new Privilege
                {
                    Url = g.Key,
                    ShowFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.ShowFlag),
                    AddFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.AddFlag),
                    EditFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.EditFlag),
                    DeleteFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.DeleteFlag),
                    PrintFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.PrintFlag)
                })
                // 只會有一筆
                .FirstOrDefault();            
            
            // 以 URL 為單位，做 grouping 並彙整此 user 所有 group 的權限
            // 意思是只要使用者所屬的任一 group 有權限就是有權限
            // 在最終回傳給前端的回覆中，避免包含最高權限
            var privileges = queryResult
                .Where(p => p.Url != PrivilegeConstants.RootAccessUrl)
                .GroupBy(privilege => privilege.Url)
                .Select(g => new Privilege
                {
                    Url = g.Key,
                    ShowFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.ShowFlag || (rootPrivileges?.ShowFlag ?? false)),
                    AddFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.AddFlag || (rootPrivileges?.AddFlag ?? false)),
                    EditFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.EditFlag || (rootPrivileges?.EditFlag ?? false)),
                    DeleteFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.DeleteFlag || (rootPrivileges?.DeleteFlag ?? false)),
                    PrintFlag = g.Aggregate(false,
                        (acc, curr) => acc || curr.PrintFlag || (rootPrivileges?.PrintFlag ?? false)),
                });

            // 和 Response wrap
            WrapResponse(filterContext, privileges);
        }

        private static void WrapResponse(ActionExecutedContext filterContext, IEnumerable<Privilege> privileges)
        {
            // 取得此次 action 完整的 HTTP Response 並轉成 JObject
            filterContext.Result = RequestHelper.CreateWrappedResponse(filterContext, filterContext.Result, privileges);
        }
    }
}