using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.UserDataController
{
    /// <summary>
    /// 處理操作紀錄 Log 的 API。<br/>
    /// 處理的是 UserLog，但因為目前開的 Route 為 UserData，因此還是歸類在 UserDataController，
    /// </summary>
    public class UserDataLogController : PublicClass
    {
        private static readonly string[] UserLogTypes = { "瀏覽", "新增", "修改", "刪除" };
        private static readonly string[] UserPasswordLogTypes = { "登入", "登出", "更改密碼" };

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(UserLog_GetList_Input_APIItem input)
        {
            // 這個功能同時需要使用 UserLog 和 UserPasswordLog，所以無法使用 Helper。
            // 1. 驗證輸入
            if (!await GetListPagedValidateInput(input))
                return GetResponseJson();

            // 2. 查詢資料並轉成回覆物件
            var result = (await GetListPagedResult(input)).ToArray();

            // 3. 回傳
            BaseResponseForPagedList<UserLog_GetList_Output_Row_APIItem> response =
                new BaseResponseForPagedList<UserLog_GetList_Output_Row_APIItem>();

            response.SetByInput(input);
            response.AllItemCt = result.Length;
            response.Items = result.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToArray();

            return GetResponseJson(response);
        }

        public async Task<bool> GetListPagedValidateInput(UserLog_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.NowPage.IsZeroOrAbove(), () => AddError(WrongFormat("查詢分頁")))
                .Validate(i => i.CutPage.IsZeroOrAbove(), () => AddError(WrongFormat("分頁筆數")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<IEnumerable<UserLog_GetList_Output_Row_APIItem>> GetListPagedResult(
            UserLog_GetList_Input_APIItem input)
        {
            DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var userLogTypeFilter = UserLogTypes
                .Where(s => s.Contains(input.EventType))
                .Select((s, i) => i);
            var userPasswordLogTypeFilter = UserPasswordLogTypes
                .Where(s => s.Contains(input.EventType))
                .Select((s, i) => i);

            var userLogs = (await DC.UserLog
                        .Include(ul => ul.UserData)
                        .Where(ul => ul.CreDate >= threeMonthsAgo)
                        // 不顯示瀏覽
                        .Where(ul => ul.ControlType != (int)UserLogControlType.Show)
                        .Where(ul => userLogTypeFilter.Contains(ul.ControlType))
                        .ToArrayAsync())
                    .Select(ul => new UserLog_GetList_Output_Row_APIItem
                    {
                        Time = ul.CreDate.ToFormattedStringDateTime(),
                        Actor = ul.UserData?.UserName ?? "",
                        EventType = UserLogTypes.Length > ul.ControlType
                            ? UserLogTypes[ul.ControlType]
                            : "",
                        Description = "使用者" +
                                      (UserLogTypes.Length > ul.ControlType
                                          ? UserLogTypes[ul.ControlType]
                                          : "操作") +
                                      TableCommentDictionary.GetCommentByTableName(ul.TargetTable),
                        CreDate = ul.CreDate
                    })
                ;

            var userPasswordLogs = (await DC.UserPasswordLog
                        .Include(upl => upl.UserData)
                        .Where(upl => upl.CreDate >= threeMonthsAgo)
                        .Where(upl => userPasswordLogTypeFilter.Contains(upl.Type))
                        .ToArrayAsync())
                    .Select(upl => new UserLog_GetList_Output_Row_APIItem
                    {
                        Time = upl.CreDate.ToFormattedStringDateTime(),
                        Actor = upl.UserData?.UserName ?? "",
                        EventType = UserPasswordLogTypes.Length > upl.Type
                            ? UserPasswordLogTypes[upl.Type]
                            : "",
                        Description = "使用者" +
                                      (UserPasswordLogTypes.Length > upl.Type
                                          ? UserPasswordLogTypes[upl.Type]
                                          : $"操作 PasswordLog, type: {upl.Type}"),
                        CreDate = upl.CreDate
                    })
                ;

            var result = userLogs.Concat(userPasswordLogs);

            // 由新到舊
            return result.OrderByDescending(i => i.CreDate);
        }

        #endregion
    }
}