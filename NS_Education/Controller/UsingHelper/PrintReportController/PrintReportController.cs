using System.Threading.Tasks;
using NS_Education.Models.APIItems.Controller.PrintReport.Distribution;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 處理報表列印的控制器。這個控制器負責基於輸入的 ReportType，進行對應的處理。
    /// </summary>
    public class PrintReportController : PublicClass
    {
        #region Distribution

        /// <summary>
        /// 基於 ReportType，處理
        /// </summary>
        /// <returns></returns>
        public async Task<string> Distribution(object input)
        {
            PrintReport_Distribution_Input_APIItem request = (PrintReport_Distribution_Input_APIItem)input;

            switch (request.ReportType)
            {
                default:
                    AddError(NotSupportedValue("報表種類", nameof(request.ReportType), "不支援此報表種類"));
                    break;
            }

            return GetResponseJson();
        }

        #endregion
    }
}