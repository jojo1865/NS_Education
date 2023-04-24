using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// Submit 功能 API 端點的介面。
    /// </summary>
    /// <typeparam name="TEntity">掌管資料的類型</typeparam>
    /// <typeparam name="TSubmitRequest">傳入要求的類型</typeparam>
    public interface ISubmit<TEntity, in TSubmitRequest>
      where TSubmitRequest : BaseRequestForSubmit
    {
        /// <summary>
        /// 新增或更新一筆資料。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入驗證失敗時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 新增，但 DB 連線異常時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 修改，但查無資料，或 DB 連線異常時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他錯誤時：拋錯。
        /// </returns>
        Task<string> Submit(TSubmitRequest input);
        
        /// <summary>
        /// 新增或更新一筆資料時，判定此次要求是否為新增。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// true：為新增。<br/>
        /// false：不是新增（視為更新）。
        /// </returns>
        [NonAction]
        bool SubmitIsAdd(TSubmitRequest input);
        
        /// <summary>
        /// 新增一筆資料時，驗證輸入格式。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證錯誤。
        /// </returns>
        [NonAction]
        Task<bool> SubmitAddValidateInput(TSubmitRequest input);
        
        /// <summary>
        /// 新增一筆資料時，依據輸入建立新物件的方法。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>欲新增的物件</returns>
        /// <remarks>若此方法是藉由預設的 Submit 方法被呼叫時，實作者對 CreUid、CreDate、UpdUid、UpdDate 及 DeleteFlag 的設定將被覆寫。</remarks>
        [NonAction]
        Task<TEntity> SubmitCreateData(TSubmitRequest input);
        
        /// <summary>
        /// 更新一筆資料時，驗證輸入格式。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證錯誤。
        /// </returns>
        [NonAction]
        Task<bool> SubmitEditValidateInput(TSubmitRequest input);
        
        /// <summary>
        /// 更新一筆資料時，找出原資料的查詢。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>查詢</returns>
        /// <remarks>若此方法是藉由預設的 Submit 方法被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        [NonAction]
        IQueryable<TEntity> SubmitEditQuery(TSubmitRequest input);
        
        /// <summary>
        /// 更新一筆資料時，依據輸入覆寫資料各欄位的方法。
        /// </summary>
        /// <param name="data">DB 資料</param>
        /// <param name="input">輸入</param>
        /// <remarks>若此方法是藉由預設的 Submit 方法被呼叫時，實作者在更新時對 UpdUid 及 UpdDate 的更新將被覆寫。</remarks>
        [NonAction]
        void SubmitEditUpdateDataFields(TEntity data, TSubmitRequest input);
    }
}