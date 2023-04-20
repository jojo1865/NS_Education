using System.Threading.Tasks;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    /// <summary>
    /// GetTypeList 功能的預設處理工具介面。
    /// </summary>
    public interface IGetTypeListHelper
    {
        /// <summary>
        /// 取得資料的類別列表的預設處理。
        /// </summary>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗，或找不到資料時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetTypeList();
    }
}