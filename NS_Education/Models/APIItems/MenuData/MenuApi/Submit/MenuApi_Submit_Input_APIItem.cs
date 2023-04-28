namespace NS_Education.Models.APIItems.MenuData.MenuApi.Submit
{
    public class MenuApi_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int MDID { get; set; }
        public int SeqNo { get; set; }
        public string ApiUrl { get; set; }
        public int APIType { get; set; }
        public string Note { get; set; }
    }
}