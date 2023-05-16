using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds2
{
    public class PrintReport_GetResverListByIds2_Input_APIItem : BaseRequestForList
    {
        public ICollection<int> Id { get; set; } = new List<int>();
    }
}