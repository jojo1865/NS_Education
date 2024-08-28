namespace NS_Education.Models.APIItems.Controller.Category.Submit
{
    public class Category_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BCID { get; set; }
        public int ParentID { get; set; }
        public int CategoryType { get; set; }

        /// <summary>
        /// 前端相容別名欄位。前端有時候會丟這個欄位
        /// </summary>
        public int iCategoryType
        {
            get => CategoryType;
            set => CategoryType = value;
        }

        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
    }
}