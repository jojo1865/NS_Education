using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Controller.StaticCode.GetTypeList
{
    public class StaticCode_GetTypeList_Output_Row_APIItem : IGetResponseRow
    {
        public int BSCID { get; set; }
        public int CodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
        public int Index { get; private set; }

        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            // 這個輸出不需要任何普遍性欄位。
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}