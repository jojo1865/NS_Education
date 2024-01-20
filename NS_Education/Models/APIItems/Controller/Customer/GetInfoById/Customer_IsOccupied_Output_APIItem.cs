namespace NS_Education.Models.APIItems.Controller.Customer.GetInfoById
{
    /// <summary>
    /// 查詢客戶代號是否已被占用的 API 的回傳物件。
    /// </summary>
    public class Customer_IsOccupied_Output_APIItem : BaseInfusable
    {
        /// <summary>
        /// 輸入的代號是否已被占用。
        /// </summary>
        public bool IsOccupied { get; internal set; }
    }
}