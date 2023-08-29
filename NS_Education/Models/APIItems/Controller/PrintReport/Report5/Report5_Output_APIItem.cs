using System.Collections.Generic;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report5
{
    /// <summary>
    /// 餐飲明細表的輸出物件。
    /// </summary>
    public class Report5_Output_APIItem : CommonResponseForPagedList<Report5_Output_Row_APIItem>
    {
        /// <summary>
        /// 製表者 ID
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 製表者名稱
        /// </summary>
        public string Username { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Partner { get; set; }
        public IEnumerable<int> RHID { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report5_Input_APIItem r5)
            {
                StartDate = r5.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r5.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
                Partner = r5.Partner;
                RHID = r5.RHID;
            }

            base.SetByInput(input);
        }
    }
}