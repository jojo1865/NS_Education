namespace NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList
{
    public class UserLog_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; } = "";
        public string EventType { get; set; } = "";
    }
}