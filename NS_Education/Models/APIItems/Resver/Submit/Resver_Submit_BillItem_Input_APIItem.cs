namespace NS_Education.Models.APIItems.Resver.Submit
{
    public class Resver_Submit_BillItem_Input_APIItem
    {
        public int RBID { get; set; }
        public int BCID { get; set; }
        public int DPTID { get; set; }
        public int Price { get; set; }
        public string Note { get; set; }
        public bool PayFlag { get; set; }
        public string PayDate { get; set; }
    }
}