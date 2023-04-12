using System.Threading.Tasks;

namespace NsEduCore_DAL.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// 透過使用者帳號，查詢使用者。
        /// </summary>
        /// <param name="LoggingAccount">輸入帳號</param>
        /// <returns>查詢結果。有可能為 null。</returns>
        Task<User> SelectByLoginAccount(string loggingAccount);

        /// <summary>
        /// 驗證登入密碼是否符合一筆使用者資料。
        /// </summary>
        /// <param name="user">驗證對象的使用者資料</param>
        /// <param name="inputPassword">輸入密碼</param>
        /// <returns>
        /// true：符合。<br/>
        /// false：不符合
        /// </returns>
        Task<bool> ValidateLoginPassword(User user, string inputPassword);
    }
}