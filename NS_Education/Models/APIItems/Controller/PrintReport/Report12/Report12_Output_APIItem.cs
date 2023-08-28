namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率一覽表的輸出物件。
    /// </summary>
    public class Report12_Output_APIItem : CommonResponseForPagedList<Report12_Output_Row_APIItem>
    {
        /// <summary>
        /// 製表者 ID
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 製表者名稱
        /// </summary>
        public string Username { get; set; }

        public int PeriodTotal { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TotalUsage { get; set; }
        public int TotalAreaSize { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report12_Input_APIItem r12)
            {
                PeriodTotal = r12.PeriodTotal;
                StartDate = r12.StartDate;
                EndDate = r12.EndDate;
            }

            base.SetByInput(input);
        }
    }
}