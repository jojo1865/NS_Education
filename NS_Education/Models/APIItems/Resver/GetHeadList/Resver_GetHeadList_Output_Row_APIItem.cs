using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Resver.GetHeadList
{
    public class Resver_GetHeadList_Output_Row_APIItem : IGetResponse
    {
        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            return Task.CompletedTask;
            // 不做任何事。這個回傳沒有任何普遍性欄位。
        }
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
    }
}