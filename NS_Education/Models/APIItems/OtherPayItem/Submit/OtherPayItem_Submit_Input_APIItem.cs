namespace NS_Education.Models.APIItems.OtherPayItem.Submit
{
    public class OtherPayItem_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DOPIID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
    }
}