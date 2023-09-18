using System.Collections.Generic;
using NS_Education.Tools.Extensions;

namespace NS_Education.Variables
{
    public static class TableCommentDictionary
    {
        private static readonly IReadOnlyDictionary<string, string> TableComment = new Dictionary<string, string>
        {
            { "B_Category", "分類" },
            { "B_Device", "設備" },
            { "B_OrderCode", "入帳代號" },
            { "B_Partner", "合作廠商" },
            { "B_PartnerItem", "合作廠商房型" },
            { "B_SiteData", "場地" },
            { "B_StaticCode", "靜態參數" },
            { "BusinessUser", "業務負責人" },
            { "Customer", "客戶" },
            { "CustomerQuestion", "客戶問題紀錄" },
            { "CustomerVisit", "客戶拜訪紀錄" },
            { "D_Company", "公司資料" },
            { "D_Department", "部門" },
            { "D_FoodCategory", "用餐" },
            { "D_Hall", "廳別" },
            { "D_OtherPayItem", "其他收費項目" },
            { "D_PayType", "付款方式" },
            { "D_Throw", "行程" },
            { "D_TimeSpan", "時段" },
            { "D_Zip", "國籍與郵遞區號" },
            { "ErrorLog", "系統錯誤記錄檔" },
            { "GiftSending", "禮品贈與紀錄" },
            { "GroupData", "權限" },
            { "Lookup", "對照檔編輯選單檔" },
            { "M_Address", "地址對照檔" },
            { "M_Contect", "聯絡方式對照" },
            { "M_Customer_BusinessUser", "客戶業務對照" },
            { "M_Customer_Category", "客戶類別對照" },
            { "M_Customer_Gift", "客戶禮品贈與對應檔" },
            { "M_Department_Category", "部門類別對照" },
            { "M_Group_Menu", "權限選單對照" },
            { "M_Group_User", "權限使用者對照" },
            { "M_Resver_TimeSpan", "預約時段對照" },
            { "M_Site_Device", "場地設備對照檔" },
            { "M_SiteGroup", "場地組合" },
            { "MenuAPI", "選單對應端點" },
            { "MenuData", "選單" },
            { "Resver_Bill", "預約繳費紀錄" },
            { "Resver_Device", "預約設備" },
            { "Resver_GiveBack", "預約回饋" },
            { "Resver_Head", "預約單" },
            { "Resver_Head_Log", "預約單操作歷史紀錄檔" },
            { "Resver_Other", "預約其他收費項目" },
            { "Resver_Site", "預約場地" },
            { "Resver_Throw", "預約行程" },
            { "Resver_Throw_Food", "預約行程補充餐飲資料" },
            { "UserData", "使用者" },
            { "UserLog", "使用者操作紀錄" },
            { "UserPasswordLog", "使用者變更密碼或登入登出紀錄" }
        };

        public static string GetCommentByTableName(string tableName)
        {
            TableComment.TryGetValue(tableName, out string comment);

            if (comment != null && comment.HasContent() && !comment.EndsWith("檔"))
                comment += "檔";

            return comment;
        }
    }
}