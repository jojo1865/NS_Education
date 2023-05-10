namespace NS_Education.Models.APIItems.TimeSpan.Submit
{
    public class TimeSpan_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DTSID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int HourS { get; set; }
        public int MinuteS { get; set; }
        public int HourE { get; set; }
        public int MinuteE { get; set; }
        
        public string PriceRate { get; set; }
    }
}