using System;
using System.Collections.Generic;

namespace NS_Education.Models
{
    public class BaseApiResponse
    {
        public bool SuccessFlag = true;
        public IEnumerable<string> Messages = Array.Empty<string>();
    }
}