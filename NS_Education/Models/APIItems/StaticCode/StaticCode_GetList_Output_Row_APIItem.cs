namespace NS_Education.Models.APIItems.StaticCode
{
    public class StaticCode_GetList_Output_Row_APIItem : BaseRequestForList
    {
        public int BSCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}