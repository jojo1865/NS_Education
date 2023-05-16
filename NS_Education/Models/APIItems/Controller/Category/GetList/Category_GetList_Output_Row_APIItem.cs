namespace NS_Education.Models.APIItems.Controller.Category.GetList
{
    public class Category_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int BCID { get; set; }
        public int iCategoryType { get; set; }
        public string sCategoryType { get; set; }
        public int ParentID { get; set; }
        public string ParentTitleC { get; set; }
        public string ParentTitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int SortNo { get; set; }
    }
}