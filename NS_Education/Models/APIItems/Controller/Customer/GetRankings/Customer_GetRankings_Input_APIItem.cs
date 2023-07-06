namespace NS_Education.Models.APIItems.Controller.Customer.GetRankings
{
    public class Customer_GetRankings_Input_APIItem : BaseRequestForPagedList
    {
        public int RankBy { get; set; }
        public string DateS { get; set; }
        public string DateE { get; set; }
    }
}