using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report7
{
    /// <summary>
    /// 營運報表明細的輸出物件。
    /// </summary>
    public class Report7_Output_APIItem : CommonResponseForPagedList<Report7_Output_Row_APIItem>
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

        public IEnumerable<int> RHID { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report7_Input_APIItem r7)
            {
                TargetDate = r7.TargetDate;
                RHID = r7.RHID;
            }

            base.SetByInput(input);
        }
    }
}