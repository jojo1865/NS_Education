namespace NS_Education.Models.APIItems.Zip.GetList
{
    public class Zip_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int ParentId { get; set; }
    }
}