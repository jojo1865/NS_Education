namespace NS_Education.Models.Errors.InputValidationErrors
{
    public abstract class BaseInputValidationError : BaseError
    {
        public sealed override char ErrorType => 'V';
    }
}