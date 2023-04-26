using System.Threading.Tasks;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    public interface IDeleteItemHelper
    {
        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <param name="deleteFlag">欲設定的刪除狀態新值</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        Task<string> DeleteItem(int id, bool? deleteFlag);
    }
}