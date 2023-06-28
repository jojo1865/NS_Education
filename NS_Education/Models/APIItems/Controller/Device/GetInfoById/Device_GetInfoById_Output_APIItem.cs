using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Device.GetInfoById
{
    public class Device_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BDID { get; set; }

        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }

        public ICollection<CommonResponseRowForSelectable> BC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BOCID { get; set; }
        public string BOC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BOC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int DHID { get; set; }
        public string DH_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> DH_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public string SupplierTitle { get; set; }
        public string SupplierName { get; set; }
        public string SupplierPhone { get; set; }
        public string Repair { get; set; }
        public string Note { get; set; }
    }
}