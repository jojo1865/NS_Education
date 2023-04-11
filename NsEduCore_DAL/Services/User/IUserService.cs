using System.Threading.Tasks;

namespace NsEduCore_DAL.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// 透過使用者帳號，查詢使用者。
        /// </summary>
        /// <param name="loggingAccount">輸入帳號</param>
        /// <returns>查詢結果。有可能為 null。</returns>
        Task<Domains.User> SelectByLoginAccount(string loggingAccount);

        /// <summary>
        /// 驗證登入密碼是否符合一筆使用者資料。
        /// </summary>
        /// <param name="user">驗證對象的使用者資料</param>
        /// <param name="inputPassword">輸入密碼</param>
        /// <returns>
        /// true：符合。<br/>
        /// false：不符合
        /// </returns>
        Task<bool> ValidateLoginPassword(Domains.User user, string inputPassword);

        /// <summary>
        /// 更新使用者的上次登入時間至呼叫當下。
        /// </summary>
        /// <param name="user">使用者</param>
        Task UpdateLoginDate(Domains.User user);

        /// <summary>
        /// 依據輸入的 UID，查詢對應使用者資料，並回傳使用者名稱。
        /// </summary>
        /// <param name="uid">對象 UID</param>
        /// <returns>使用者名稱。查無資料或格式不正確時，回傳 null。</returns>
        string GetUsername(int uid);
    }
}