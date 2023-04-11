using NsEduCore.Responses.cReturnMessage;

namespace NsEduCore.Responses.User
{
    /// <summary>
    /// 使用者登入的回傳物件。
    /// </summary>
    public class UserLoginResponse : cReturnMessageInfusableAbstract
    {
        /// <summary>
        /// 使用者名稱。
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// JWT Token。
        /// </summary>
        public string JwtToken { get; set; }
    }
}