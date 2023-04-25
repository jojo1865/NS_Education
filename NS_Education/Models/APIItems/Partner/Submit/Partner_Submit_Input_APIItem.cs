namespace NS_Education.Models.APIItems.Partner.Submit
{
    public class Partner_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BPID { get; set; }
        
        public int BCID { get; set; }

        public string Code { get; set; }
        public string Title { get; set; }
        public string Compilation { get; set; }
        
        public int BSCID { get; set; }
        

        public string Email { get; set; }
        public string Note { get; set; }
        public bool CleanFlag { get; set; }
        public int CleanPrice { get; set; }
        public string CleanSDate { get; set; }
        public string CleanEDate { get; set; }
    }
}