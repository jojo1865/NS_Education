namespace NS_Education.Models.APIItems.Controller.TimeSpan.GetInfoById
{
    public class TimeSpan_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DTSID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int HourS { get; set; }
        public int MinuteS { get; set; }
        public int HourE { get; set; }
        public int MinuteE { get; set; }

        public string TimeS { get; set; }
        public string TimeE { get; set; }
        
        public string PriceRate { get; set; }
        public string GetTimeSpan { get; set; }
    }
}