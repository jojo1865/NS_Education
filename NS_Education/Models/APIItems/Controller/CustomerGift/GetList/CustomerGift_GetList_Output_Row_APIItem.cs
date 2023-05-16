namespace NS_Education.Models.APIItems.Controller.CustomerGift.GetList
{
    public class CustomerGift_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int CGID { get; set; }
        
        public int CID { get; set; }
        public string C_TitleC { get; set; }
        public string C_TitleE { get; set; }
        
        public int Year { get; set; }
        public string SendDate { get; set; }
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        
        public string Title { get; set; }
        public int Ct { get; set; }
        public string Note { get; set; }
    }
}