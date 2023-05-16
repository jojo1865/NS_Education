using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Company.GetInfoById
{
    public class Company_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }

        public ICollection<BaseResponseRowForSelectable> BC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int DepartmentCt { get; set; }
    }
}