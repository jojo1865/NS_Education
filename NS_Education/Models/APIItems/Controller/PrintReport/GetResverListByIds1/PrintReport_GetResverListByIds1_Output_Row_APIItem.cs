using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds1
{
    public class PrintReport_GetResverListByIds1_Output_Row_APIItem : IGetResponseRow
    {
        public ICollection<PrintReport_GetResverListByIds1_SiteItem_APIItem> SiteItems =
            new List<PrintReport_GetResverListByIds1_SiteItem_APIItem>();

        public int RHID { get; set; }

        // 普遍性欄位
        public int? CreUID { get; private set; }
        public string CreName { get; private set; }
        public string CreDate { get; private set; }

        // 自有欄位

        public string Code { get; set; }
        public string CustomerTitle { get; set; }
        public string ContactTitle1 { get; set; }
        public string ContactValue1 { get; set; }
        public string ContactTitle2 { get; set; }
        public string ContactValue2 { get; set; }
        public string ContactName { get; set; }
        public string Compilation { get; set; }
        public string Title { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public int PeopleCt { get; set; }
        public int QuotedPrice { get; set; }
        public int Index { get; private set; }

        public async Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            CreUID = entity.GetIfHasProperty<T, int?>(DbConstants.CreUid);
            CreName = CreUID is int i ? await controller.GetUserNameByID(i) : null;
            CreDate = entity.GetIfHasProperty<T, DateTime?>(DbConstants.CreDate) is DateTime dt
                ? dt.ToFormattedStringDateTime()
                : null;
        }

        /// <inheritdoc />
        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}