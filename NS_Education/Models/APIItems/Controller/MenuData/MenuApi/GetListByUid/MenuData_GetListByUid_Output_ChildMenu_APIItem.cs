using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.MenuData.MenuApi.GetListByUid
{
    public class MenuData_GetListByUid_Output_ChildMenu_APIItem
    {
        public int MDID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int SortNo { get; set; }

        public IEnumerable<MenuData_GetListByUid_Output_MenuApi_APIItem> Items { get; set; } =
            new List<MenuData_GetListByUid_Output_MenuApi_APIItem>();
    }
}