using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.Submit
{
    public class Resver_Submit_ThrowItem_Input_APIItem
    {
        public int RTID { get; set; }
        public int BSCID { get; set; }
        public string Title { get; set; }
        public int BOCID { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }

        public ICollection<int> TimeSpanItems { get; set; } =
            new List<int>();

        public ICollection<Resver_Submit_FoodItem_Input_APIItem> FoodItems { get; set; } =
            new List<Resver_Submit_FoodItem_Input_APIItem>();
    }
}