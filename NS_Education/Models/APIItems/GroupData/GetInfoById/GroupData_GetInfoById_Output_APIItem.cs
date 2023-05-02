using System.Collections.Generic;

namespace NS_Education.Models.APIItems.GroupData.GetInfoById
{
    public class GroupData_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
    {
        public int GID { get; set; }
        public string Title { get; set; }

        public ICollection<GroupData_MenuItem_APIItem> GroupItems { get; set; } =
            new List<GroupData_MenuItem_APIItem>();
    }
}