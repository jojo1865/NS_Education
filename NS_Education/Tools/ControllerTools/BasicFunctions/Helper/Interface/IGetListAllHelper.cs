using System.Threading.Tasks;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    /// <summary>
    /// GetList 功能的預設處理工具介面。針對無須分頁的 GetList 提供。
    /// </summary>
    /// <typeparam name="TGetListRequest">傳入物件類型</typeparam>
    public interface IGetListAllHelper<in TGetListRequest>
    where TGetListRequest : BaseRequestForList
    {
        /// <summary>
        /// 取得符合條件的無分頁整批資料列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetAllList(TGetListRequest input);
    }
}