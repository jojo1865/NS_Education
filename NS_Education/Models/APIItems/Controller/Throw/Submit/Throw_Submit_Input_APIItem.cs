namespace NS_Education.Models.APIItems.Controller.Throw.Submit
{
    public class Throw_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DTID { get; set; }
        public int BOCID { get; set; }
        public int BSCID { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public string Remark { get; set; }
    }
}