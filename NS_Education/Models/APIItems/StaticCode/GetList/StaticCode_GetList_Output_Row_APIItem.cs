namespace NS_Education.Models.APIItems.StaticCode.GetList
{
    public class StaticCode_GetList_Output_Row_APIItem : BaseGetResponseWithCreUpd
    {
        public int BSCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}