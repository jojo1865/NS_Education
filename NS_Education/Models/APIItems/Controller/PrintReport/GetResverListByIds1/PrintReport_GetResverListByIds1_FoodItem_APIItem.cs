using System;

namespace NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds1
{
    /// <summary>
    /// 報價暨預約確認單
    /// </summary>
    public class PrintReport_GetResverListByIds1_FoodItem_APIItem
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 餐種
        /// </summary>
        public string FoodType { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 單價
        /// </summary>
        public decimal SinglePrice { get; set; }

        /// <summary>
        /// 總價
        /// </summary>
        public decimal TotalPrice => SinglePrice * Count;
    }
}