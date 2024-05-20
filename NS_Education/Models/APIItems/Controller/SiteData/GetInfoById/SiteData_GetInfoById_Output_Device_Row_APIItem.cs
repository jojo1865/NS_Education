using System;

namespace NS_Education.Models.APIItems.Controller.SiteData.GetInfoById
{
    public class SiteData_GetInfoById_Output_Device_Row_APIItem
    {
        [Obsolete] public int BDID { get; set; }

        [Obsolete] public string BD_Code { get; set; }

        [Obsolete] public string BD_Title { get; set; }

        [Obsolete] public bool IsImplicit { get; set; }

        public int BSID { get; set; }
        public string BS_Code { get; set; }
        public string BS_Title { get; set; }
        public string DeviceName { get; set; }
        public int Ct { get; set; }
    }
}