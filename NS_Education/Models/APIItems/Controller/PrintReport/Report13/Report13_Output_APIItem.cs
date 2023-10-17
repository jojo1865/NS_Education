namespace NS_Education.Models.APIItems.Controller.PrintReport.Report13
{
    /// <summary>
    /// 場地預估銷售月報表的輸出物件。
    /// </summary>
    public class Report13_Output_APIItem : CommonResponseForPagedList<Report13_Output_Row_APIItem>
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
            if (input is Report13_Input_APIItem r13)
            {
                StartDate = r13.StartDate;
                EndDate = r13.EndDate;
                SiteName = r13.SiteName;
                BCID = r13.BCID;
                IsActive = r13.IsActive;
                BSCID1 = r13.BSCID1;
                BasicSize = r13.BasicSize;
            }

            base.SetByInput(input);
        }
    }
}