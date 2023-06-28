using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.CustomerVisit.GetInfoById
{
    public class CustomerVisit_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int CVID { get; set; }
        public int CID { get; set; }
        public string C_TitleC { get; set; }
        public string C_TitleE { get; set; }

        public ICollection<CustomerVisit_CustomerSelectable> C_List { get; set; } =
            new List<CustomerVisit_CustomerSelectable>();

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BUID { get; set; }
        public string BU_Name { get; set; }

        public ICollection<CommonResponseRowForSelectable> BU_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string TargetTitle { get; set; }
        public string Title { get; set; }

        public string VisitDate { get; set; }
        public string Description { get; set; }
        public string AfterNote { get; set; }

        public bool HasReservation { get; set; }
        public int? BSCID15 { get; set; }
        public string BSCID15_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSCID15_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public ICollection<CustomerVisit_GetInfoById_GiftSendings_Row_APIItem> GiftSendings { get; set; } =
            new List<CustomerVisit_GetInfoById_GiftSendings_Row_APIItem>();
    }
}