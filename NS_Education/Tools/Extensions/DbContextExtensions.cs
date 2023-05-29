using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NS_Education.Models.Entities;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    /// <summary>
    /// 用以處理 DbContext 相關的擴充方法。
    /// </summary>
    public static class DbContextExtensions
    {
        private static readonly DateTime MinimumDbDateTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

        /// <summary>
        /// 儲存修改之前，自動設置 CreUid 等欄位，並寫入 UserLog。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="uid">要求者的 UID。</param>
        /// <param name="httpRequestBase"></param>
        public static void SaveChangesStandardProcedure(this NsDbContext context, int uid,
            HttpRequestBase httpRequestBase)
        {
            DoStandardProcedure(context, uid, httpRequestBase);

            context.SaveChanges();
        }

        /// <summary>
        /// 異步儲存修改之前，自動設置 CreUid 等欄位，並寫入 UserLog。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="uid">要求者的 UID。</param>
        /// <param name="httpRequest"></param>
        public static async Task SaveChangesStandardProcedureAsync(this NsDbContext context, int uid,
            HttpRequest httpRequest)
        {
            await SaveChangesStandardProcedureAsync(context, uid, new HttpRequestWrapper(httpRequest));
        }

        public static async Task SaveChangesStandardProcedureAsync(this NsDbContext context, int uid,
            HttpRequestBase httpRequestBase)
        {
            DoStandardProcedure(context, uid, httpRequestBase);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 寫一筆 UserLog 並儲存到 DB。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="type">操作類型</param>
        /// <param name="uid">使用者 ID</param>
        /// <param name="httpRequest"></param>
        public static void WriteUserLogAndSave(this NsDbContext context, UserLogControlType type, int uid,
            HttpRequest httpRequest)
        {
            context.WriteUserLog(type, uid, new HttpRequestWrapper(httpRequest));

            context.SaveChanges();
        }

        /// <summary>
        /// 異步地寫一筆 UserLog 並儲存到 DB。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="type">操作類型</param>
        /// <param name="uid">使用者 ID</param>
        /// <param name="httpRequest"></param>
        public static async Task WriteUserLogAndSaveAsync(this NsDbContext context, UserLogControlType type, int uid,
            HttpRequest httpRequest)
        {
            await context.WriteUserLogAndSaveAsync(type, uid, new HttpRequestWrapper(httpRequest));
        }

        public static async Task WriteUserLogAndSaveAsync(this NsDbContext context, UserLogControlType type, int uid,
            HttpRequestBase httpRequestBase)
        {
            context.WriteUserLog(type, uid, httpRequestBase);

            await context.SaveChangesAsync();
        }

        private static void DoStandardProcedure(NsDbContext context, int uid, HttpRequestBase httpRequestBase)
        {
            // write log
            context.ChangeTracker.DetectChanges();

            foreach (var change in context.ChangeTracker.Entries().ToArray())
            {
                // 未變動或是在寫 UserLog 時，跳過
                if (change.Entity is UserLog
                    || change.State == EntityState.Unchanged)
                    return;

                SetBasicFields(uid, change);
                WriteUserLog(context, uid, change, httpRequestBase);
                SanitizeDateColumns(change);
            }

            // 把前述的 changes 一同納入下次存檔
            context.ChangeTracker.DetectChanges();
        }

        private static void SanitizeDateColumns(DbEntityEntry change)
        {
            // 檢查日期欄位，如果太小就設為 SQL Server 可支援的最小值（1753/1/1 12:00:00）
            foreach (var propertyInfo in change.Entity.GetType().GetProperties().Where(p =>
                         p.PropertyType == typeof(DateTime) && (DateTime)p.GetValue(change.Entity) < MinimumDbDateTime))
                propertyInfo.SetValue(change.Entity, MinimumDbDateTime);
        }

        private static void SetBasicFields(int uid, DbEntityEntry change)
        {
            UserLogControlType type = GetUserLogControlType(change);
            switch (type)
            {
                case UserLogControlType.Add:
                    change.SetIfEntityHasProperty(DbConstants.DeleteFlag, false);
                    change.SetIfEntityHasProperty(DbConstants.CreDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.CreUid, uid);
                    change.SetIfEntityHasProperty(DbConstants.UpdDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.UpdUid, 0);
                    break;
                case UserLogControlType.Delete:
                    change.SetIfEntityHasProperty(DbConstants.DeleteFlag, true);
                    change.SetIfEntityHasProperty(DbConstants.UpdDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.UpdUid, uid);
                    break;
                case UserLogControlType.Edit:
                    change.SetIfEntityHasProperty(DbConstants.UpdDate, DateTime.Now);
                    change.SetIfEntityHasProperty(DbConstants.UpdUid, uid);
                    break;
            }
        }

        private static void SetIfEntityHasProperty(this DbEntityEntry change, string propertyName, object newValue)
        {
            change.Entity.SetIfHasProperty(propertyName, newValue);

            if (change.State != EntityState.Unchanged) return;
            change.State = EntityState.Modified;
        }

        private static void WriteUserLog(NsDbContext context, int uid, DbEntityEntry change,
            HttpRequestBase httpRequestBase)
        {
            // 取得此資料的第一個 PK 欄位（通常是流水號）
            int targetId = context.GetPrimaryKeyFromEntityEntry(change);

            // 依據這筆修改的狀態，指定 ControlType
            UserLogControlType controlType = GetUserLogControlType(change);

            context.WriteUserLog(context.GetTableName(change), targetId, controlType, uid, httpRequestBase);
        }

        private static UserLogControlType GetUserLogControlType(DbEntityEntry change)
        {
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
            object deleteFlagString = change.Entity.GetType().GetProperty(DbConstants.DeleteFlag);
            if (bool.TryParse(deleteFlagString?.ToString(), out bool deleteFlag)
                && change.State.HasFlag(EntityState.Modified)
                && change.OriginalValues[DbConstants.DeleteFlag] is false
                && deleteFlag)
                controlType = UserLogControlType.Delete;
            return controlType;
        }

        private static int GetPrimaryKeyFromEntityEntry(this NsDbContext context, DbEntityEntry entityEntry)
        {
            return context.GetPrimaryKeyFromEntity(entityEntry.Entity);
        }

        public static int GetPrimaryKeyFromEntity<TEntity>(this NsDbContext context, TEntity entity)
            where TEntity : class
        {
            // 從 Entity 找出 PK 並找出手上物件的該欄位值，如果有任何 null 時，回傳 0
            // 通常如果是 false，表示 context 中還沒有追蹤這個物件（PK = 0），所以回傳 0
            bool hasStateEntry = ((IObjectContextAdapter)context)
                .ObjectContext
                .ObjectStateManager
                .TryGetObjectStateEntry(entity, out var stateEntry);

            if (!hasStateEntry)
                return 0;

            object result = stateEntry
                .EntityKey
                .EntityKeyValues?
                .Select(kv => kv.Value)
                .FirstOrDefault();

            return result is int i ? i : 0;
        }

        private static void WriteUserLog(this NsDbContext context, string targetTable, int targetId,
            UserLogControlType controlType, int uid, HttpRequestBase httpRequestBase)
        {
            context.UserLog.Add(new UserLog
            {
                UID = uid,
                TargetTable = targetTable ?? "",
                TargetID = targetId,
                ControlType = (int)controlType,
                CreDate = DateTime.Now,
                RequestUrl = httpRequestBase.Url?.PathAndQuery ?? "",
            });
        }

        private static void WriteUserLog(this NsDbContext context, UserLogControlType controlType, int uid,
            HttpRequestBase httpRequestBase)
        {
            // 未指定 targetTable 跟 targetId 時的 helper
            context.WriteUserLog(null, 0, controlType, uid, httpRequestBase);
        }

        /// <summary>
        /// 取得一種物件在 DbContext 中對應的 Table 名。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="entry">物件的 EntityEntry </param>
        /// <returns>Table 名。</returns>
        private static string GetTableName(this NsDbContext context, DbEntityEntry entry)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .Single(e => objectItemCollection.GetClrType(e) == entry.Entity.GetType());

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                .Single()
                .EntitySetMappings
                .Single(s => s.EntitySet == entitySet);

            // Find the storage entity sets (tables) that the entity is mapped
            var tables = mapping
                .EntityTypeMappings.Single()
                .Fragments;

            // Return the table name from the storage entity set
            return tables
                .Select(f => (string)f.StoreEntitySet.MetadataProperties["Table"].Value ?? f.StoreEntitySet.Name)
                .FirstOrDefault();
        }

        /// <summary>
        /// 取得一種物件在 DbContext 中對應的 Table 名。
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>Table 名。</returns>
        public static string GetTableName<T>(this NsDbContext context)
            where T : class
        {
            ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;
            ObjectSet<T> objectSet = objectContext.CreateObjectSet<T>();
            EntityType entityType = objectSet.EntitySet.ElementType;

            string tableName = entityType.MetadataProperties
                .Where(p => p.Name == "Name")
                .Select(p => p.Value.ToString())
                .SingleOrDefault();

            return tableName;
        }

        public static async Task AddAsync<TEntity>(this NsDbContext context, TEntity entity)
            where TEntity : class
        {
            await Task.Run(() => context.Set(entity.GetType()).Add(entity));
        }

        public static async Task AddRangeAsync<TEntity>(this NsDbContext context, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            foreach (var entity in entities)
            {
                // 如果已經是 tracking, 跳過
                if (context.Entry(entity).State != EntityState.Detached)
                    continue;

                await Task.Run(() => context.Set(entity.GetType()).Add(entity));
            }
        }

        public static async Task AddAsync<TEntity>(this DbSet<TEntity> dbSet, TEntity entity)
            where TEntity : class
        {
            await Task.Run(() => dbSet.Add(entity));
        }
    }
}