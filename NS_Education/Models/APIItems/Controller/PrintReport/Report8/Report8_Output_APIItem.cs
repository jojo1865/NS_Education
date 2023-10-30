using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report8
{
    /// <summary>
    /// 滿意度調查表報表的輸出物件。
    /// </summary>
    public class Report8_Output_APIItem : CommonResponseForPagedList<Report8_Output_Row_APIItem>
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
            if (input is Report8_Input_APIItem r8)
            {
                StartDate = r8.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r8.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
            }

            base.SetByInput(input);
        }
    }
}