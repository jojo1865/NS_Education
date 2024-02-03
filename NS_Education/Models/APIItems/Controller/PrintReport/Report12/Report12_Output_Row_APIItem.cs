using System;
using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率分析表中代表每個資料行的物件。
    /// </summary>
    public class Report12_Output_Row_APIItem
    {
        public IEnumerable<string> Cells { get; set; } =
            Array.Empty<string>();
    }
}