namespace NS_Education.Models.APIItems.Controller.CustomerQuestion.GetUniqueAreas
{
    public class CustomerQuestion_GetUniqueAreas_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int MaxRow { get; set; } = 10;
    }
}