using System;
using System.Collections.Generic;
using System.Linq;
using NS_Education.Models.Errors;

namespace NS_Education.Models
{
    public class CommonApiResponse
    {
        public IEnumerable<BaseError> Errors = Array.Empty<BaseError>();
        public bool SuccessFlag = true;

        public IEnumerable<string> Messages =>
            Errors.Select(error => $"{error.ErrorType}-{error.ErrorCode} {error.ErrorMessage}");
    }
}