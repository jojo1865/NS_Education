namespace NsEduCore.Requests.User
{
    /// <summary>
    /// 登入的要求物件。
    /// </summary>
    public class UserLoginRequest
    {
        /// <summary>
        /// 使用者的登入帳號。
        /// </summary>
        public string LoginAccount { get; set; }
        
        /// <summary>
        /// 使用者的登入密碼。
        /// </summary>
        public string LoginPassword { get; set; }
    }
}