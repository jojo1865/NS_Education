using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_ThrowItem_APIItem
    {
        public ICollection<Resver_GetAllInfoById_Output_FoodItem_APIItem> FoodItems =
            new List<Resver_GetAllInfoById_Output_FoodItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem> TimeSpanItems =
            new List<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem>();

        /// <summary>
        /// 此行程的獨特 ID，提供給前端加總時用。
        /// </summary>
        public int OnlyID { get; set; }

        public int RTID { get; set; }
        public string TargetDate { get; set; }

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string Title { get; set; }

        public int BOCID { get; set; }
        public string BOC_Title { get; set; }

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