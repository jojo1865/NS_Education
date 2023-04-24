using System.Linq;
using Microsoft.EntityFrameworkCore;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common
{
    /// <summary>
    /// 針對判定資料庫物件中是否有某些普遍型欄位，進行預設處理的工具。
    /// </summary>
    /// <typeparam name="TEntity">資料物件</typeparam>
    internal static class FlagHelper<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 回傳 <see cref="TEntity"/> 是否有名為 ActiveFlag 的欄位。
        /// </summary>
        internal static bool HasActiveFlag { get; } = typeof(TEntity).HasProperty(DbConstants.ActiveFlag);
        
        /// <summary>
        /// 回傳 <see cref="TEntity"/> 是否有名為 DeleteFlag 的欄位。
        /// </summary>
        internal static bool HasDeleteFlag { get; } = typeof(TEntity).HasProperty(DbConstants.DeleteFlag);

        /// <summary>
        /// 回傳 <see cref="TEntity"/> 是否有名為 CreUid 的欄位。
        /// </summary>
        internal static bool HasCreUid { get; } = typeof(TEntity).HasProperty(DbConstants.CreUid);
        
        /// <summary>
        /// 回傳 <see cref="TEntity"/> 是否有名為 CreDate 的欄位。
        /// </summary>
        internal static bool HasCreDate { get; } = typeof(TEntity).HasProperty(DbConstants.CreDate);
        
        /// <summary>
        /// 回傳 <see cref="TEntity"/> 是否有名為 UpdUid 的欄位。
        /// </summary>
        internal static bool HasUpdUid { get; } = typeof(TEntity).HasProperty(DbConstants.UpdUid);
        
        /// <summary>
        /// 回傳 <see cref="TEntity"/> 是否有名為 UpdDate 的欄位。
        /// </summary>
        internal static bool HasUpdDate { get; } = typeof(TEntity).HasProperty(DbConstants.UpdDate);
    }

    /// <summary>
    /// 針對為物件設值，或是以欄位存在為條件的某些普遍型處理，進行預設處理的工具。
    /// </summary>
    internal static class FlagHelper
    {
        /// <summary>
        /// 為查詢加上篩選掉 DeleteFlag = true 的資料。<br/>
        /// 如果查詢的對象物件沒有此欄位，不做任何事。
        /// </summary>
        /// <param name="query">查詢</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>
        /// 物件有 DeleteFlag 時：加上 DeleteFlag == false 的條件之後的查詢。<br/>
        /// 其它情況：原本的查詢。
        /// </returns>
        internal static IQueryable<T> FilterDeletedIfHasFlag<T>(IQueryable<T> query)
            where T : class
        {
            if (FlagHelper<T>.HasDeleteFlag)
                query = query.Where(entity => EF.Property<bool>(entity, DbConstants.DeleteFlag) == false);
            return query;
        }

        /// <summary>
        /// 依據輸入的 ActiveFlag 值對 Query 新增篩選條件。<br/>
        /// 如果物件沒有 ActiveFlag 欄位，不做任何事。
        /// </summary>
        /// <param name="query">查詢</param>
        /// <param name="activeFlag">目標啟用狀態</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>
        /// 物件有 ActiveFlag 時：加上 ActiveFlag == activeFlag 的條件之後的查詢。<br/>
        /// 其它情況：原本的查詢。
        /// </returns>
        internal static IQueryable<T> FilterByInputActiveFlag<T>(IQueryable<T> query, bool activeFlag)
            where T : class
        {
            if (FlagHelper<T>.HasActiveFlag)
                query = query.Where(entity => EF.Property<bool>(entity, DbConstants.ActiveFlag) == activeFlag);

            return query;
        }
        
        /// <summary>
        /// 依據輸入的 DeleteFlag 值對 Query 新增篩選條件。<br/>
        /// 如果物件沒有 DeleteFlag 欄位，不做任何事。
        /// </summary>
        /// <param name="query">查詢</param>
        /// <param name="deleteFlag">目標刪除狀態</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>
        /// 物件有 DeleteFlag 時：加上 DeleteFlag == deleteFlag 的條件之後的查詢。<br/>
        /// 其它情況：原本的查詢。
        /// </returns>
        internal static IQueryable<T> FilterByInputDeleteFlag<T>(IQueryable<T> query, bool deleteFlag)
            where T : class
        {
            if (FlagHelper<T>.HasDeleteFlag)
                query = query.Where(entity => EF.Property<bool>(entity, DbConstants.DeleteFlag) == deleteFlag);

            return query;
        }

        /// <summary>
        /// 若物件有名稱為 ActiveFlag 的欄位，將該值設為 newValue。
        /// </summary>
        /// <param name="entity">欲設定的物件</param>
        /// <param name="newValue">欲設定的新值</param>
        /// <typeparam name="T">Generic Type</typeparam>
        internal static void SetActiveFlag<T>(T entity, bool newValue) 
            where T : class
        {
            entity.SetIfHasProperty(DbConstants.ActiveFlag, newValue);
        }

        /// <summary>
        /// 若物件有名稱為 DeleteFlag 的欄位，將該值設為 newValue。
        /// </summary>
        /// <param name="entity">欲設定的物件</param>
        /// <param name="newValue">欲設定的新值</param>
        /// <typeparam name="TEntity">Generic Type</typeparam>
        internal static void SetDeleteFlag<TEntity>(TEntity entity, bool newValue) where TEntity : class
        {
            entity.SetIfHasProperty(DbConstants.DeleteFlag, newValue);
        }
    }
}