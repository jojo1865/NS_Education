namespace NS_Education.Models.APIItems.StaticCode.GetList
{
    public class StaticCode_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int CodeType { get; set; }
    }
}