using System;

namespace NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType
{
    [Flags]
    public enum RequirePrivilege
    {
        None = 1,
        ShowFlag = 2,
        AddFlag = 4,
        EditFlag = 8,
        DeleteFlag = 16,
        PrintFlag = 32,
        AddOrEdit = 64
    }
}