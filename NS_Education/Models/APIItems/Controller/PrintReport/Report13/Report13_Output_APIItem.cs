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

        /// <summary>
        /// 查詢月份，格式 yyyy/MM，要求若未傳入此欄位則為 null。
        /// </summary>
        public string TargetMonth { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report13_Input_APIItem r13)
                TargetMonth = r13.TargetMonth;

            base.SetByInput(input);
        }
    }
}