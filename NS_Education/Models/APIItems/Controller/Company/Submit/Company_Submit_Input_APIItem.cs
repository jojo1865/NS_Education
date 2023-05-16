namespace NS_Education.Models.APIItems.Controller.Company.Submit
{
    public class Company_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
    }
}