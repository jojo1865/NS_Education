namespace NS_Education.Models.APIItems.Controller.Zip.GetList
{
    public class Zip_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int ParentId { get; set; }
    }
}