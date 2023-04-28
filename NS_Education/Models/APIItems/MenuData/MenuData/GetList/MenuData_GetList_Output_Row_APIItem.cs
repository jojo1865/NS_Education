namespace NS_Education.Models.APIItems.MenuData.MenuData.GetList
{
    public class MenuData_GetList_Output_Row_APIItem : BaseGetResponseWithCreUpd
    {
        public int MDID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int SortNo { get; set; }
    }
}