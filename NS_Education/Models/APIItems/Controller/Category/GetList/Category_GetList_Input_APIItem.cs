namespace NS_Education.Models.APIItems.Controller.Category.GetList
{
    public class Category_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int CategoryType { get; set; }
    }
}