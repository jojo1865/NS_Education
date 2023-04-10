using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NsEduCore.Controllers.Messages;
using NsEduCore.Requests;
using NsEduCore_DAL.Services.User;
using NsEduCore_Tools.BeingValidated;
using NsEduCore_Tools.Extensions;

namespace NsEduCore.Controllers
{
    public class UserController : BaseController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginRequest input)
        {
            // 1. 確認帳號輸入無誤
            if (input.LoginAccount.IsNullOrWhitespace())
                AddError(UserControllerMessages.LoginAccountNotFound);

            // 2. 查詢資料
            User queried = await _userService.SelectByLoginAccount(input.LoginAccount);

            // 3. 驗證
            // a. 確實有此帳號
            // b. 確認帳號啟用與刪除狀態
            // c. 驗證登入密碼
            // d. 更新使用者上次登入時間
            bool isValid = await queried.StartValidate(true)
                .Validate(q => q != null, () => AddError(UserControllerMessages.LoginAccountNotFound))
                .Validate(q => q.ActiveFlag && !q.DeleteFlag,
                    () => AddError(UserControllerMessages.LoginAccountNotFound))
                .ValidateAsync(async q => await _userService.ValidateLoginPassword(q, input.LoginPassword),
                    () => AddError(UserControllerMessages.LoginPasswordIncorrect))
                .ValidateAsync(UpdateUserLoginDate)
                .IsValid();
            
            // 4. 建立並回傳 JWT
            await Task.Run(() => Console.WriteLine("Hello World!"));
            return Ok();
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
    }
}