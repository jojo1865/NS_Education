using System.Collections.Generic;
using NsEduCore.Responses.BaseResponse;

namespace NsEduCore.Responses.Company
{
    /// <summary>
    /// 取得單筆公司資料的回傳物件。
    /// </summary>
    public class CompanyGetResponse : BaseResponse.BaseResponse
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }
        public List<BaseResponseItem> CategoryList { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int DepartmentCt { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
    }
}