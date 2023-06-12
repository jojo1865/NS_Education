using System.Collections.Generic;

namespace NS_Education.Models.Errors
{
    public class BusinessError : BaseError
    {
        public BusinessError(int code, string message, params KeyValuePair<ErrorField, object>[] additionalValues)
        {
            ErrorCodeInt = code;
            ErrorMessage = message;

            foreach (KeyValuePair<ErrorField, object> kvp in additionalValues)
            {
                if (kvp.Value != null)
                    AddAdditionalValues(kvp.Key, kvp.Value);
            }
        }

        public override char ErrorType => 'B';
    }
}