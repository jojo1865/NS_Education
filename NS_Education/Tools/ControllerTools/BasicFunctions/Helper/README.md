## 什麼是 Helper
---
這個專案的絕大部分控制器都提供 GetInfoById、GetList、ChangeActive、DeleteItem、Submit 五種基本功能。 這些功能又同時有些共通欄位需要處理：例如，資料都有
CreDate、UpdDate、CreUser、UpdUser ...。 或是 GetList 的排序需要提供正序與倒序。

Helper 的職責，就是以「共通功能」為單位，內聚這些「共通處理｣。

但是有一些處理，仍需要客製化或是外部提供。例如資料來源、查詢條件、輸入驗證。 這些客製化的部分，以介面溝通。例如「ChangeActiveHelper」，要求呼叫者實作「IChangeActive」。

## 不是萬用解
---
因為 Helper 的設計問題，彈性並不大，有部分不適用的端點仍使用自訂實作，沒有使用 Helper，這類端點的方法中會盡量留下註解。