namespace NS_Education.Models.APIItems.Company.GetList
{
    public class Company_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
    }
}