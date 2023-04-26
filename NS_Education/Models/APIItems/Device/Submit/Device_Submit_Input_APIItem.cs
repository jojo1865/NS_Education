namespace NS_Education.Models.APIItems.Device.Submit
{
    public class Device_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BDID { get; set; }
        
        public int BCID { get; set; }

        public int BSCID { get; set; }

        public int BOCID { get; set; }

        public int DHID { get; set; }

        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public string SupplierTitle { get; set; }
        public string SupplierName { get; set; }
        public string SupplierPhone { get; set; }
        public string Repair { get; set; }
        public string Note { get; set; }
    }
}