using System;

namespace NS_Education.Models.APIItems.Controller.SiteData.Submit
{
    public class SiteData_Submit_Input_Devices_Row_APIItem
    {
        [Obsolete] public int BDID { get; set; }

        [Obsolete] public bool? IsImplicit { get; set; }


        public int BSID { get; set; }
        public int Ct { get; set; }
        public string DeviceName { get; set; }
    }
}