using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.SiteData.GetInfoById
{
    public class SiteData_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BSID { get; set; }
        public bool IsCombinedSiteChild { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int BasicSize { get; set; }
        public int MaxSize { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public bool CubicleFlag { get; set; }
        public string PhoneExt1 { get; set; }
        public string PhoneExt2 { get; set; }
        public string PhoneExt3 { get; set; }
        public string Note { get; set; }
        public int BSCID1 { get; set; }

        public ICollection<CommonResponseRowForSelectable> FloorList { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BSCID5 { get; set; }

        public ICollection<CommonResponseRowForSelectable> TableList { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int DHID { get; set; }

        public ICollection<CommonResponseRowForSelectable> HallList { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BOCID { get; set; }

        public ICollection<SiteData_GetInfoById_Output_GroupList_Row_APIItem> Items { get; set; } =
            new List<SiteData_GetInfoById_Output_GroupList_Row_APIItem>();

        public ICollection<SiteData_GetInfoById_Output_Device_Row_APIItem> Devices { get; set; } =
            new List<SiteData_GetInfoById_Output_Device_Row_APIItem>();
    }
}