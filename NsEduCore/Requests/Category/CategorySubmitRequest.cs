namespace NsEduCore.Requests.Category
{
    /// <summary>
    /// Category 的新增/修改的傳入物件。
    /// </summary>
    public class CategorySubmitRequest
    {
        public int BCID { get; set; }
        public int CreUID { get; set; }
        public int UpdUID { get; set; }
        public int ParentID { get; set; }
        public int CategoryType { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
    }
}