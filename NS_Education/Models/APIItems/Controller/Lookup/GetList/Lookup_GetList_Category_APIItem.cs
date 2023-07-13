using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Lookup.GetList
{
    public class Lookup_GetList_Category_APIItem
    {
        public string Title { get; set; }

        public IEnumerable<Lookup_GetList_Lookup_APIItem> Lookups { get; set; } =
            new List<Lookup_GetList_Lookup_APIItem>();
    }
}