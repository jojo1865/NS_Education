using System;

namespace NS_Education.Tools.Filters.JwtAuthFilter
{
    [Flags]
    public enum AuthorizeBy
    {
        Admin,
        UserSelf,
        Any
    }
}