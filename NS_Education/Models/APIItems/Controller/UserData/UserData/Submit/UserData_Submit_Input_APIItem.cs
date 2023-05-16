namespace NS_Education.Models.APIItems.Controller.UserData.UserData.Submit
{
    public class UserData_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DDID { get; set; }
        
        /// <summary>
        /// 對象的 UID，而非操作者的 UID。
        /// </summary>
        public int UID { get; set; }
        public string Username { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public string Note { get; set; }
        public int GID { get; set; }
    }
}