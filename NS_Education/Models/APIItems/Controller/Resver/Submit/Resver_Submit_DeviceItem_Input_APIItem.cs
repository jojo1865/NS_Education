using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.Submit
{
    public class Resver_Submit_DeviceItem_Input_APIItem
    {
        public int RDID { get; set; }
        public string TargetDate { get; set; }
        public int BDID { get; set; }
        public int Ct { get; set; }
        public int BOCID { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }

        public ICollection<Resver_Submit_TimeSpanItem_Input_APIItem> TimeSpanItems { get; set; } =
            new List<Resver_Submit_TimeSpanItem_Input_APIItem>();
    }
}