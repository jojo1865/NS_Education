namespace NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList
{
    public class UserLog_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreDateOnly
    {
        public long ULID { get; set; }
        public int UID { get; set; }
        public string UserName { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public string ControlType { get; set; }
        public string RequestUrl { get; set; }
        public string CreDate { get; set; }
    }
}