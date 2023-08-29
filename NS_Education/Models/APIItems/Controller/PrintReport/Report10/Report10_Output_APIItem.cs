using System.Collections.Generic;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report10
{
    /// <summary>
    /// 未成交原因分析的輸出物件。
    /// </summary>
    public class Report10_Output_APIItem : CommonResponseForPagedList<Report10_Output_Row_APIItem>
    {
        /// <summary>
        /// 製表者 ID
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 製表者名稱
        /// </summary>
        public string Username { get; set; }

        public string TargetDate { get; set; }

        public IEnumerable<int> CVID { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report10_Input_APIItem r10)
            {
                TargetDate = r10.TargetDate?.ParseDateTime().ToString("yyyy/MM/dd");
                CVID = r10.CVID;
            }

            base.SetByInput(input);
        }
    }
}