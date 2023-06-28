using System.Collections.Generic;
using NS_Education.Models.APIItems.Controller.ContactType.GetList;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public ICollection<Resver_GetAllInfoById_Output_BillItem_APIItem> BillItems =
            new List<Resver_GetAllInfoById_Output_BillItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_ContactItem_APIItem> ContactItems =
            new List<Resver_GetAllInfoById_Output_ContactItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_GiveBackItem_APIItem> GiveBackItems =
            new List<Resver_GetAllInfoById_Output_GiveBackItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_OtherItem_APIItem> OtherItems =
            new List<Resver_GetAllInfoById_Output_OtherItem_APIItem>();

        public ICollection<Resver_GetAllInfoById_Output_SiteItem_APIItem> SiteItems =
            new List<Resver_GetAllInfoById_Output_SiteItem_APIItem>();

        public int RHID { get; set; }

        public int BSCID12 { get; set; }
        public string BSC12_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC12_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BSCID11 { get; set; }
        public string BSC11_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC11_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string Code { get; set; }
        public string Title { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public int PeopleCt { get; set; }
        public int CID { get; set; }
        public string CustomerTitle { get; set; }

        public ICollection<CommonResponseRowForSelectable> C_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string ContactName { get; set; }

        public ICollection<ContactType_GetList_Output_Row_APIItem> ContactTypeList { get; set; } =
            new List<ContactType_GetList_Output_Row_APIItem>();

        public int MK_BUID { get; set; }
        public string MK_BU_Name { get; set; }

        public ICollection<CommonResponseRowForSelectable> MK_BU_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string MK_Phone { get; set; }

        public int OP_BUID { get; set; }
        public string OP_BU_Name { get; set; }

        public ICollection<CommonResponseRowForSelectable> OP_BU_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string OP_Phone { get; set; }

        public string Note { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
    }
}