namespace NS_Education.Models.Errors.AuthorizationErrors
{
    public sealed class InsufficientPrivilegeError : BaseAuthorizationError
    {
        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => "權限不足！";
    }
}