using System.Linq;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率分析表的單筆輸出物件中，表示每月使用率的子物件。
    /// </summary>
    public class Report12_Output_Row_MonthlyUsage_APIItem
    {
        public string Jan => MonthlyUsage[0];
        public string Feb => MonthlyUsage[1];
        public string Mar => MonthlyUsage[2];
        public string Apr => MonthlyUsage[3];
        public string May => MonthlyUsage[4];
        public string Jun => MonthlyUsage[5];
        public string Jul => MonthlyUsage[6];
        public string Aug => MonthlyUsage[7];
        public string Sep => MonthlyUsage[8];
        public string Oct => MonthlyUsage[9];
        public string Nov => MonthlyUsage[10];
        public string Dec => MonthlyUsage[11];
        public string[] MonthlyUsage => MonthlyUsageDecimal.Select(dec => dec?.ToString("P")).ToArray();
        internal decimal?[] MonthlyUsageDecimal { get; set; }
    }
}