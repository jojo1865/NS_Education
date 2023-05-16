namespace NS_Education.Models.APIItems.Controller.Department.Submit
{
    public class Department_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DDID { get; set; }
        public int DCID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int PeopleCt { get; set; }
    }
}