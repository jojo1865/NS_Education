using System;

namespace NS_Education.Tools.ExcelBuild
{
    /// <summary>
    /// 在 ExcelBuilder 中，提供製表者等基本資訊的 DTO。
    /// </summary>
    public class ExcelBuilderInfo
    {
        /// <summary>
        /// 製表者 ID
        /// </summary>
        public string CreatorId { get; set; }

        /// <summary>
        /// 製表者姓名
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// 製表日
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        /// <summary>
        /// 當前頁次
        /// </summary>
        public int NowPage { get; set; } = 1;

        /// <summary>
        /// 最大頁數
        /// </summary>
        public int TotalPage { get; set; } = 1;
    }
}