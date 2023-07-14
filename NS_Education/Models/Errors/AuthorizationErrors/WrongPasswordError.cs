namespace NS_Education.Models.Errors.AuthorizationErrors
{
    public class WrongPasswordError : BaseAuthorizationError
    {
        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => "密碼錯誤！";
    }
}