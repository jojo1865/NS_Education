using System;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.UserData.UserData.Login
{
    public class UserData_Login_Output_APIItem : BaseInfusable
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public int MaxAgeMinutes => DateTime.Now.MinutesUntil(DateTime.Now.GetNextJwtExpireDateTime());

        public int DaysUntilPasswordExpires { get; set; }

        public bool IsAdministrator { get; set; }

        public string IpAddress { get; set; }
    }
}