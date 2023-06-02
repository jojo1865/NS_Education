namespace NS_Education.Models.APIItems.Controller.Company.GetList
{
    public class Company_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
        public int DepartmentDeleteFlag { get; set; } = 0;
    }
}