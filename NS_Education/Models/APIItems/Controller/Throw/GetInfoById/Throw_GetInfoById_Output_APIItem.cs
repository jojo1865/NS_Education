namespace NS_Education.Models.APIItems.Controller.Throw.GetInfoById
{
    public class Throw_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DTID { get; set; }
        public int BOCID { get; set; }
        public string BOC_Title { get; set; }
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public string Remark { get; set; }
    }
}