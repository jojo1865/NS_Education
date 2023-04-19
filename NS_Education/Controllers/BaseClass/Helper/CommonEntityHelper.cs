using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NS_Education.Variables;

namespace NS_Education.Controllers.BaseClass.Helper
{
    internal static class CommonEntityHelper<TEntity>
        where TEntity : class
    {
        internal static bool HasActiveFlag { get; } = HasProperty(typeof(TEntity), DbConstants.ActiveFlag);
        internal static bool HasDeleteFlag { get; } = HasProperty(typeof(TEntity), DbConstants.DeleteFlag);

        internal static bool HasCreUid { get; } = HasProperty(typeof(TEntity), DbConstants.CreUid);
        internal static bool HasCreDate { get; } = HasProperty(typeof(TEntity), DbConstants.CreDate);
        internal static bool HasUpdUid { get; } = HasProperty(typeof(TEntity), DbConstants.UpdUid);
        internal static bool HasUpdDate { get; } = HasProperty(typeof(TEntity), DbConstants.UpdDate);

        internal static void SetProperty<T>(T t, string propertyName, object value) =>
            GetProperty<T>(propertyName).SetValue(t, value);
        
        
        internal static IQueryable<TEntity> FilterDeletedIfHasFlag(IQueryable<TEntity> query)
        {
            if (HasDeleteFlag)
                query = query.Where(entity => EF.Property<bool>(entity, DbConstants.DeleteFlag) == false);
            return query;
        }

        private static PropertyInfo GetProperty<T>(string propertyName) => typeof(T).GetProperty(propertyName);
        private static PropertyInfo GetProperty(Type type, string propertyName) => type.GetProperty(propertyName);
        private static bool HasProperty(Type type, string propertyName) => !(GetProperty(type, propertyName) is null);
    }
}