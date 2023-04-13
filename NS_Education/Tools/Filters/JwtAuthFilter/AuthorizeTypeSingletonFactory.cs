using System;

namespace NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType
{
    public static class AuthorizeTypeSingletonFactory
    {
        /// <summary>
        /// 取得 AnyRole 的 Singleton。
        /// </summary>
        public static IAuthorizeType Any { get; } = new AnyRole();
        
        /// <summary>
        /// 取得 AdminRole 的 Singleton。
        /// </summary>
        public static IAuthorizeType Admin { get; } = new AdminRole();
        
        /// <summary>
        /// 取得 UserSelf 的 Singleton。
        /// </summary>
        public static IAuthorizeType User { get; } = new UserRole();

        /// <summary>
        /// 依據輸入 AuthorizeBy 回傳對應的 Singleton。
        /// </summary>
        /// <param name="type">AuthorizeBy 種類</param>
        /// <returns>對應的 IAuthorizeType 的 Singleton。</returns>
        /// <exception cref="ArgumentOutOfRangeException">當 enum 值未列入此處邏輯時</exception>
        public static IAuthorizeType GetByType(AuthorizeBy type)
        {
            switch (type)
            {
                case AuthorizeBy.Admin:
                    return Admin;
                case AuthorizeBy.UserSelf:
                    return User;
                case AuthorizeBy.Any:
                    return Any;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

        }
    }
}