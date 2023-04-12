namespace NS_Education.Tools.Extensions
{
    public static class StandardColumnExtensionMethods
    {
        /// <summary>
        /// 驗證 UID 是否正確。
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>true：UID 正確。<br/>
        /// false：UID 錯誤。
        /// </returns>
        public static bool IsIncorrectUid(this int uid)
        {
            // TODO: 加上驗證資料庫？
            return uid <= 0;
        }
    }
}