using System.Collections.Generic;

namespace NS_Education.Models.APIItems.GroupData.SubmitMenuData
{
    public class GroupData_SubmitMenuData_Input_APIItem
    {
        public int GID { get; set; }

        public ICollection<GroupData_MenuItem_APIItem> GroupItems { get; set; }
            = new List<GroupData_MenuItem_APIItem>();
    }
}