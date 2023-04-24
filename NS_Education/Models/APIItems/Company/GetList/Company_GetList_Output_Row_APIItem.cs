using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.Company.GetList
{
    public class Company_GetList_Output_Row_APIItem : BaseResponseWithCreUpd<D_Company>
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int DepartmentCt { get; set; }
    }
}