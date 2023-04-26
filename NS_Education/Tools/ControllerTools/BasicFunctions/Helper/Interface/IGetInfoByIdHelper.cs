using System.Threading.Tasks;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    public interface IGetInfoByIdHelper
    {
        /// <summary>
        /// 取得單筆資料。
        /// </summary>
        /// <param name="id">查詢用的索引鍵</param>
        /// <returns>
        /// 成功時：包含資料的通用訊息回傳格式。<br/>
        /// 輸入驗證失敗，或查無資料時：不包含資料的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetInfoById(int id);
    }
}