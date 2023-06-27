using System.Collections.Generic;

namespace NS_Education.Models.Errors
{
    public sealed class BusinessError : BaseError
    {
        public BusinessError(int code, string message, IDictionary<string, object> additionalValues = null)
        {
            AdditionalValues = additionalValues;
            ErrorCodeInt = code;
            ErrorMessage = message;
        }

        public IDictionary<string, object> AdditionalValues { get; }

        public override char ErrorType => 'B';
    }
}