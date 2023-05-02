using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Category.GetInfoById
{
    public class Category_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
    {
        public int BCID { get; set; }
        public int iCategoryType { get; set; }
        public string sCategoryType { get; set; }

        public ICollection<BaseResponseRowForType> CategoryTypeList { get; set; } =
            new List<BaseResponseRowForType>();
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int SortNo { get; set; }
    }
}