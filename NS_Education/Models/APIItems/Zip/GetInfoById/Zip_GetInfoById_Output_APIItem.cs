namespace NS_Education.Models.APIItems.Zip.GetInfoById
{
    public class Zip_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
    {
        public int DZID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public int ParentID { get; set; }
        public string GroupName { get; set; }
    }
}