namespace NS_Education.Models.APIItems.Department.GetList
{
    public class Department_GetList_Input_APIItem : BaseRequestForPagedList
    {
       public string Keyword { get; set; }
       public int DCID { get; set; }
    }
}