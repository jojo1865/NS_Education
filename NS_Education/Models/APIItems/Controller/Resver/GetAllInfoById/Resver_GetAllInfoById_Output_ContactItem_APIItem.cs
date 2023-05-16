using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_ContactItem_APIItem
    {
        public int MID { get; set; }
        public int ContactType { get; set; }

        public ICollection<BaseResponseRowForSelectable> ContactTypeList { get; set; } =
            new List<BaseResponseRowForSelectable>();
        public string ContactData { get; set; }
    }
}