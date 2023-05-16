namespace NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList
{
    public class UserLog_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public int UID { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
    }
}