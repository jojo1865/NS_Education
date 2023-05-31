using System.Collections.Generic;
using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Controller.Resver.GetResverSiteList
{
    public class Resver_GetResverSiteList_Output_Row_APIItem : IGetResponseRow
    {
        public ICollection<Resver_GetResverSiteList_TimeSpan_Output_APIItem> Items =
            new List<Resver_GetResverSiteList_TimeSpan_Output_APIItem>();

        public int BSID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int BOCID { get; set; }
        public string BOC_Code { get; set; }
        public string BOC_Title { get; set; }
        public string BOC_PrintTitle { get; set; }
        public string BOC_PrintNote { get; set; }

        public int Index { get; set; }

        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            return Task.CompletedTask;
            // 不做任何事。這個輸出沒有任何普遍欄位
        }

        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}