using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetLogKeepDays;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetTypeList;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.SubmitLogKeepDays;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;
using WebGrease.Css.Extensions;

namespace NS_Education.Controller.UsingHelper.UserDataController
{
    /// <summary>
    /// 處理操作紀錄 Log 的 API。<br/>
    /// 處理的是 UserLog，但因為目前開的 Route 為 UserData，因此還是歸類在 UserDataController，
    /// </summary>
    public class UserDataLogController : PublicClass,
        IGetListLocal<UserLog_GetTypeList_Output_APIItem>
    {
        private static readonly string[] UserLogTypes = { "瀏覽", "新增", "修改", "刪除" };
        private static readonly string[] UserPasswordLogTypes = { "登入", "登出", "更改密碼" };

        private static readonly IList<UserLog_GetTypeList_Output_APIItem> typeList = UserLogTypes
            .Select((s, i) => new UserLog_GetTypeList_Output_APIItem
            {
                Title = s,
                UserLogType = i
            })
            .Concat(UserPasswordLogTypes.Select((s, i) => new UserLog_GetTypeList_Output_APIItem
            {
                Title = s,
                UserPasswordLogType = i
            })).ToSafeReadOnlyCollection();

        private readonly IGetListLocalHelper _getListLocalHelper;

        #region Initialization

        public UserDataLogController()
        {
            _getListLocalHelper =
                new GetListLocalHelper<UserDataLogController, UserLog_GetTypeList_Output_APIItem>(this);
        }

        #endregion

        #region SubmitLogKeepDays

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> SubmitLogKeepDays(UserLog_SubmitLogKeepDays_Input_APIItem input)
        {
            // 這支輸入沒有 ActiveFlag，所以不使用 Helper
            B_StaticCode data = await DC.B_StaticCode
                .Where(sc => sc.BSCID == DbConstants.SafetyControlLogKeepDaysBSCID)
                .FirstOrDefaultAsync();

            if (data is null)
            {
                AddError(NotFound());
                return GetResponseJson();
            }

            try
            {
                data.SortNo = input.KeepDays;
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }

            return GetResponseJson();
        }

        #endregion

        #region GetLogKeepDays

        /// <summary>
        /// 處理取得紀錄保留天數的設定值的端點，實際 Route 請參照 RouteConfig。
        /// </summary>
        /// <returns>紀錄保留天數（通用訊息回傳格式）</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetLogKeepDays()
        {
            // 無輸入，無法使用 helper。
            // 1. 查詢資料
            B_StaticCode keepDaysStaticCode =
                await DC.B_StaticCode.FirstOrDefaultAsync(sc => sc.BSCID == DbConstants.SafetyControlLogKeepDaysBSCID);

            if (keepDaysStaticCode == null)
            {
                AddError(NotFound());
                return GetResponseJson();
            }

            // 2. 轉成回傳物件，設值，回傳
            UserLog_GetLogKeepDays_Output_APIItem response = new UserLog_GetLogKeepDays_Output_APIItem
            {
                KeepDays = keepDaysStaticCode.SortNo
            };

            return GetResponseJson(response);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetUserLogList(UserLog_GetList_Input_APIItem input)
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

            // 有以下幾種情況：
            // |- a. UserLogType 不合法，UserPasswordLogType 不合法：不篩選，全查
            // |- b. UserLogType 合法，UserPasswordLogType 不合法：只查 UserLog 並套用篩選
            // |- c. UserLogType 不合法，UserPasswordLogType 合法：只查 UserPasswordLog 並套用篩選
            // +- d. UserLogType 合法，UserPasswordLogType 合法：兩個都查並套用篩選

            var userLogQuery = DC.UserLog
                .Include(ul => ul.UserData)
                .Where(ul => ul.CreDate >= threeMonthsAgo);

            bool isUserLogTypeValid = input.UserLogType.IsInBetween(0, 3);
            bool isUserPasswordLogTypeValid = input.UserPasswordLogType.IsInBetween(0, 2);

            if (isUserLogTypeValid)
                userLogQuery = userLogQuery.Where(ul => ul.ControlType == input.UserLogType);
            else if (isUserPasswordLogTypeValid)
                userLogQuery = userLogQuery.Take(0);

            var userPasswordLogQuery = DC.UserPasswordLog
                .Include(upl => upl.UserData)
                .Where(upl => upl.CreDate >= threeMonthsAgo);

            if (isUserPasswordLogTypeValid)
                userPasswordLogQuery = userPasswordLogQuery.Where(upl => upl.Type == input.UserPasswordLogType);
            else if (isUserLogTypeValid)
                userPasswordLogQuery = userPasswordLogQuery.Take(0);

            var userLogs = (await userLogQuery
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

            var userPasswordLogs = (await userPasswordLogQuery
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

            var result = userLogs
                .Concat(userPasswordLogs);

            if (input.Keyword.HasContent())
                result = result.Where(i =>
                    i.EventType.Contains(input.Keyword) || i.Description.Contains(input.Keyword));

            // 由新到舊
            return result.OrderByDescending(i => i.CreDate);
        }

        #endregion

        #region GetTypeList

        /// <summary>
        /// 提供「事件類型」的端點，確切 Route 請參考 RouteConfig。
        /// </summary>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            return await _getListLocalHelper.GetListLocal();
        }

        /// <inheritdoc />
        public async Task<ICollection<UserLog_GetTypeList_Output_APIItem>> GetListLocalResults()
        {
            return await Task.FromResult(typeList);
        }

        #endregion
    }
}