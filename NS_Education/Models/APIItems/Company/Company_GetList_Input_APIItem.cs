namespace NS_Education.Models.APIItems.Company
{
    public class Company_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
    }
}