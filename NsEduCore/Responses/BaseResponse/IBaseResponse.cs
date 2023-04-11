namespace NsEduCore.Responses.BaseResponse
{
    public interface IBaseResponse
    {
        /// <summary>
        /// 設定此物件的通用回傳訊息。
        /// </summary>
        /// <param name="success">此次要求是否成功。</param>
        /// <param name="message">此次要求的錯誤訊息。（可選）</param>
        void SetBaseMessage(bool success, string message = null);
    }
}