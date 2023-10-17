using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    /// <summary>
    /// 場地庫存狀況表的輸出物件。
    /// </summary>
    public class Report11_Output_APIItem : CommonResponseForPagedList<IDictionary<string, string>>
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

        public string SiteName { get; set; }
        public int? BCID { get; set; }
        public bool? IsActive { get; set; }
        public int? BSCID1 { get; set; }
        public int? BasicSize { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report11_Input_APIItem r11)
            {
                StartDate = r11.StartDate;
                EndDate = r11.EndDate;
                SiteName = r11.SiteName;
                BCID = r11.BCID;
                IsActive = r11.IsActive;
                BSCID1 = r11.BSCID1;
                BasicSize = r11.BasicSize;
            }

            base.SetByInput(input);
        }
    }
}