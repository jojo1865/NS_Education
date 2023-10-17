using System;

namespace NS_Education.Models.Utilities.PrintReport
{
    public class PdfColumn<TOutput>
    {
        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 用於 RelativeColumn 的寬度權重
        /// </summary>
        public int LengthWeight { get; set; }
        
        /// <summary>
        /// 此欄位取值的定義
        /// </summary>
        public Func<TOutput, object> Selector { get; set; }

        /// <summary>
        /// 此欄位格式化的定義
        /// </summary>
        public Func<object, string> Formatter { get; set; } = obj => obj.ToString();
        
        /// <summary>
        /// 是否輸出合計
        /// </summary>
        public bool OutputTotal { get; set; }
    }
}