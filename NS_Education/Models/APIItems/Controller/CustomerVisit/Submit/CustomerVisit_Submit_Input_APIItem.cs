namespace NS_Education.Models.APIItems.Controller.CustomerVisit.Submit
{
    public class CustomerVisit_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int CVID { get; set; }
        public int CID { get; set; }
        public int BSCID { get; set; }
        public int? BSCID15 { get; set; }
        public int BUID { get; set; }
        public string TargetTitle { get; set; }
        public string Title { get; set; }

        public string VisitDate { get; set; }
        public string Description { get; set; }
        public string AfterNote { get; set; }
    }
}