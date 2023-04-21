namespace NS_Education.Models.APIItems.StaticCode.Submit
{
    public class StaticCode_Submit_Input_APIItem
    {
        public int? BSCID { get; set; }
        public int? CodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public bool? ActiveFlag { get; set; }
        public bool? DeleteFlag { get; set; }
    }
}