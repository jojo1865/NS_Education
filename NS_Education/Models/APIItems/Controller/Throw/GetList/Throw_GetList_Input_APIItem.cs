namespace NS_Education.Models.APIItems.Controller.Throw.GetList
{
    public class Throw_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }

        public int? BOCID { get; set; }

        public int? BSCID { get; set; }
    }
}