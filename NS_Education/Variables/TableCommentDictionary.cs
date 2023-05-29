using System.Collections.Generic;
using System.Linq;
using NS_Education.Tools.Extensions;

namespace NS_Education.Variables
{
    public static class TableCommentDictionary
    {
        private static readonly IDictionary<string, string> TableComment = new[]
        {
            new KeyValuePair<string, string>("M_Department_Category", "部門類別對照"),
            new KeyValuePair<string, string>("M_Customer_Category", "客戶類別對照"),
            new KeyValuePair<string, string>("BusinessUser", "業務負責人"),
            new KeyValuePair<string, string>("Customer", "客戶"),
            new KeyValuePair<string, string>("Resver_Head", "預約單"),
            new KeyValuePair<string, string>("CustomerVisit", "客戶拜訪紀錄"),
            new KeyValuePair<string, string>("CustomerQuestion", "客戶問題紀錄"),
            new KeyValuePair<string, string>("CustomerGift", "客戶禮品贈與紀錄"),
            new KeyValuePair<string, string>("B_Category", "分類"),
            new KeyValuePair<string, string>("M_Resver_TimeSpan", "預約時段對照"),
            new KeyValuePair<string, string>("M_Contect", "聯絡方式對照"),
            new KeyValuePair<string, string>("MenuData", "選單"),
            new KeyValuePair<string, string>("GroupData", "權限"),
            new KeyValuePair<string, string>("M_Group_Menu", "權限選單對照"),
            new KeyValuePair<string, string>("B_OrderCode", "入帳代號"),
            new KeyValuePair<string, string>("M_Group_User", "權限使用者對照"),
            new KeyValuePair<string, string>("UserLog", "使用者操作紀錄"),
            new KeyValuePair<string, string>("M_SiteGroup", "場地組合"),
            new KeyValuePair<string, string>("D_Hall", "廳別"),
            new KeyValuePair<string, string>("MenuAPI", "選單對應端點"),
            new KeyValuePair<string, string>("Resver_Device", "預約設備"),
            new KeyValuePair<string, string>("Resver_GiveBack", "預約回饋"),
            new KeyValuePair<string, string>("M_Customer_BusinessUser", "客戶業務對照"),
            new KeyValuePair<string, string>("D_TimeSpan", "時段"),
            new KeyValuePair<string, string>("D_Company", "公司資料"),
            new KeyValuePair<string, string>("B_StaticCode", "靜態參數"),
            new KeyValuePair<string, string>("Resver_Throw_Food", "預約行程補充餐飲資料"),
            new KeyValuePair<string, string>("Resver_Site", "預約場地"),
            new KeyValuePair<string, string>("D_FoodCategory", "用餐"),
            new KeyValuePair<string, string>("D_OtherPayItem", "其他收費項目"),
            new KeyValuePair<string, string>("D_PayType", "付款方式"),
            new KeyValuePair<string, string>("Resver_Throw", "預約行程"),
            new KeyValuePair<string, string>("UserData", "使用者"),
            new KeyValuePair<string, string>("B_SiteData", "場地"),
            new KeyValuePair<string, string>("Resver_Other", "預約其他收費項目"),
            new KeyValuePair<string, string>("Resver_Bill", "預約繳費紀錄"),
            new KeyValuePair<string, string>("D_Department", "部門"),
            new KeyValuePair<string, string>("B_PartnerItem", "合作廠商房型"),
            new KeyValuePair<string, string>("B_Partner", "合作廠商"),
            new KeyValuePair<string, string>("UserPasswordLog", "使用者變更密碼或登入登出紀錄"),
            new KeyValuePair<string, string>("B_Device", "設備"),
            new KeyValuePair<string, string>("D_Zip", "國籍與郵遞區號")
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public static string GetCommentByTableName(string tableName)
        {
            TableComment.TryGetValue(tableName, out string comment);

            if (comment != null && comment.HasContent() && !comment.EndsWith("檔"))
                comment += "檔";

            return comment;
        }
    }
}