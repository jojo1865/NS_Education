using System.Threading.Tasks;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    /// <summary>
    /// GetList 功能的預設處理工具介面。傳入時單純只執行 GET 而無其他參數，且不與 DB 互動，回傳所有掌管資料。
    /// </summary>
    public interface IGetListLocalHelper
    {
        /// <summary>
        /// 取得所有資料的列表。
        /// </summary>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetListLocal();
    }
}