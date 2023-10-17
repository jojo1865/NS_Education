namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率分析表的輸入物件。
    /// </summary>
    public class Report12_Input_APIItem : BaseRequestForPagedList
    {
        public int Year { get; set; }
        public int? JanHours { get; set; }
        public int? FebHours { get; set; }
        public int? MarHours { get; set; }
        public int? AprHours { get; set; }
        public int? MayHours { get; set; }
        public int? JunHours { get; set; }
        public int? JulHours { get; set; }
        public int? AugHours { get; set; }
        public int? SepHours { get; set; }
        public int? OctHours { get; set; }
        public int? NovHours { get; set; }
        public int? DecHours { get; set; }

        internal int?[] Hours => new int?[]
        {
            JanHours, FebHours, MarHours, AprHours, MayHours, JunHours, JulHours, AugHours, SepHours, OctHours,
            NovHours, DecHours
        };
    }
}