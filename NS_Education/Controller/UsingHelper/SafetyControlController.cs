using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.SafetyControl.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper
{
    /// <summary>
    /// 處理安全控管設定的 Controller。
    /// </summary>
    public class SafetyControlController : PublicClass
    {
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            // 這個 API 雖然叫做 GetList，但實際上最後只組成單一物件，每個欄位代表一種安全參數設定。
            // 所以，這裡不使用 Helper。

            // 整理出後端已知的所有代碼
            HashSet<string> knownCodes = Enum.GetValues(typeof(StaticCodeSafetyControlCode))
                .Cast<StaticCodeSafetyControlCode>()
                .Select(codeEnum => ((int)codeEnum).ToString())
                .ToHashSet();

            // 1. 查詢資料
            IDictionary<int, B_StaticCode> staticCodes = await DC.B_StaticCode
                .Where(bsc => bsc.CodeType == (int)StaticCodeType.SafetyControl)
                .Where(bsc => !bsc.DeleteFlag)
                .Where(bsc => knownCodes.Contains(bsc.Code))
                .ToDictionaryAsync(bsc => int.Parse(bsc.Code), bsc => bsc);

            // 2. 驗證數量相符
            if (staticCodes.Keys.Count != knownCodes.Count)
            {
                AddError(
                    $"後端約定的安全控管參數數量（{knownCodes.Count}）與資料庫未刪除的安全控管參數數量（{staticCodes.Keys.Count}）不符！請確認資料庫是否有重複資料，或刪除了既有的安全參數設定！");
                return GetResponseJson();
            }

            // 3. 設值回傳
            SafetyControl_GetList_Output_APIItem response = new SafetyControl_GetList_Output_APIItem
            {
                PasswordMinLength = staticCodes[(int)StaticCodeSafetyControlCode.PasswordMinLength].SortNo,
                PasswordChangeDailyLimit =
                    staticCodes[(int)StaticCodeSafetyControlCode.PasswordChangeDailyLimit].SortNo,
                PasswordNoReuseCount = staticCodes[(int)StaticCodeSafetyControlCode.PasswordNoReuseCount].SortNo,
                PasswordExpireDays = staticCodes[(int)StaticCodeSafetyControlCode.PasswordExpireDays].SortNo,
                SuspendIfLoginFailTooMuch =
                    staticCodes[(int)StaticCodeSafetyControlCode.SuspendIfLoginFailTooMuch].SortNo == 1,
                WarnChangePasswordInDays =
                    staticCodes[(int)StaticCodeSafetyControlCode.WarnChangePasswordInDays].SortNo,
                IdleSecondsBeforeScreenSaver =
                    staticCodes[(int)StaticCodeSafetyControlCode.IdlSecondsBeforeScreenSaver].SortNo,
                EnforceOneSessionPerUser =
                    staticCodes[(int)StaticCodeSafetyControlCode.EnforceOneSessionPerUser].SortNo == 1,
                NewSessionTerminatesOld =
                    staticCodes[(int)StaticCodeSafetyControlCode.NewSessionTerminatesOld].SortNo == 1,
                PersistSecurityControlErrors =
                    staticCodes[(int)StaticCodeSafetyControlCode.PersistSecurityControlErrors].SortNo == 1,
                LoginFailLimit = staticCodes[(int)StaticCodeSafetyControlCode.LoginFailLimit].SortNo
            };

            return GetResponseJson(response);
        }

        #endregion
    }
}