namespace NS_Education.Models.APIItems.Controller.OrderCode.GetList
{
    public class OrderCode_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int BOCID { get; set; }
        public string iCodeType { get; set; }
        public string sCodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int SortNo { get; set; }
    }
}