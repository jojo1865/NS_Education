using System.Collections.Generic;
using NS_Education.Models.APIItems.Controller.OrderCode.GetTypeList;

namespace NS_Education.Models.APIItems.Controller.OrderCode.GetInfoById
{
    public class OrderCode_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BOCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }

        public List<OrderCode_GetTypeList_Output_APIItem> CodeTypeList { get; set; } =
            new List<OrderCode_GetTypeList_Output_APIItem>();

        public string Code { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int SortNo { get; set; }
    }
}