using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.GetAllInfoById
{
    public class Resver_GetAllInfoById_Output_FoodItem_APIItem
    {
        /// <summary>
        /// 所屬行程的獨特 ID，提供給前端加總時用。
        /// </summary>
        public int ParentID { get; set; }

        public int RTFID { get; set; }

        public int DFCID { get; set; }
        public string DFC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> DFC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int BPID { get; set; }
        public string BP_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BP_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }

        public string ArriveTime { get; set; }
    }
}