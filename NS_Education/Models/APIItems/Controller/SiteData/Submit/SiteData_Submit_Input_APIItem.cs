using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.SiteData.Submit
{
    public class SiteData_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BSID { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int BasicSize { get; set; }
        public int MaxSize { get; set; } = 0;
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public bool CubicleFlag { get; set; }
        public string PhoneExt1 { get; set; }
        public string PhoneExt2 { get; set; }
        public string PhoneExt3 { get; set; }
        public string Note { get; set; }
        public int BSCID1 { get; set; }
        public int BSCID5 { get; set; }
        public int DHID { get; set; }
        public int BOCID { get; set; }

        public List<SiteData_Submit_Input_GroupList_Row_APIItem> GroupList { get; set; } =
            new List<SiteData_Submit_Input_GroupList_Row_APIItem>();

        public List<SiteData_Submit_Input_Devices_Row_APIItem> Devices { get; set; } =
            new List<SiteData_Submit_Input_Devices_Row_APIItem>();
    }
}