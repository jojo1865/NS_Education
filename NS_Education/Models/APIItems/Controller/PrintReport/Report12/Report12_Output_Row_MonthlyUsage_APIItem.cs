using System.Linq;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率分析表的單筆輸出物件中，表示每月使用率的子物件。
    /// </summary>
    public class Report12_Output_Row_MonthlyUsage_APIItem
    {
        public string[] MonthlyUsage => MonthlyUsageDecimal.Select(dec => dec?.ToString("P")).ToArray();
        internal decimal?[] MonthlyUsageDecimal { get; set; }
    }
}