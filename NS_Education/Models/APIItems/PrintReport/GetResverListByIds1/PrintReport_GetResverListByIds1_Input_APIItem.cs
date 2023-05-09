using System.Collections.Generic;

namespace NS_Education.Models.APIItems.PrintReport.GetResverListByIds1
{
    public class PrintReport_GetResverListByIds1_Input_APIItem : BaseRequestForList
    {
        public ICollection<int> Id { get; set; } = new List<int>();
    }
}