using System.Collections.Generic;
using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems.PrintReport.GetResverListByIds2
{
    public class PrintReport_GetResverListByIds2_Output_Row_APIItem : IGetResponseRow
    {
        public async Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            CreUID = entity.GetIfHasProperty<T, int?>(DbConstants.CreUid);
            CreName = CreUID is null ? default : await controller.GetUserNameByID(CreUID.Value);
        }
        
        public int RHID { get; set; }
        
        // 普遍性欄位
        public int? CreUID { get; private set; }
        public string CreName { get; private set; }
        
        // 其他 API 欄位
        public string PrintDate { get; set; }
        public string Code { get; set; }
        public string CustomerTitle { get; set; }
        public string ContactName { get; set; }
        public string Title { get; set; }
        public int TotalPrice { get; set; }
        public int PaidPrice { get; set; }
        public int Balance => TotalPrice - PaidPrice;
        public string Compilation { get; set; }

        public ICollection<PrintReport_GetResverListByIds2_PayItem_APIItem> Items { get; set; }
            = new List<PrintReport_GetResverListByIds2_PayItem_APIItem>();
    }
}