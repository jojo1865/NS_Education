using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_DeviceItem_APIItem
    {
        public ICollection<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem> TimeSpanItems =
            new List<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem>();

        public int RDID { get; set; }
        public string TargetDate { get; set; }
        public int BDID { get; set; }
        public string BD_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BD_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int Ct { get; set; }
        public int BOCID { get; set; }
        public string BOC_Code { get; set; }

        public ICollection<CommonResponseRowForSelectable> BOC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}