using System.Collections.Generic;
using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.StaticCode.GetInfoById
{
    public class StaticCode_GetInfoById_Output_APIItem : BaseResponseWithCreUpdInfusable<B_StaticCode>
    {
        public int BSCID { get; set; }
        public int iCodeType { get; set; }
        public string sCodeType { get; set; }
        public List<BaseResponseRowForType> CodeTypeList { get; set; } = new List<BaseResponseRowForType>();
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }
}