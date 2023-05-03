namespace NS_Education.Models.APIItems.TimeSpan.GetInfoById
{
    public class TimeSpan_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
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

        public string GetTimeSpan { get; set; }
    }
}