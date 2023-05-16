using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_FoodItem_APIItem
    {
        public int RTFID { get; set; }
        
        public int DFCID { get; set; }
        public string DFC_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> DFC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> BSC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int BPID { get; set; }
        public string BP_Title { get; set; }

        public ICollection<BaseResponseRowForSelectable> BP_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }
    }
}