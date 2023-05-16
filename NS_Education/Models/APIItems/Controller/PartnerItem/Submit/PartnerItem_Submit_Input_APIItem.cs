namespace NS_Education.Models.APIItems.Controller.PartnerItem.Submit
{
    public class PartnerItem_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BPIID { get; set; }
        
        public int BPID { get; set; }

        public int BSCID { get; set; }

        public int BOCID { get; set; }

        public int DHID { get; set; }

        public int Ct { get; set; }
        public int Price { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}