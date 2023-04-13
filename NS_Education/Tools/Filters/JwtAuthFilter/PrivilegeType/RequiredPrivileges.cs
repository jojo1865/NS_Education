using System;
using Microsoft.Ajax.Utilities;

namespace NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType
{
    public class RequiredPrivileges
    {
        public bool None { get; }
        public bool RequireShowFlag { get; }
        public bool RequireAddFlag { get; }
        public bool RequireEditFlag { get; }
        public bool RequireDeleteFlag { get; }
        public bool RequirePrintFlag { get; }

        public RequiredPrivileges(RequirePrivilege aggregate)
        {
            if (aggregate == RequirePrivilege.None)
            {
                None = true;
                return;
            }
            
            RequireShowFlag = aggregate.HasFlag(RequirePrivilege.ShowFlag);
            RequireAddFlag = (aggregate & RequirePrivilege.AddFlag) != 0;
            RequireEditFlag = (aggregate & RequirePrivilege.EditFlag) != 0;
            RequireDeleteFlag = (aggregate & RequirePrivilege.DeleteFlag) != 0;
            RequirePrintFlag = (aggregate & RequirePrivilege.PrintFlag) != 0;
        }
    }
}