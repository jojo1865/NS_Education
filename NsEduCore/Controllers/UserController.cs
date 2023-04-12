using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NsEduCore.Controllers.Messages;
using NsEduCore.Requests;
using NsEduCore.Requests.User;
using NsEduCore.Responses.User;
using NsEduCore.Variables;
using NsEduCore_DAL.Services.User;
using NsEduCore_Tools.BeingValidated;
using NsEduCore_Tools.Encryption;
using NsEduCore_Tools.Extensions;

namespace NsEduCore.Controllers
{
    /// <summary>
    /// 使用者資料相關的 Controller。
    /// </summary>
    [Route("/UserData/[action]")]
    public class UserController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IJwtHelper _jwtHelper;

        /// <summary>
        /// 建立 UserController。
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        /// <param name="userService">IUserService</param>
        /// <param name="jwtHelper">IJwtHelper</param>
        public UserController(IConfiguration configuration, IUserService userService, IJwtHelper jwtHelper)
        {
            _configuration = configuration;
            _userService = userService;
            _jwtHelper = jwtHelper;
        }

        #region Login
        /// <summary>
        /// 登入。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：JWT Token 與使用者名稱。<br/>
        /// 失敗時：通用回傳訊息格式。
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginRequest input)
        {
            // 1. 確認帳號輸入無誤
            if (input.LoginAccount.IsNullOrWhitespace())
                AddError(UserControllerMessages.LoginAccountNotFound);

            // 2. 查詢資料
            User queried = await _userService.SelectByLoginAccount(input.LoginAccount);

            // 3. 驗證
            // |- a. 確實有此帳號
            // |- b. 確認帳號啟用與刪除狀態
            // |- c. 驗證登入密碼
            // +- d. 更新使用者上次登入時間
            bool isValid = await queried.StartValidate(true)
                .Validate(q => q != null, () => AddError(UserControllerMessages.LoginAccountNotFound))
                .Validate(q => q.ActiveFlag && !q.DeleteFlag,
                    () => AddError(UserControllerMessages.LoginAccountNotFound))
                .ValidateAsync(async q => await _userService.ValidateLoginPassword(q, input.LoginPassword),
                    () => AddError(UserControllerMessages.LoginPasswordIncorrect))
                .ValidateAsync(UpdateUserLoginDate)
                .IsValid();

            if (!isValid)
                return Ok(GetReturnMessage());

            // 4. 建立 JWT
            string jwtToken;

            try
            {
                jwtToken = _jwtHelper.GenerateToken(_configuration.GetSection(JwtConstants.Key)?.Value,
                    JwtConstants.ExpireMinutes,
                    new Claim[]
                    {
                        new("uid", queried.UID.ToString())
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AddError("登入資料正確，但產生 JWT Token 時失敗，請確認程式 Log！");
                return Ok(GetReturnMessage());
            }

            // 5. 回傳
            UserLoginResponseAbstract responseAbstract = new()
            {
                Username = queried.UserName,
                JwtToken = jwtToken 
            };
            
            return Ok(GetReturnMessage(responseAbstract));
        }

        /// <summary>
        /// 更新使用者的登入日期時間。
        /// </summary>
        /// <param name="user">使用者資料</param>
        /// <returns>
        /// true：更新成功。<br/>
        /// false：更新失敗。
        /// </returns>
        private async Task<bool> UpdateUserLoginDate(User user)
        {
            bool result = true;
            
            try
            {
                await _userService.UpdateLoginDate(user);
            }
            catch (Exception e)
            {
                AddError(UserControllerMessages.LoginDateUpdateFailed(e));
                result = false;
            }

            return result;
        }
        #endregion

        [HttpGet]
        [Authorize]
        public IActionResult TestJWT()
        {
            return Ok();
        }
    }
}