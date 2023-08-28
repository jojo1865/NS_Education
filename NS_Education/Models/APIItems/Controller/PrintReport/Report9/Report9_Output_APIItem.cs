using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report9
{
    /// <summary>
    /// 客戶歷史資料報表的輸出物件。
    /// </summary>
    public class Report9_Output_APIItem : CommonResponseForPagedList<Report9_Output_Row_APIItem>
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
        public bool Internal { get; set; }
        public bool External { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report9_Input_APIItem r9)
            {
                StartDate = r9.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r9.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
                External = r9.External;
                Internal = r9.Internal;
            }

            base.SetByInput(input);
        }
    }
}