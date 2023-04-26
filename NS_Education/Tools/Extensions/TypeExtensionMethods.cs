using System;

namespace NS_Education.Tools.Extensions
{
    public static class TypeExtensionMethods
    {
        /// <summary>
        /// 檢查 type 中是否有名稱與 propertyName 相同的欄位。
        /// </summary>
        /// <param name="type">要檢查的類型。</param>
        /// <param name="propertyName">要檢查的欄位名稱。</param>
        /// <returns>
        /// true: 欄位存在。<br/>
        /// false: 欄位不存在。
        /// </returns>
        public static bool HasProperty(this Type type, string propertyName) => !(type.GetProperty(propertyName) is null);
    }
}