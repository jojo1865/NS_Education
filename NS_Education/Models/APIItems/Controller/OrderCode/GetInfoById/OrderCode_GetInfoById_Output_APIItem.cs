using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.OrderCode.GetInfoById
{
    public class OrderCode_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BOCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }
        public List<CommonResponseRowIdTitle> CodeTypeList { get; set; } = new List<CommonResponseRowIdTitle>();
        public string Code { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int SortNo { get; set; }
    }
}