using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report16
{
    /// <summary>
    /// 場地使用一覽表的輸出物件。
    /// </summary>
    public class Report16_Output_APIItem : CommonResponseForPagedList<Report16_Output_Row_APIItem>
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
            if (input is Report16_Input_APIItem r16)
            {
                StartDate = r16.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r16.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
            }

            base.SetByInput(input);
        }
    }
}