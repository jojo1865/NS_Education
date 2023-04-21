namespace NS_Education.Models.APIItems.OrderCode.Submit
{
    public class OrderCode_Submit_Input_APIItem
    {
        public int BOCID { get; set; }
        public int CodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public bool? ActiveFlag { get; set; }
        public bool? DeleteFlag { get; set; } = false;
    }
}