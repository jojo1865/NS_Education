using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NS_Education.Models.Entities.DbContext;
using NS_Education.Variables;

namespace NS_Education.Tools.Filters.ResponsePrivilegeWrapper
{
    /// <summary>
    /// 在此 Action 執行完之後，依據 JWT Token Claims 的 UID 與現在的選單，查詢所有子目錄節點權限，在 Response 外面多包一層後回傳。
    /// </summary>
    public class ResponsePrivilegeWrapperFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // 1. 確定是 200, 否則提早返回
            if (filterContext.HttpContext.Response.StatusCode == 200)
            {
                WrapResponseWithPrivilege(filterContext);
            }

            base.OnActionExecuted(filterContext);
        }

        private static void WrapResponseWithPrivilege(ActionExecutedContext filterContext)
        {
            ClaimsPrincipal claims = null;
            try
            {
                JwtAuthFilter.JwtAuthFilter.ValidateToken(filterContext, JwtConstants.Secret,
                    out claims);
            }
            catch (Exception e)
            {
                filterContext.Result = new HttpUnauthorizedResult($"JWT 驗證失敗。{e.Message}");
            }

            if (claims == null) return;
            
            NsDbContext context = new NsDbContext();
            // Query 1: 找出目前所在的 MenuData 底下所有 MenuAPI
            var menuApis = context.MenuData
                .Include(menuData => menuData.MenuAPI)
                .Where(mainMenu => mainMenu.ActiveFlag
                                   && !mainMenu.DeleteFlag
                                   && FilterStaticTools.GetContextUri(filterContext)
                                       .Contains(mainMenu.URL))
                .SelectMany(mainMenu => mainMenu.MenuAPI);

            // Query 2: 將 menuAPI 對應回 m_group_menu, 確認 user 有無權限
            var queryResult  = context.M_Group_Menu
                .Include(groupMenu => groupMenu.G)
                .ThenInclude(group => @group.M_Group_User)
                .ThenInclude(groupUser => groupUser.U)
                .Include(groupMenu => groupMenu.MD)
                // 選出所有此 user 擁有權限的 groupMenu
                .Where(groupMenu
                    => groupMenu.G.M_Group_User
                        .Select(groupUser => groupUser.U)
                        .Any(user => user.ActiveFlag
                                     && !user.DeleteFlag
                                     && user.UID == FilterStaticTools.GetUidInClaimInt(claims))
                )
                .Join(menuApis,
                    groupMenu => groupMenu.MD.URL,
                    menuApi => menuApi.APIURL,
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
                
            // 以 URL 為單位，做 grouping 並彙整此 user 所有 group 的權限
            // 意思是只要使用者所屬的任一 group 有權限就是有權限
            var privileges = queryResult.GroupBy(privilege => privilege.Url)
                .Select(g => new Privilege
                {
                    Url = g.Key,
                    ShowFlag = g.Aggregate(false, (acc, curr) => acc || curr.ShowFlag),
                    AddFlag = g.Aggregate(false, (acc, curr) => acc || curr.AddFlag),
                    EditFlag = g.Aggregate(false, (acc, curr) => acc || curr.EditFlag),
                    DeleteFlag = g.Aggregate(false, (acc, curr) => acc || curr.DeleteFlag),
                    PrintFlag = g.Aggregate(false, (acc, curr) => acc || curr.PrintFlag),
                });
                    
            // 2. 和 Response wrap
            WrapResponse(filterContext, privileges);
        }

        private static void WrapResponse(ActionExecutedContext filterContext, IEnumerable<Privilege> privileges)
        {
            string responseJson = JsonConvert.SerializeObject(filterContext.Result);

            Console.WriteLine(responseJson);

            JObject modify = JsonConvert.DeserializeObject<JObject>(responseJson);

            modify["Content"] = JToken.FromObject(new
                {
                    ApiResponse = modify["Content"],
                    Privileges = privileges
                }
            );
            
            Console.WriteLine(JsonConvert.SerializeObject(modify));
        }
    }
}