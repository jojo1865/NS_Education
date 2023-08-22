using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.Submit
{
    public class Resver_Submit_SiteItem_Input_APIItem
    {
        public int RSID { get; set; }
        public string TargetDate { get; set; }
        public int BSID { get; set; }
        public int BOCID { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        public int BSCID { get; set; }

        public ICollection<int> TimeSpanItems { get; set; } =
            new List<int>();

        public ICollection<Resver_Submit_ThrowItem_Input_APIItem> ThrowItems { get; set; } =
            new List<Resver_Submit_ThrowItem_Input_APIItem>();

        public ICollection<Resver_Submit_DeviceItem_Input_APIItem> DeviceItems { get; set; } =
            new List<Resver_Submit_DeviceItem_Input_APIItem>();
    }
}