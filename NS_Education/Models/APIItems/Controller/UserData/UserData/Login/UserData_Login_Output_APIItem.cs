namespace NS_Education.Models.APIItems.Controller.UserData.UserData.Login
{
    public class UserData_Login_Output_APIItem : BaseInfusable
    {
        public int UID { get; set; }
        public string Username { get; set; }

        // TODO: 原本 JWT 透過這裡傳給前端，後來改為 cookie 處理，待前端調整完畢後，這裡也要拿掉。
        public string JwtToken { get; } = "dummy";

        public string CookieName { get; set; }
    }
}