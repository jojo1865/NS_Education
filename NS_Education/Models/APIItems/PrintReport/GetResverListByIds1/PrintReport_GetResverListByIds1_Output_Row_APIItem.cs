using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems.PrintReport.GetResverListByIds1
{
    public class PrintReport_GetResverListByIds1_Output_Row_APIItem : IGetResponse

    {
        public async Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            CreUID = entity.GetIfHasProperty<T, int?>(DbConstants.CreUid);
            CreName = CreUID is int i ? await controller.GetUserNameByID(i) : null;
            CreDate = entity.GetIfHasProperty<T, DateTime?>(DbConstants.CreDate) is DateTime dt
                ? dt.ToFormattedStringDateTime()
                : null;
        }
        
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

        public ICollection<PrintReport_GetResverListByIds1_SiteItem_APIItem> SiteItems =
            new List<PrintReport_GetResverListByIds1_SiteItem_APIItem>();
    }

    public class PrintReport_GetResverListByIds1_SiteItem_APIItem
    {
        public int RSID { get; set; }
        public string Date { get; set; }
        public string SiteTitle { get; set; }
        public string TableTitle { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }

        public ICollection<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem> TimeSpanItems =
            new List<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem>();
        
        public ICollection<PrintReport_GetResverListByIds1_DeviceItem_APIItem> DeviceItems =
            new List<PrintReport_GetResverListByIds1_DeviceItem_APIItem>();
    }

    public class PrintReport_GetResverListByIds1_DeviceItem_APIItem
    {
        public int RDID { get; set; }
        public string TargetDate { get; set; }
        public string BD_Title { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
    }

    public class PrintReport_GetResverListByIds1_TimeSpanItem_APIItem
    {
        public int DTSID { get; set; }
        public string Title { get; set; }
        public string TimeS { get; set; }
        public string TimeE { get; set; }
        public int Minutes { get; set; }
    }
}