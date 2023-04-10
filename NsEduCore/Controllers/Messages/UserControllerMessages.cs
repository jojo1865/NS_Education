using System;

namespace NsEduCore.Controllers.Messages
{
    public static class UserControllerMessages
    {
        #region 錯誤訊息 - 通用
        /// <summary>
        /// 回傳「請填入{columnName}！」
        /// </summary>
        /// <param name="columnName">欄位名稱</param>
        /// <returns>錯誤訊息字串</returns>
        internal static string EmptyNotAllowed(string columnName)
            => $"請填入{columnName}！";
        #endregion
        
        #region 錯誤訊息 - 註冊/更新
        internal const string SubmitOriginalPasswordEmptyOrIncorrect = "原密碼未輸入或不正確！";
        internal const string SubmitPasswordAlphanumericOnly = "使用者密碼只允許半形英文字母、數字！";
        internal const string SubmitUidIncorrect = "缺少 UID，無法寫入！";
        #endregion

        #region 錯誤訊息 - 登入
        internal const string LoginAccountNotFound = "查無此使用者帳號，請重新確認！";
        internal const string LoginPasswordIncorrect = "使用者密碼錯誤！";
        internal static string LoginDateUpdateFailed(Exception e)
            => $"上次登入時間更新失敗，錯誤訊息：{e.Message}！";
        #endregion

        #region 錯誤訊息 - 刪除

        internal const string DeleteItemOperatorUidIncorrect = "未提供操作者的 UID，無法寫入！";
        internal const string DeleteItemTargetUidIncorrect = "未提供欲刪除的 UID 或格式不正確！";
        internal const string DeleteItemTargetUidNotFound = "查無對應的欲刪除 UID，請檢查輸入是否正確！";
        internal const string DeleteItemTargetAlreadyDeleted = "指定使用者已為刪除狀態！";
        internal static string DeleteItemFailed(Exception e)
            => $"刪除使用者時失敗，錯誤訊息：{e.Message}！";
        
        #endregion
    }
}