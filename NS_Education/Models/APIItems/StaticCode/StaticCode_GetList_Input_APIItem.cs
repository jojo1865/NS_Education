namespace NS_Education.Models.APIItems.StaticCode
{
    public class StaticCode_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int CodeType { get; set; }
    }
}