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

        /// <summary>
        /// 這個參數用於控制「預約管理」這種無論角色、權限設定，總是開放部分權限的選單。<br/>
        /// 因為這種設定必須跨角色、跨權限，在我們的系統設計上，等於每新增角色或是使用者時，都必須綁定一個對應該選單權限的 M_GroupMenu。<br/>
        /// 但這樣一來，刪除資料上就變得很難控制，因為我們不知道某筆權限資料是否是這類預設權限而不得刪除，也很容易會需要在後端寫死 ID 來做處理。<br/>
        /// 這裡的解法是，在權限取得（GroupData、MenuData 等的 Get 相關 APIs）做特例處理，這類選單的 MenuAPIs 必定提供部分權限（e.g. 在 /MenuData/GetListByUID 等端點的查詢結果中總是顯示為 true）。<br/>
        /// 權限驗證的部分，則單純把這類 MenuData 對應的需要總是開放的 API 設為不需任何權限。（e.g. 預約管理類的瀏覽、更新、新增型 APIs 只需要登入，不需要任何細項權限）<br/>
        /// <br/>
        /// 簡單來說，假如這部分的設計沒有改動，如果未來有新的選單也要這麼做，我們需要做兩件事：<br/>
        /// 1. 把相關的 Controller Action 的 [JwtAuth] 設定，所需 Privileges 設定為 None。<br/>
        /// 2. 把新選單的網址加到這個集合，並且到引用這個集合的地方確認實作沒有問題。
        /// </summary>
        public static readonly IReadOnlyCollection<string> AlwaysShowAddEditMenuUrls = new List<string>
        {
            "/appointment-management"
        }.AsSafeReadOnly();
    }
}
