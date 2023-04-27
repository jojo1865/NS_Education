namespace NS_Education.Models.APIItems.UserData.UserLog.GetList
{
    public class UserLog_GetList_Output_Row_APIItem : BaseGetResponseWithCreDateOnly
    {
        public long ULID { get; set; }
        public int UID { get; set; }
        public string UserName { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public string ControlType { get; set; }
    }
}