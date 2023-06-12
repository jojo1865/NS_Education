using System;
using System.Collections.Generic;
using NS_Education.Models.Errors;

namespace NS_Education.Models
{
    public class BaseApiResponse
    {
        public IEnumerable<BaseError> Messages = Array.Empty<BaseError>();
        public bool SuccessFlag = true;
    }
}