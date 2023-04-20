using System.Threading.Tasks;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    /// <summary>
    /// GetList 功能的預設處理工具介面。
    /// </summary>
    /// <typeparam name="TGetListRequest">傳入物件類型</typeparam>
    public interface IGetPagedListHelper<in TGetListRequest>
    where TGetListRequest : class
    {
        /// <summary>
        /// 取得符合條件的列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗，或找不到資料時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetPagedList(TGetListRequest input);
    }
}