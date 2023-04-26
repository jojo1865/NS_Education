namespace NS_Education.Models.APIItems.CustomerGift.Submit
{
    public class CustomerGift_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int CGID { get; set; }
        public int CID { get; set; }
        public int Year { get; set; }
        public string SendDate { get; set; }
        public int BSCID { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public string Note { get; set; }
    }
}