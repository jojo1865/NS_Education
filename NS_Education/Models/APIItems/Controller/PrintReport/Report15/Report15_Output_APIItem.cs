using NS_Education.Models.APIItems.Controller.PrintReport.Report14;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report13
{
    /// <summary>
    /// 場地實際銷售統計表的輸出物件。
    /// </summary>
    public class Report15_Output_APIItem : CommonResponseForPagedList<Report15_Output_Row_APIItem>
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
            if (input is Report15_Input_APIItem r15)
            {
                StartDate = r15.StartDate?.ParseDateTime().ToString("yyyy/MM/dd");
                EndDate = r15.EndDate?.ParseDateTime().ToString("yyyy/MM/dd");
            }

            base.SetByInput(input);
        }
    }
}