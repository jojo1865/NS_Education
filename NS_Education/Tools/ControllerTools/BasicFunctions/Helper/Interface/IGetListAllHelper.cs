using System.Collections.Generic;
using System.Threading.Tasks;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    /// <summary>
    /// GetList 功能的預設處理工具介面。針對無須分頁的 GetList 提供。
    /// </summary>
    /// <typeparam name="TGetListRequest">傳入物件類型</typeparam>
    public interface IGetListAllHelper<in TGetListRequest>
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

        /// <summary>
        /// 取得資料列。用於查回資料後，還需要根據資料進行其他操作的情境。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <typeparam name="TRow">資料列的物件類型</typeparam>
        /// <returns>
        /// 成功時：資料列的集合。<br/>
        /// 驗證失敗時：空集合。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<ICollection<TRow>> GetRows<TRow>(TGetListRequest input);
    }
}