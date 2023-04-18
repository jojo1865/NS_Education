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
        public bool RequireAddOrEditFlag { get; }

        public RequiredPrivileges(RequirePrivilege aggregate)
        {
            if (aggregate == RequirePrivilege.None)
            {
                None = true;
                return;
            }
            
            RequireShowFlag = aggregate.HasFlag(RequirePrivilege.ShowFlag);
            RequireAddFlag = aggregate.HasFlag(RequirePrivilege.AddFlag);
            RequireEditFlag = aggregate.HasFlag(RequirePrivilege.EditFlag);
            RequireDeleteFlag = aggregate.HasFlag(RequirePrivilege.DeleteFlag);
            RequirePrintFlag = aggregate.HasFlag(RequirePrivilege.PrintFlag);
            RequireAddOrEditFlag = aggregate.HasFlag(RequirePrivilege.AddOrEdit);
        }
    }
}