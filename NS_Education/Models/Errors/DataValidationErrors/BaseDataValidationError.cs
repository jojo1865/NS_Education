namespace NS_Education.Models.Errors.DataValidationErrors
{
    public abstract class BaseDataValidationError : BaseError
    {
        public sealed override char ErrorType => 'D';
    }
}