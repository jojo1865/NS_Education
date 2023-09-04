using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report2
{
    /// <summary>
    /// Function Order 的輸入物件。
    /// </summary>
    public class Report2_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> RHID { get; set; }
    }
}