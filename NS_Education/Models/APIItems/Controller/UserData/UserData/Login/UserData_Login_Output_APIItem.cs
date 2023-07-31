using NS_Education.Variables;

namespace NS_Education.Models.APIItems.Controller.UserData.UserData.Login
{
    public class UserData_Login_Output_APIItem : BaseInfusable
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public int MaxAgeMinutes => JwtConstants.ExpireMinutes;

        public bool IsAdministrator { get; set; }
    }
}