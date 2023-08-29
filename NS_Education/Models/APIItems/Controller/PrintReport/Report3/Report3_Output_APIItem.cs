using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    /// <summary>
    /// 客戶授權簽核表的輸出物件。
    /// </summary>
    public class Report3_Output_APIItem : CommonResponseForPagedList<Report3_Output_Row_APIItem>
    {
        /// <summary>
        /// 製表者 ID
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 製表者名稱
        /// </summary>
        public string Username { get; set; }

        public IEnumerable<int> RHID { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report3_Input_APIItem r3)
            {
                RHID = r3.RHID;
            }

            base.SetByInput(input);
        }
    }
}