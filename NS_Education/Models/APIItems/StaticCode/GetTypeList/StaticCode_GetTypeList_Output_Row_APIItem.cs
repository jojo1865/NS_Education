using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.StaticCode.GetTypeList
{
    public class StaticCode_GetTypeList_Output_Row_APIItem : IGetResponseRow
    {
        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            // 這個輸出不需要任何普遍性欄位。
            return Task.CompletedTask;
        }
        
        public int BSCID { get; set; }
        public int CodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
    }
}