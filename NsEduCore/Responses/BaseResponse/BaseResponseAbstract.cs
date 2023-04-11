using System;

namespace NsEduCore.Responses.BaseResponse
{
    /// <summary>
    /// 通用訊息回傳格式的基礎型態。
    /// </summary>
    public abstract class BaseResponseAbstract
    {
        /// <summary>
        /// 此次要求是否成功。<br/>
        /// true：成功。<br/>
        /// false：有出錯。<br/>
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 此次要求的錯誤訊息。無任何錯誤時為空字串。
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 設定此物件的通用回傳訊息。
        /// </summary>
        /// <param name="success">此次要求是否成功。</param>
        /// <param name="message">此次要求的錯誤訊息。（可選）</param>
        internal void SetBaseMessage(bool success, string message = null)
        {
            Success = success;
            Message = message ?? String.Empty;
        }
    }
}