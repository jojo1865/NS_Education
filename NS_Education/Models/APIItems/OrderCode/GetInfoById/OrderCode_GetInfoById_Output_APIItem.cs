using System.Collections.Generic;

namespace NS_Education.Models.APIItems.OrderCode.GetInfoById
{
    public class OrderCode_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
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
    }
}