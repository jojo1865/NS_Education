using System.Collections.Generic;

namespace NS_Education.Models.APIItems.PartnerItem.GetInfoById
{
    public class PartnerItem_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BPIID { get; set; }
        
        public int BPID { get; set; }
        public string BP_Title { get; set; }

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        public ICollection<BaseResponseRowForSelectable> BSC_List { get; set; } = new List<BaseResponseRowForSelectable>();
        
        public int BOCID { get; set; }
        public string BOC_Title { get; set; }
        public ICollection<BaseResponseRowForSelectable> BOC_List { get; set; } = new List<BaseResponseRowForSelectable>();
        
        public int DHID { get; set; }
        public string DH_Title { get; set; }
        public ICollection<BaseResponseRowForSelectable> DH_List { get; set; } = new List<BaseResponseRowForSelectable>();
        
        public int Ct { get; set; }
        public int Price { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}