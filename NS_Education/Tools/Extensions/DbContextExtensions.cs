using System;
using System.Collections.Generic;
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
    public static class DbContextExtensions
    {
        public static void SaveChangesWithLog(this NsDbContext context, int? uid = null)
        {
            WriteUserLog(context, uid);

            // call base
            context.SaveChanges();
        }

        public static async Task SaveChangesWithLogAsync(this NsDbContext context, int? uid = null)
        {
            WriteUserLog(context, uid);
            
            // call base
            await context.SaveChangesAsync();
        }
        
        public static void WriteUserLog<T>(this NsDbContext context, int targetId, UserLogControlType type, int? uid = null)
        {
            context.WriteUserLog(context.GetTableNameFromType<T>(), targetId, type, uid);
        }
        
        public static async Task WriteUserLogsAndSaveAsync<T>(this NsDbContext context, IEnumerable<T> entities, UserLogControlType type, int? uid = null)
        {
            foreach (T entity in entities)
            {
                context.WriteUserLog<T>(context.GetTargetIdFromEntity(entity), type, uid);
            }

            await context.SaveChangesAsync();
        }

        public static void WriteUserLogAndSave(this NsDbContext context, UserLogControlType type, int? uid = null)
        {
            context.WriteUserLog(type, uid);

            context.SaveChanges();
        }

        private static string GetTableNameFromType<T>(this NsDbContext context)
        {
            return context.Model.FindEntityType(nameof(T))?.GetTableName();
        }

        private static void WriteUserLog(NsDbContext context, int? uid = null)
        {
            // write log
            
            context.ChangeTracker.DetectChanges();

            foreach (var change in context.ChangeTracker.Entries())
            {
                if (change.Entity is UserLog || change.State == EntityState.Unchanged)
                    continue;

                // 只有登入日期改變時，不做任何事（這是登入紀錄）
                if (change.Entity is UserData && change.Properties.All(p => !p.IsModified || p.Metadata.Name == nameof(UserData.LoginDate)))
                    continue;

                int targetId = context.GetTargetIdFromEntity(change.Entity);

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
            IEntityType entityType = context.Model.FindEntityType(nameof(T));
            int.TryParse(entityType?.FindPrimaryKey()?.Properties?.FirstOrDefault()?.PropertyInfo?.GetValue(entity)?.ToString(),
                out int result);
            return result;
        }

        private static void WriteUserLog(this NsDbContext context, string targetTable, int targetId, UserLogControlType controlType, int? uid = null)
        {
            HttpRequestBase request = new HttpRequestWrapper(HttpContext.Current.Request);
            
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
            context.WriteUserLog(null, 0, controlType, uid);
        }
    }
}