# 這是什麼？
---

* 這是給 EF scaffold 使用的空專案。
* 因為 EF 雖然支援 .NET Framework，但它的工具只能在 Core 專案下才能啟動，所以需要一個 dummy 專案給它啟用。

# Scaffold 指令
* 在本專案下執行
---
```sh
dotnet ef dbcontext scaffold "Data Source=192.168.50.125\SQL2019D;Database=db_NS_Education;User Id=User_Kevin;Password=User_Kevin;" Microsoft.EntityFrameworkCore.SqlServer --use-database-names --context-dir ..\NS_Education\Models\Entities\DbContext --output-dir ..\NS_Education\Models\Entities --context NsDbContext --force
```

# 跑完後所須處理
* 將所有物件的 namespace 調正確
* 將 DbContext 的 OnConfiguring 改為以下內容
```c#
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;
            
            var connectionStrings = System.Web.Configuration.WebConfigurationManager.ConnectionStrings;

            string env =
                Environment.GetEnvironmentVariable("ConnectionStrings:NsEducationJojo");
            string fallback =
                connectionStrings["db_NS_EducationConnectionStringFallback"].ConnectionString;

            optionsBuilder.UseSqlServer((!env.IsNullOrWhiteSpace() ? env : fallback) ?? throw new NullReferenceException("ConnectionString"));
        } 
```