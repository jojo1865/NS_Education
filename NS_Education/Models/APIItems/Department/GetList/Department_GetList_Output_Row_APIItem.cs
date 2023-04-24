using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.Department.GetList
{
    public class Department_GetList_Output_Row_APIItem : BaseResponseWithCreUpd<D_Department>
    {
        public int DDID { get; set; }
        public int DCID { get; set; }
        public string DC_TitleC { get; set; }
        public string DC_TitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int PeopleCt { get; set; }
        public int HallCt { get; set; }
    }
}