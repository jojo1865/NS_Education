using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.FoodCategory.GetList
{
    public class FoodCategory_GetList_Output_Row_APIItem : BaseResponseWithCreUpd<D_FoodCategory>
    {
        public int DFCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }
    }
}