namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    /// <summary>
    /// 場地庫存狀況表的輸出物件。
    /// </summary>
    public class Report11_Output_APIItem : CommonResponseForPagedList<Report11_Output_Row_APIItem>
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
            if (input is Report11_Input_APIItem r11)
            {
                StartDate = r11.StartDate;
                EndDate = r11.EndDate;
            }

            base.SetByInput(input);
        }
    }
}