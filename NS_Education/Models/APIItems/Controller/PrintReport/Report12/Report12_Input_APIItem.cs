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

        /// <summary>
        /// （可選）年月區間（起）。YYYY/MM
        /// </summary>
        public string StartYearMonth { get; set; }

        /// <summary>
        /// （可選）年月區間（迄）。YYYY/MM
        /// </summary>
        public string EndYearMonth { get; set; }

        /// <summary>
        /// （可選）場地類別的名稱
        /// </summary>
        public string BC_Title { get; set; }

        /// <summary>
        /// （可選）場地名稱（支援模糊搜尋）
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// （可選）顯示/不顯示內部單位。未給值時，視為 true。
        /// </summary>
        public bool ShowInternal { get; set; } = true;

        /// <summary>
        /// （可選）顯示/不顯示外部單位。未給值時，視為 true。
        /// </summary>
        public bool ShowExternal { get; set; } = true;

        /// <summary>
        /// （可選）顯示/不顯示通訊處。未給值時，視為 true。
        /// </summary>
        public bool ShowCommDept { get; set; } = true;

        internal int?[] Hours => new[]
        {
            JanHours, FebHours, MarHours, AprHours, MayHours, JunHours, JulHours, AugHours, SepHours, OctHours,
            NovHours, DecHours
        };
    }
}