using System;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report13
{
    /// <summary>
    /// 場地預估銷售月報表的輸入物件。
    /// </summary>
    public class Report13_Input_APIItem : BaseRequestForPagedList
    {
        /// <summary>
        /// （可選）查詢月份，格式 yyyy/MM
        /// </summary>
        public string TargetMonth { get; set; }

        internal string[] Splits => TargetMonth.Split('/');

        internal bool HasInputTargetMonth => Year != null && Month != null;
        internal int? Year => Splits.Length == 2 ? Convert.ToInt32(Splits[0]) : default;

        internal int? Month => Splits.Length == 2 ? Convert.ToInt32(Splits[1]) : default;
    }
}