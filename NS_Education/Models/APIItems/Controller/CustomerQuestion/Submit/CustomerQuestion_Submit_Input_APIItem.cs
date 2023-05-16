namespace NS_Education.Models.APIItems.Controller.CustomerQuestion.Submit
{
    public class CustomerQuestion_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int CQID { get; set; }
        
        public int CID { get; set; }

        public string AskDate { get; set; }
        public string AskTitle { get; set; }
        public string AskArea { get; set; }
        public string AskDescription { get; set; }
        
        public bool ResponseFlag { get; set; }
        public string ResponseUser { get; set; }
        public string ResponseDescription { get; set; }
        public string ResponseDate { get; set; }
    }
}