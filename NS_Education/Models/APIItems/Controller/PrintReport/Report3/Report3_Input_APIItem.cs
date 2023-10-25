using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    /// <summary>
    /// 客戶授權簽核表的輸入物件。
    /// </summary>
    public class Report3_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> RHID { get; set; }
        public string Description { get; set; }
    }
}