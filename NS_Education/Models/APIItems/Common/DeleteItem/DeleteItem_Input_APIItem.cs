using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Common.DeleteItem
{
    public class DeleteItem_Input_APIItem
    {
        public IEnumerable<DeleteItem_Input_Row_APIItem> Items { get; set; } = new List<DeleteItem_Input_Row_APIItem>();
    }
}