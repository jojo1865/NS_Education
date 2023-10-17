using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report14
{
    /// <summary>
    /// 場地預估銷售月報表的輸出物件。
    /// </summary>
    public class Report14_Output_APIItem : CommonResponseForPagedList<Report14_Output_Row_APIItem>
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
            if (input is Report14_Input_APIItem r14)
            {
                StartDate = r14.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r14.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
                SiteName = r14.SiteName;
                BCID = r14.BCID;
                IsActive = r14.IsActive;
                BSCID1 = r14.BSCID1;
                BasicSize = r14.BasicSize;
            }

            base.SetByInput(input);
        }
    }
}