using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Controller.Customer.GetUniqueNames
{
    public class Customer_GetUniqueNames_Output_Row_APIItem : IGetResponseRow
    {
        public int Index { get; private set; }

        public int CID { get; set; }
        public string TitleC { get; set; }

        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            // 這個物件沒有普遍性欄位需要設定。
            return Task.CompletedTask;
        }

        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}