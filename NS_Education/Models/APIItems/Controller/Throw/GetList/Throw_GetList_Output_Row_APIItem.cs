namespace NS_Education.Models.APIItems.Controller.Throw.GetList
{
    public class Throw_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int DTID { get; set; }
        public string BOC_Title { get; set; }
        public string BSC_Title { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
    }
}