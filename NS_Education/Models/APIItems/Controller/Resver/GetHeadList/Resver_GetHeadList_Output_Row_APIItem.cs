using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.Resver.GetHeadList
{
    public class Resver_GetHeadList_Output_Row_APIItem : IGetResponseRow
    {
        public int RHID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public string CustomerTitle { get; set; }
        public string CustomerCode { get; set; }
        public int PeopleCt { get; set; }

        public int BSCID12 { get; set; }
        public string BSCID12_Title { get; set; }

        public bool DeleteFlag { get; set; }
        public int Index { get; private set; }

        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            DeleteFlag = entity.GetIfHasProperty<T, bool>(nameof(DeleteFlag));
            return Task.CompletedTask;
            // 不做任何事。這個回傳沒有任何普遍性欄位。
        }

        /// <inheritdoc />
        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}