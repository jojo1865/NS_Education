namespace NS_Education.Models.APIItems.OtherPayItem.GetList
{
    public class OtherPayItem_GetList_Output_Row_APIItem : BaseGetResponseWithCreUpd
    {
        public int DOPIID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int PaidType { get; set; }
        public int BSCID { get; set; }
        public int BOCID { get; set; }
    }
}