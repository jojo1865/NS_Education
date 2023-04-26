using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.Zip.GetList
{
    public class Zip_GetList_Output_Row_APIItem : BaseResponseWithCreUpd<D_Zip>
    {
        public int DZID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public int ParentID { get; set; }
        public string Note { get; set; }
    }
}