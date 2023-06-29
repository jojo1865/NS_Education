using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Models.APIItems.Common.DeleteItem
{
    public class DeleteItem_Input_APIItem
    {
        public IEnumerable<DeleteItem_Input_Row_APIItem> Items { get; set; } = new List<DeleteItem_Input_Row_APIItem>();

        public HashSet<int> GetUniqueDeleteId()
        {
            return GetUniqueId(true);
        }

        public HashSet<int> GetUniqueReviveId()
        {
            return GetUniqueId(true);
        }

        private HashSet<int> GetUniqueId(bool deleteFlag)
        {
            return Items
                .Where(i => i.Id != null && i.DeleteFlag == deleteFlag)
                .Select(i => i.Id.Value)
                .Distinct()
                .ToHashSet();
        }
    }
}