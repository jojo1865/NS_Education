using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.StaticCode.GetInfoById
{
    public class StaticCode_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int BSCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }
        public List<BaseResponseRowIdTitle> CodeTypeList { get; set; } = new List<BaseResponseRowIdTitle>();
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}