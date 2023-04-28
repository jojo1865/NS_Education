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
        /// 儲存修改之前，自動設置 CreUid 等欄位，並寫入 UserLog。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="uid">（可選）要求者的 UID。</param>
        public static void SaveChangesStandardProcedure(this NsDbContext context, int? uid = null)
        {
            DoStandardProcedure(context, uid ?? GetUid());

            context.SaveChanges();
        }
        
        /// <summary>
        /// 異步儲存修改之前，自動設置 CreUid 等欄位，並寫入 UserLog。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="uid">（可選）要求者的 UID。</param>
        public static async Task SaveChangesStandardProcedureAsync(this NsDbContext context, int? uid = null)
        {
            DoStandardProcedure(context, uid ?? GetUid());

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
            context.WriteUserLog(type, uid ?? GetUid());

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
            context.WriteUserLog(type, uid ?? GetUid());

            await context.SaveChangesAsync();
        }

        private static void DoStandardProcedure(NsDbContext context, int uid)
        {
            // write log
            context.ChangeTracker.DetectChanges();

            foreach (var change in context.ChangeTracker.Entries().ToArray())
            {
                // 未變動或是在寫 UserLog 時，跳過
                if (change.Entity is UserLog 
                    || change.State == EntityState.Unchanged)
                    return;

                // 只有登入日期改變時，不做任何事（這是登入紀錄）
                if (change.Entity is UserData 
                    && change.State == EntityState.Modified
                    && change.Properties.All(p => !p.IsModified || p.Metadata.Name == nameof(UserData.LoginDate)))
                    return;

                SetBasicFields(uid, change);
                WriteUserLog(context, uid, change);
            }
            
            // 把前述的 changes 一同納入下次存檔
            context.ChangeTracker.DetectChanges();
        }

        private static void SetBasicFields(int uid, EntityEntry change)
        {
            switch (change.State)
            {
                case EntityState.Added:
                    change.SetIfEntityHasProperty(DbConstants.DeleteFlag, false);
                    change.SetIfEntityHasProperty(DbConstants.CreDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.CreUid, uid);
                    change.SetIfEntityHasProperty(DbConstants.UpdDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.UpdUid, 0);
                    break;
                case EntityState.Deleted:
                    change.SetIfEntityHasProperty(DbConstants.DeleteFlag, true);
                    change.SetIfEntityHasProperty(DbConstants.UpdDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.UpdUid, uid);
                    break;
                case EntityState.Modified:
                    change.SetIfEntityHasProperty(DbConstants.UpdDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.UpdUid, uid);
                    break;
            }
        }

        private static void SetIfEntityHasProperty(this EntityEntry change, string propertyName, object newValue)
        {
            PropertyEntry propertyEntry = change.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);

            propertyEntry?.Metadata.PropertyInfo.SetValue(change.Entity, newValue);
        }

        private static void WriteUserLog(NsDbContext context, int uid, EntityEntry change)
        {
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

        private static int GetTargetIdFromEntity<T>(this NsDbContext context, T entity)
        {
            // 從 Entity 找出 PK 並找出手上物件的該欄位值，如果有任何 null 時，回傳 0
            IEntityType entityType = context.Model.FindEntityType(nameof(T));
            int.TryParse(entityType?.FindPrimaryKey()?.Properties?.FirstOrDefault()?.PropertyInfo?.GetValue(entity)?.ToString(),
                out int result);
            return result;
        }

        private static void WriteUserLog(this NsDbContext context, string targetTable, int targetId, UserLogControlType controlType, int uid)
        {
            HttpRequestBase request = GetCurrentRequest();
            
            context.UserLog.Add(new UserLog
            {
                UID = uid,
                TargetTable = targetTable ?? "",
                TargetID = targetId,
                ControlType = (int)controlType,
                CreDate = DateTime.Now,
                RequestUrl = request.Url?.PathAndQuery ?? ""
            });
        }

        private static HttpRequestWrapper GetCurrentRequest()
        {
            return new HttpRequestWrapper(HttpContext.Current.Request);
        }

        private static int GetUid(HttpRequestBase request = null)
        { 
            return FilterStaticTools.GetUidInRequestInt(request ?? GetCurrentRequest());
        }

        private static void WriteUserLog(this NsDbContext context, UserLogControlType controlType, int uid)
        {
            // 未指定 targetTable 跟 targetId 時的 helper
            context.WriteUserLog(null, 0, controlType, uid);
        }
    }
}