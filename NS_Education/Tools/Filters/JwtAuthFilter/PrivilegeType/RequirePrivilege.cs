using System;

namespace NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType
{
    [Flags]
    public enum RequirePrivilege
    {
        ShowFlag = 0,
        AddFlag = 1,
        EditFlag = 2,
        DeleteFlag = 3,
        PrintFlag = 4,
        AuthenticateFlag = 5,
        None = -1
    }
}