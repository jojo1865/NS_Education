# 需先確保的事情
* 在**本專案**設定 dotnet user-secrets。`ConnectionStrings:NsEduCore` 的值需要是你的 Connection Strings。
  * `dotnet user-secrets set "Name=ConnectionStrings:NsEduCore" "置入你的連線字串"`

# 更新資料庫模型的指令
* 在**本專案**執行以下指令：
```cmd
dotnet ef dbcontext scaffold "Name=ConnectionStrings:NsEduCore" Microsoft.EntityFrameworkCore.SqlServer --use-database-names --no-pluralize -p ..\NsEduCore_DAL\ --context-dir Models\Data --output-dir Models --context NsDataContext --force
```

# 附註：為什麼不在 DAL 專案跑
* 在 DAL 專案跑，似乎因為 DAL 專案沒有 IConfiguration 可以用，所以不支援 Named connection strings。