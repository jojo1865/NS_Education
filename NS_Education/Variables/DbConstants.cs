using System.Collections.Generic;
using WebGrease.Css.Extensions;

namespace NS_Education.Variables
{
    public static class DbConstants
    {
        public const string ActiveFlag = "ActiveFlag";
        public const string DeleteFlag = "DeleteFlag";
        public const string CreUid = "CreUID";
        public const string CreDate = "CreDate";
        public const string UpdUid = "UpdUID";
        public const string UpdDate = "UpdDate";
        public const int OpAndMkSalesMappingType = 3;
        public const int OpSalesMappingType = 2;
        public const int MkSalesMappingType = 1;

        /// <summary>
        /// 用於取得安全控管設定中，紀錄保留天數的資料。寫死一個 ID。
        /// </summary>
        public const int SafetyControlLogKeepDaysBSCID = 122;

        /// <summary>
        /// 用於確認預約行程的種類是否為餐飲。
        /// </summary>
        public const string ThrowDineTitle = "餐飲";

        public static readonly IReadOnlyCollection<string> AlwaysShowAddEditMenuUrls = new List<string>
        {
            "/appointment-management"
        }.AsSafeReadOnly();
    }
}