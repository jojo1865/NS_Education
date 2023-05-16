namespace NS_Education.Models.APIItems.Controller.FoodCategory.Submit
{
    public class FoodCategory_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DFCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }
    }
}