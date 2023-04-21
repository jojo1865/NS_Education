﻿using NS_Education.Tools;

namespace NS_Education.Models.APIItems.UserData.Login
{
    public class UserData_Login_Output_APIItem : BaseInfusable
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
    }
}