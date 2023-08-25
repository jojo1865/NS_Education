using NS_Education.Models.APIItems.Controller.PrintReport.Report11;
using NS_Education.Tools.Extensions;

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

        public string TargetDate { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report11_Input_APIItem r11)
            {
                TargetDate = r11.TargetDate?.ParseDateTime().ToString("yyyy/MM/dd");
            }

            base.SetByInput(input);
        }
    }
}