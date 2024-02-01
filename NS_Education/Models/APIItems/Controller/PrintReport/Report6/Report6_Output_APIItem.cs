using System.Collections.Generic;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report6
{
    /// <summary>
    /// 人次統計表的輸出物件。
    /// </summary>
    public class Report6_Output_APIItem : CommonResponseForPagedList<Report6_Output_Row_APIItem>
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
        public string[] Opponents { get; set; }
        public string CustomerName { get; set; }

        public int? State { get; set; }

        public IEnumerable<int> RHID { get; set; }

        public bool Internal { get; set; } = true;
        public bool External { get; set; } = true;

        public bool CommDept { get; set; } = true;

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report6_Input_APIItem r6)
            {
                StartDate = r6.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r6.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
                List<string> opponents = new List<string>();

                if (r6.Internal)
                    opponents.Add("INTERNAL");

                if (r6.External)
                    opponents.Add("EXTERNAL");

                Opponents = opponents.ToArray();

                RHID = r6.RHID;

                CustomerName = r6.CustomerName;
                State = r6.State;

                Internal = r6.Internal;
                External = r6.External;
                CommDept = r6.CommDept;
            }

            base.SetByInput(input);
        }
    }
}