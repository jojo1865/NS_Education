using System.Collections.Generic;
using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Controller.Customer.GetRankings
{
    public class Customer_GetRankings_Output_Row_APIItem : IGetResponseRow
    {
        public int Index { get; set; }
        public int RentCt { get; set; }
        public string QuotedTotal { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Industry { get; set; }
        public string ContactName { get; set; }

        public IEnumerable<Customer_GetRankings_Output_Contact_APIItem> Contacts { get; set; } =
            new List<Customer_GetRankings_Output_Contact_APIItem>();

        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            // 這個物件沒有需要設定的普遍性欄位。
            return Task.CompletedTask;
        }

        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}