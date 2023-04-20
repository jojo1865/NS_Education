namespace NS_Education.Tools
{
    public static class ObjectExtensionMethods
    {
        /// <summary>
        /// 將 <see cref="T"/> 中名稱與 propertyName 相同的欄位的值設為 value。<br/>
        /// 找不到該欄位時，不做任何事。
        /// </summary>
        /// <param name="t">Generic Type</param>
        /// <param name="propertyName">欄位名稱</param>
        /// <param name="value">新值</param>
        /// <typeparam name="T">Generic Type</typeparam>
        internal static void SetIfHasProperty<T>(this T t, string propertyName, object value) =>
            typeof(T).GetProperty(propertyName)?.SetValue(t, value);
    }
}