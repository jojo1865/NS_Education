using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems
{
    public interface IGetResponseRow
    {
        /// <summary>
        /// 依據實際的 DB 物件，設置此欄位的基本欄位。<br/>
        /// 若使用 Helper 時，此方法會被自動呼叫，實作者可以省略。
        /// </summary>
        /// <param name="entity">DB 物件</param>
        /// <param name="controller">Controller 物件</param>
        Task SetInfoFromEntity<T>(T entity, PublicClass controller)
            where T : class;

        /// <summary>
        /// 設定此資訊在列表中為第幾筆。
        /// </summary>
        /// <param name="index">第幾筆</param>
        void SetIndex(int index);
    }
}