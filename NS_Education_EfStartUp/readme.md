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

# 跑完後所須處理
* 將所有物件的 namespace 調正確
* 將 DbContext 的 OnConfiguring 改為以下內容
```c#
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;
            
            var connectionStrings = System.Web.Configuration.WebConfigurationManager.ConnectionStrings;
            string connectionString =
                Environment.ExpandEnvironmentVariables(connectionStrings["db_NS_EducationConnectionStringEnv"].ConnectionString);

            try
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            catch
            {
                connectionString = connectionStrings["db_NS_EducationConnectionString"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        } 
```