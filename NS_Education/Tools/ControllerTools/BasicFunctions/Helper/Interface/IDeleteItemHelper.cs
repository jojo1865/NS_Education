using System.Threading.Tasks;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface
{
    public interface IDeleteItemHelper
    {
        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="input">輸入，參照 <see cref="DeleteItem_Input_APIItem"/></param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        Task<string> DeleteItem(DeleteItem_Input_APIItem input);

        Task<bool> DeleteItemValidateReservation<TReservationEntity>(DeleteItem_Input_APIItem input,
            IDeleteItemValidateReservation<TReservationEntity> validation) where TReservationEntity : class;
    }
}