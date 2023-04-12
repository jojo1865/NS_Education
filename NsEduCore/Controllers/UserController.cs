using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NsEduCore.Requests;
using NsEduCore_DAL.Models.Data;

namespace NsEduCore.Controllers
{
    public class UserController : BaseController
    {
        private readonly NsDataContext _context;

        public UserController(NsDataContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginRequest input)
        {
            // 1. 確認帳號輸入無誤
            // 2. 查詢資料
            // 3. 確認有資料
            // 4. 確認密碼相符
            // 5. 建立並回傳 JWT
            await Task.Run(() => Console.WriteLine("Hello World!"));
            return Ok();
        }
    }
}