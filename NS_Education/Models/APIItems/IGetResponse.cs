using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems
{
    public interface IGetResponse
    {
        /// <summary>
        /// 依據實際的 DB 物件，設置此欄位的基本欄位。<br/>
        /// 若使用 Helper 時，此方法會被自動呼叫，實作者可以省略。
        /// </summary>
        /// <param name="entity">DB 物件</param>
        /// <param name="controller">Controller 物件</param>
        Task SetInfoFromEntity(object entity, PublicClass controller);
    }
}