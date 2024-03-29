using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_SiteItem_APIItem
    {
        public ICollection<Resver_GetAllInfoById_Output_DeviceItem_APIItem> DeviceItems =
            new List<Resver_GetAllInfoById_Output_DeviceItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_ThrowItem_APIItem> ThrowItems =
            new List<Resver_GetAllInfoById_Output_ThrowItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem> TimeSpanItems =
            new List<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem>();

        public int RSID { get; set; }
        public string TargetDate { get; set; }
        public int BSID { get; set; }
        public string BS_Title { get; set; }
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

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public string ArriveTimeStart { get; set; }
        public string ArriveTimeEnd { get; set; }
        public string ArriveDescription { get; set; }
        public string TableDescription { get; set; }
        public string SeatImage { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();
    }
}