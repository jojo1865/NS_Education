namespace NS_Education.Models.APIItems.FoodCategory.GetInfoById
{
    public class FoodCategory_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DFCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }
    }
}