using System.Collections.Generic;
using System.Linq;
using NS_Education.Models.Entities;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// 主要用為 IDeleteItem 的擴充介面。用於在刪除時檢查沒有進行中的預約單。
    /// </summary>
    public interface IDeleteItemValidateReservation<TReservationEntity>
        where TReservationEntity : class
    {
        /// <summary>
        /// 將 query 加上能夠導向 TReservationEntity 的處理，並篩選 ID 符合 uniqueDeleteId。
        /// </summary>
        /// <param name="basicQuery">已篩選進行中預約單的查詢</param>
        /// <param name="uniqueDeleteId">欲刪除的 ID 的集合</param>
        /// <returns>TReservationEntity 的查詢</returns>
        IQueryable<TReservationEntity> SupplyQueryWithInputIdCondition(IQueryable<Resver_Head> basicQuery,
            HashSet<int> uniqueDeleteId);

        /// <summary>
        /// 從 TReservationEntity 中取得對應的欲刪除資料的 ID。
        /// </summary>
        /// <param name="cantDelete">TReservationEntity</param>
        /// <returns>欲刪除資料的 ID</returns>
        object GetInputId(TReservationEntity cantDelete);

        /// <summary>
        /// 從 TReservationEntity 中取得預約單 ID。
        /// </summary>
        /// <param name="cantDelete">TReservationEntity</param>
        /// <returns>預約單 ID</returns>
        int GetHeadId(TReservationEntity cantDelete);
    }
}