namespace NS_Education.Models.APIItems.Hall.GetList
{
    public class Hall_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int DDID { get; set; }
    }
}