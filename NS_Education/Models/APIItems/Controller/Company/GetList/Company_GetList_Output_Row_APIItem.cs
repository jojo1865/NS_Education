using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Company.GetList
{
    public class Company_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }

        public ICollection<Company_GetList_DepartmentItem_APIItem> DepartmentItems { get; set; } = new List<Company_GetList_DepartmentItem_APIItem>();
        public int DepartmentCt { get; set; }
    }
}