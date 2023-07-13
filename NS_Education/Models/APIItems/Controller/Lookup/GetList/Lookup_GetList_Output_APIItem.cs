using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Lookup.GetList
{
    public class Lookup_GetList_Output_APIItem
    {
        public IEnumerable<Lookup_GetList_Category_APIItem> Categories { get; set; } =
            new List<Lookup_GetList_Category_APIItem>();
    }
}