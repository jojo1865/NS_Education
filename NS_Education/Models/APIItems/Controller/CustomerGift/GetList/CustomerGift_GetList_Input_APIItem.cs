using System;

namespace NS_Education.Models.APIItems.Controller.CustomerGift.GetList
{
    public class CustomerGift_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int CID { get; set; }
        public int SendYear { get; set; } = DateTime.Now.Year;
        public string SDate { get; set; }
        public string EDate { get; set; }
    }
}