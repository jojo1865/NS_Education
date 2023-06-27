using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.SafetyControl.GetList;
using NS_Education.Models.APIItems.Controller.SafetyControl.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
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
        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> Submit(SafetyControl_Submit_Input_APIItem input)
        {
            // 這個 API 需要一次更新所有安全控管設定，因此不使用 Helper

            // 1. 驗證輸入
            input.StartValidate()
                .Validate(i => i.PasswordMinLength.IsAboveZero(),
                    () => AddError(OutOfRange("密碼的最小長度", 1)))
                .Validate(i => i.PasswordChangeDailyLimit.IsZeroOrAbove(),
                    () => AddError(OutOfRange("一天最多可更改幾次密碼", 0)))
                .Validate(i => i.PasswordNoReuseCount.IsZeroOrAbove(),
                    () => AddError(OutOfRange("更新密碼時必須與前面多少次的使用密碼不同", 0)))
                .Validate(i => i.PasswordExpireDays.IsAboveZero(),
                    () => AddError(OutOfRange("更換密碼後，密碼有效天數", 1)))
                .Validate(i => i.LoginFailLimit.IsAboveZero(),
                    () => AddError(OutOfRange("密碼輸入多少次錯誤時，系統會自動結束", 1)))
                .Validate(i => i.WarnChangePasswordInDays.IsZeroOrAbove(),
                    () => AddError(OutOfRange("密碼更換到期日警告訊息在幾天前開始顯示", 0)))
                .Validate(i => i.IdleSecondsBeforeScreenSaver >= 10,
                    () => AddError(OutOfRange("帳號登入系統超過多少秒沒動作時，系統將自動進入保護程式", 10)));

            // 2. 查資料
            var staticCodes = await GetStaticCodesAndValidateCount();

            if (HasError())
                return GetResponseJson();

            // 3. 設值
            // 避免 enum 和 request 格式耦合, 這裡不用 enum.GetValues 然後直接用 Property names SetValue
            staticCodes[(int)StaticCodeSafetyControlCode.PasswordMinLength].SortNo = input.PasswordMinLength;
            staticCodes[(int)StaticCodeSafetyControlCode.PasswordChangeDailyLimit].SortNo =
                input.PasswordChangeDailyLimit;
            staticCodes[(int)StaticCodeSafetyControlCode.PasswordNoReuseCount].SortNo = input.PasswordNoReuseCount;
            staticCodes[(int)StaticCodeSafetyControlCode.PasswordExpireDays].SortNo = input.PasswordExpireDays;
            staticCodes[(int)StaticCodeSafetyControlCode.SuspendIfLoginFailTooMuch].SortNo =
                input.SuspendIfLoginFailTooMuch ? 1 : 0;
            staticCodes[(int)StaticCodeSafetyControlCode.WarnChangePasswordInDays].SortNo =
                input.WarnChangePasswordInDays;
            staticCodes[(int)StaticCodeSafetyControlCode.IdleSecondsBeforeScreenSaver].SortNo =
                input.IdleSecondsBeforeScreenSaver;
            staticCodes[(int)StaticCodeSafetyControlCode.EnforceOneSessionPerUser].SortNo =
                input.EnforceOneSessionPerUser ? 1 : 0;
            staticCodes[(int)StaticCodeSafetyControlCode.NewSessionTerminatesOld].SortNo =
                input.NewSessionTerminatesOld ? 1 : 0;
            staticCodes[(int)StaticCodeSafetyControlCode.PersistSecurityControlErrors].SortNo =
                input.PersistSecurityControlErrors ? 1 : 0;
            staticCodes[(int)StaticCodeSafetyControlCode.LoginFailLimit].SortNo = input.LoginFailLimit;

            // 4. 儲存
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }

            return GetResponseJson();
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            // 這個 API 雖然叫做 GetList，但實際上最後只組成單一物件，每個欄位代表一種安全參數設定。
            // 所以，這裡不使用 Helper。

            // 取得所有安全控管設定
            // 這個方法會順便驗證，所以如果回來之後有錯誤，可以直接返回
            IDictionary<int, B_StaticCode> staticCodes = await GetStaticCodesAndValidateCount();

            if (HasError())
                return GetResponseJson();

            // 3. 設值回傳
            // 避免 enum 和 response 格式耦合, 這裡不用 enum.GetValues 然後直接用 Property names SetValue
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
                    staticCodes[(int)StaticCodeSafetyControlCode.IdleSecondsBeforeScreenSaver].SortNo,
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

        private async Task<IDictionary<int, B_StaticCode>> GetStaticCodesAndValidateCount()
        {
            // 整理出後端已知的所有代碼
            HashSet<string> knownCodes = Enum.GetValues(typeof(StaticCodeSafetyControlCode))
                .Cast<StaticCodeSafetyControlCode>()
                .Select(codeEnum => ((int)codeEnum).ToString())
                .ToHashSet();

            // 1. 查詢資料
            var result = await DC.B_StaticCode
                .Where(bsc => bsc.CodeType == (int)StaticCodeType.SafetyControl)
                .Where(bsc => !bsc.DeleteFlag)
                .Where(bsc => knownCodes.Contains(bsc.Code))
                .ToDictionaryAsync(bsc => int.Parse(bsc.Code), bsc => bsc);

            // 2. 驗證數量相符
            if (result.Keys.Count != knownCodes.Count)
            {
                AddError(1,
                    $"後端約定的安全控管參數數量（{knownCodes.Count}）與資料庫未刪除的安全控管參數數量（{result.Keys.Count}）不符！請確認資料庫是否有重複資料，或刪除了既有的安全參數設定！");
            }

            return result;
        }

        #endregion
    }
}