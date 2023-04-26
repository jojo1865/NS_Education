namespace NS_Education.Models.APIItems.CustomerQuestion.GetList
{
    public class CustomerQuestion_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int CID { get; set; }
        public int ResponseType { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
    }
}