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

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report13_Input_APIItem r13)
            {
                StartDate = r13.StartDate;
                EndDate = r13.EndDate;
            }

            base.SetByInput(input);
        }
    }
}