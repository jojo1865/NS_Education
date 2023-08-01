using System;
using System.Collections.Generic;
using System.Reflection;
using BeingValidated;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.Extensions
{
    public static class ListSortingExtension
    {
        public static void ValidateSorts<TEntity>(this IEnumerable<ListSorting> sorts) where TEntity : class
        {
            ICollection<string> errors = new List<string>();

            // 這邊 flags 的目標是忽略大小寫
            // 但單純只放 IgnoreCase 會覆寫掉預設的 flags, 所以手動把預設的 flags 也加進來以便順利找到 property

            bool isValid = sorts.StartValidateElements()
                .Validate(
                    sort => sort.PropertyName.HasContent() && typeof(TEntity).GetProperty(sort.PropertyName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null,
                    sort => errors.Add($"套用排序時，查無此欄位：{sort.PropertyName}"))
                .IsValid();

            if (!isValid)
                throw new Exception(String.Join(",\n", errors));
        }
    }
}