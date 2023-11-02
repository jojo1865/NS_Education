namespace NS_Education.Models.APIItems.Controller.PrintReport.Report10
{
    /// <summary>
    /// 未成交原因分析的輸出物件。
    /// </summary>
    public class Report10_Output_APIItem : CommonResponseForPagedList<Report10_Output_Row_APIItem>
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
        public int? CID { get; set; }
        public int? BSCID { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report10_Input_APIItem r10)
            {
                StartDate = r10.StartDate;
                EndDate = r10.EndDate;
                CID = r10.CID;
                BSCID = r10.BSCID;
            }

            base.SetByInput(input);
        }
    }
}