namespace NS_Education.Models.APIItems.Controller.UserData.UserData.GetList
{
    public class UserData_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int Uid { get; set; }
        public string Username { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
    }
}