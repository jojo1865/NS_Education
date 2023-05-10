namespace NS_Education.Models.APIItems.TimeSpan.GetList
{
    public class TimeSpan_GetList_Output_Row_APIItem : BaseGetResponseWithCreUpd
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