using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_BillItem_APIItem
    {
        public int RBID { get; set; }
        
        public int BCID { get; set; }
        public string BC_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> BC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int DPTID { get; set; }
        public string DPT_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> DPT_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int Price { get; set; }
        public string Note { get; set; }
        public bool PayFlag { get; set; }
        public string PayDate { get; set; }
    }
}