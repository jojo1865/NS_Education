namespace NS_Education.Models.APIItems.Controller.PrintReport.Report18
{
    /// <summary>
    /// 日誌贈送報表的輸出物件。
    /// </summary>
    public class Report18_Output_APIItem : CommonResponseForPagedList<Report18_Output_Row_APIItem>
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
        /// 起日
        /// </summary>
        public string SDate { get; set; }

        /// <summary>
        /// 迄日
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string CustomerTitleC { get; set; }

        /// <summary>
        /// 贈送年份
        /// </summary>
        public int? SendYear { get; set; }

        /// <summary>
        /// 禮品
        /// </summary>
        public string Keyword { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report18_Input_APIItem r18)
            {
                SDate = r18.SDate;
                EDate = r18.EDate;
                SendYear = r18.SendYear;
                CustomerTitleC = r18.CustomerTitleC;
                Keyword = r18.Keyword;
            }

            base.SetByInput(input);
        }
    }
}