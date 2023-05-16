using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_OtherItem_APIItem
    {
        public int ROID { get; set; }
        public string TargetDate { get; set; }
        public int DOPIID { get; set; }
        public string DOPI_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> DOPI_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        
        public int BOCID { get; set; }
        public string BOC_Code { get; set; }

        public ICollection<BaseResponseRowForSelectable> BOC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public int Ct { get; set; }
        public int QuotedPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}