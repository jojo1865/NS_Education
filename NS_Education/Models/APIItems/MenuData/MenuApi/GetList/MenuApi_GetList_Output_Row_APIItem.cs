namespace NS_Education.Models.APIItems.MenuData.MenuApi.GetList
{
    public class MenuApi_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreDateOnly
    {
        public int SeqNo { get; set; }
        public string ApiUrl { get; set; }
        public int iApiType { get; set; }
        public string sApiType { get; set; }
        public string Note { get; set; }
    }
}