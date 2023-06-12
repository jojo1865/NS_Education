namespace NS_Education.Models.Errors.AuthorizationErrors
{
    public abstract class BaseAuthorizationError : BaseError
    {
        public sealed override char ErrorType => 'A';
    }
}