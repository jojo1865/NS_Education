using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Models.APIItems.Common.DeleteItem
{
    public class DeleteItem_Input_APIItem
    {
        public IEnumerable<DeleteItem_Input_Row_APIItem> Items { get; set; } = new List<DeleteItem_Input_Row_APIItem>();

        public HashSet<int> GetUniqueDeleteId()
        {
            return Items
                .Where(i => i.Id != null && i.DeleteFlag == true)
                .Select(i => i.Id.Value)
                .Distinct()
                .ToHashSet();
        }
    }
}