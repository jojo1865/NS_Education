using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using NS_Education.Models.Entities;
using NS_Education.Models.Entities.DbContext;
using NS_Education.Tools.Filters;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    /// <summary>
    /// 用以處理 DbContext 相關的擴充方法。
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// 儲存修改的同時，寫入 UserLog。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="uid">（可選）使用者 ID</param>
        public static void SaveChangesWithLog(this NsDbContext context, int? uid = null)
        {
            WriteUserLog(context, uid);

            // call base
            context.SaveChanges();
        }

        /// <summary>
        /// 非同步儲存修改的同時，寫入 UserLog。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="uid">（可選）使用者 ID</param>
        public static async Task SaveChangesWithLogAsync(this NsDbContext context, int? uid = null)
        {
            WriteUserLog(context, uid);
            
            // call base
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 寫一筆 UserLog 並儲存到 DB。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="type">操作類型</param>
        /// <param name="uid">（可選）使用者 ID</param>
        public static void WriteUserLogAndSave(this NsDbContext context, UserLogControlType type, int? uid = null)
        {
            context.WriteUserLog(type, uid);

            context.SaveChanges();
        }
        
        /// <summary>
        /// 異步地寫一筆 UserLog 並儲存到 DB。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="type">操作類型</param>
        /// <param name="uid">（可選）使用者 ID</param>
        public static async Task WriteUserLogAndSaveAsync(this NsDbContext context, UserLogControlType type, int? uid = null)
        {
            context.WriteUserLog(type, uid);

            await context.SaveChangesAsync();
        }

        private static void WriteUserLog(NsDbContext context, int? uid = null)
        {
            // write log
            context.ChangeTracker.DetectChanges();

            foreach (var change in context.ChangeTracker.Entries().ToArray())
            {
                // 未變動或是在寫 UserLog 時，跳過
                if (change.Entity is UserLog || change.State == EntityState.Unchanged)
                    continue;

                // 只有登入日期改變時，不做任何事（這是登入紀錄）
                if (change.Entity is UserData && change.Properties.All(p => !p.IsModified || p.Metadata.Name == nameof(UserData.LoginDate)))
                    continue;

                // 取得此資料的第一個 PK 欄位（通常是流水號）
                int targetId = context.GetTargetIdFromEntity(change.Entity);

                // 依據這筆修改的狀態，指定 ControlType
                UserLogControlType controlType = UserLogControlType.Show;
                switch (change.State)
                {
                    case EntityState.Added:
                        controlType = UserLogControlType.Add;
                        break;
                    case EntityState.Modified:
                        controlType = UserLogControlType.Edit;
                        break;
                    case EntityState.Deleted:
                        controlType = UserLogControlType.Delete;
                        break;
                }
                
                // 如果是 DeleteFlag 改成 true, 視為刪除
                PropertyEntry deleteFlag = change.Properties.FirstOrDefault(p => p.Metadata.Name == DbConstants.DeleteFlag);
                if (deleteFlag != null && deleteFlag.IsModified && deleteFlag.OriginalValue is false)
                    controlType = UserLogControlType.Delete;

                context.WriteUserLog(change.Metadata.GetTableName(), targetId, controlType, uid);
            }
        }
        private static int GetTargetIdFromEntity<T>(this NsDbContext context, T entity)
        {
            // 從 Entity 找出 PK 並找出手上物件的該欄位值，如果有任何 null 時，回傳 0
            IEntityType entityType = context.Model.FindEntityType(nameof(T));
            int.TryParse(entityType?.FindPrimaryKey()?.Properties?.FirstOrDefault()?.PropertyInfo?.GetValue(entity)?.ToString(),
                out int result);
            return result;
        }

        private static void WriteUserLog(this NsDbContext context, string targetTable, int targetId, UserLogControlType controlType, int? uid = null)
        {
            HttpRequestBase request = new HttpRequestWrapper(HttpContext.Current.Request);
            
            // 從 Request header 中的 Authorization 的 JWT Token 找到 UID。
            // 如果找不到，或是 Authorization 格式有問題，都會直接拋錯。
            // 這兩種情況都不應該進到這個步驟，所以此處不特意做 try-catch。
            if (uid is null)
                uid =  FilterStaticTools.GetUidInRequestInt(request);
            
            context.UserLog.Add(new UserLog
            {
                UID = uid.Value,
                TargetTable = targetTable ?? "",
                TargetID = targetId,
                ControlType = (int)controlType,
                CreDate = DateTime.Now,
                RequestUrl = request.Url?.PathAndQuery ?? ""
            });
        }
        
        private static void WriteUserLog(this NsDbContext context, UserLogControlType controlType, int? uid = null)
        {
            // 未指定 targetTable 跟 targetId 時的 helper
            context.WriteUserLog(null, 0, controlType, uid);
        }
    }
}