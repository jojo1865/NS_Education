namespace NS_Education.Models.APIItems.Category.Submit
{
    public class Category_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BCID { get; set; }
        public int ParentID { get; set; }
        public int CategoryType { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
    }
}