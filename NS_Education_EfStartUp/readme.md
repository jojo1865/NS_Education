# 這是什麼？
---

* 這是給 EF scaffold 使用的空專案。
* 因為 EF 雖然支援 .NET Framework，但它的工具只能在 Core 專案下才能啟動，所以需要一個 dummy 專案給它啟用。

# Scaffold 指令
* 在本專案下執行
---
```sh
dotnet ef dbcontext scaffold "Data Source=LAPTOP-RUU8RF7Q\NS_EDUCATION;Initial Catalog=ns;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer --use-database-names --context-dir ..\NS_Education\Models\Entities\DbContext --output-dir ..\NS_Education\Models\Entities --context NsDbContext
```         