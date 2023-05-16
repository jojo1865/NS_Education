using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.BusinessUser.GetList
{
    public class BusinessUser_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int BUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool MKsalesFlag { get; set; }
        public bool OPsalesFlag { get; set; }

        public ICollection<BusinessUser_GetList_Customer_APIItem> Items { get; set; } =
            new List<BusinessUser_GetList_Customer_APIItem>();
    }
}