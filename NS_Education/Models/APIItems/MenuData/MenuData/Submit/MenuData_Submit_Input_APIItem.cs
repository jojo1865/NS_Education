namespace NS_Education.Models.APIItems.MenuData.MenuData.Submit
{
    public class MenuData_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int MDID { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; } = "";
        public int SortNo { get; set; }
    }
}