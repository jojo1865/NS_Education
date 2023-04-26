namespace NS_Education.Models.APIItems.Device.GetList
{
    public class Device_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
        public int DHID { get; set; }
        public int BOCID { get; set; }
    }
}