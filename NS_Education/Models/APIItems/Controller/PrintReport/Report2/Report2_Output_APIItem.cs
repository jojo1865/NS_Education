using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report2
{
    /// <summary>
    /// Function Order 的輸出物件。
    /// </summary>
    public class Report2_Output_APIItem : CommonResponseForPagedList<Report2_Output_Row_APIItem>
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
            if (input is Report2_Input_APIItem r2)
            {
                RHID = r2.RHID;
            }

            base.SetByInput(input);
        }
    }
}