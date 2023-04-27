namespace NS_Education.Models.APIItems.PartnerItem.GetList
{
    public class PartnerItem_GetList_Output_Row_APIItem : BaseGetResponseWithCreUpd
    {
        public int BPIID { get; set; }
        
        public int BPID { get; set; }
        public string BP_Title { get; set; }
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        
        public int BOCID { get; set; }
        public string BOC_Title { get; set; }
        
        public int DHID { get; set; }
        public string DH_Title { get; set; }
        
        public int Ct { get; set; }
        public int Price { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}