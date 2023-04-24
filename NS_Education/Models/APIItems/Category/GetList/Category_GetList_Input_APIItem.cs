namespace NS_Education.Models.APIItems.Category.GetList
{
    public class Category_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int CategoryType { get; set; }
    }
}