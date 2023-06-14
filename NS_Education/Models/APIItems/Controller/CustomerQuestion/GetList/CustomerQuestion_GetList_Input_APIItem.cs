namespace NS_Education.Models.APIItems.Controller.CustomerQuestion.GetList
{
    public class CustomerQuestion_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string QuestionTitle { get; set; }
        public string Area { get; set; }
        public int CID { get; set; }
        public int ResponseType { get; set; } = -1;
        public string SDate { get; set; }
        public string EDate { get; set; }
    }
}