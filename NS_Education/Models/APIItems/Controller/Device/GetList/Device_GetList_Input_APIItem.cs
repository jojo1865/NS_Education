namespace NS_Education.Models.APIItems.Controller.Device.GetList
{
    public class Device_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
        public int DHID { get; set; }
        public int BOCID { get; set; }
    }
}