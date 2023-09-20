using NS_Education.Variables;

namespace NS_Education.Models.APIItems.Controller.UserData.UserLog.GetTypeList
{
    public class UserLog_GetTypeList_Output_APIItem
    {
        public string Title { get; set; }
        public int UserLogType { get; set; } = -1;
        public int UserPasswordLogType { get; set; } = -1;
        public UserLogType Type { get; set; }
    }
}