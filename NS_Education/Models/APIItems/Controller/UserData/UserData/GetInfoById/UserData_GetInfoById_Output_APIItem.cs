namespace NS_Education.Models.APIItems.Controller.UserData.UserData.GetInfoById
{
    public class UserData_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public int DDID { get; set; }
        public int GID { get; set; }
        public string Note { get; set; }
    }
}