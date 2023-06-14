using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.CustomerVisit.GetInfoById
{
    public class CustomerVisit_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int CVID { get; set; }
        public int CID { get; set; }
        public string C_TitleC { get; set; }
        public string C_TitleE { get; set; }

        public ICollection<BaseResponseRowForSelectable> C_List { get; set; } =
            new List<BaseResponseRowForSelectable>();

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> BSC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();

        public int BUID { get; set; }
        public string BU_Name { get; set; }

        public ICollection<BaseResponseRowForSelectable> BU_List { get; set; } =
            new List<BaseResponseRowForSelectable>();

        public string TargetTitle { get; set; }
        public string Title { get; set; }

        public string VisitDate { get; set; }
        public string Description { get; set; }
        public string AfterNote { get; set; }

        public string BSCID15_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> BSCID15_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
    }
}