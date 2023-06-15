using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.OtherPayItem.GetInfoById
{
    public class OtherPayItem_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DOPIID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int PaidType { get; set; }
        public int BSCID { get; set; }

        public IEnumerable<BaseResponseRowForSelectable> BSC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();

        public int BOCID { get; set; }
    }
}