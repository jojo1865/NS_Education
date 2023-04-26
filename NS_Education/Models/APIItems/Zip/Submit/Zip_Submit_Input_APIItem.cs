namespace NS_Education.Models.APIItems.Zip.Submit
{
    public class Zip_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DZID { get; set; }
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Note { get; set; }
    }
}