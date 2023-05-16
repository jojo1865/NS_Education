namespace NS_Education.Models.APIItems.BusinessUser.GetInfoById
{
    public class BusinessUser_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool MKsalesFlag { get; set; }
        public bool OPsalesFlag { get; set; }
        public int CustomerCt { get; set; }
    }
}