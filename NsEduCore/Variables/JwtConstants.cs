namespace NsEduCore.Variables
{
    /// <summary>
    /// JWT 相關的常數。
    /// </summary>
    public static class JwtConstants
    {
        /// <summary>
        /// JWT 密鑰的環境參數索引名稱。
        /// </summary>
        public const string Key = "NsEduCore:JWT:SecretKey";

        /// <summary>
        /// JWT Token 的有效時間（分鐘）。
        /// </summary>
        public const int ExpireMinutes = 30;
    }
}