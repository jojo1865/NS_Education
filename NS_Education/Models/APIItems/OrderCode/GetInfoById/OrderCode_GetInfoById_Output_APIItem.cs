using System.Collections.Generic;
using NS_Education.Tools;

namespace NS_Education.Models.APIItems.OrderCode.GetInfoById
{
    public class OrderCode_GetInfoById_Output_APIItem : cReturnMessageInfusableAbstract
    {
        public int BOCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }
        public List<BaseResponseRowForType> CodeTypeList { get; set; } = new List<BaseResponseRowForType>();
        public string Code { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}