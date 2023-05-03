using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_SiteItem_APIItem
    {
        public int RSID { get; set; }
        public string TargetDate { get; set; }
        public int BSID { get; set; }
        public string BS_Title { get; set; }
        public int BOCID { get; set; }
        public string BOC_Code { get; set; }

        public ICollection<BaseResponseRowForSelectable> BOC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> BSC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();

        public ICollection<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem> TimeSpanItems =
            new List<Resver_GetAllInfoById_Output_TimeSpanItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_ThrowItem_APIItem> ThrowItems =
            new List<Resver_GetAllInfoById_Output_ThrowItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_DeviceItem_APIItem> DeviceItems =
            new List<Resver_GetAllInfoById_Output_DeviceItem_APIItem>();
    }
}