using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.SiteData.GetInfoById
{
    public class SiteData_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BSID { get; set; }
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

        public ICollection<BaseResponseRowForSelectable> FloorList { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int BSCID5 { get; set; }
        public ICollection<BaseResponseRowForSelectable> TableList { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int DHID { get; set; }
        public ICollection<BaseResponseRowForSelectable> HallList { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int BOCID { get; set; }

        public ICollection<SiteData_GetInfoById_Output_GroupList_Row_APIItem> Items { get; set; } =
            new List<SiteData_GetInfoById_Output_GroupList_Row_APIItem>();
    }
}