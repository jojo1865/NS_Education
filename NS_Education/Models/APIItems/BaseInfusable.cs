using System.Collections.Generic;
using NS_Education.Models.Errors;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseInfusable : IReturnMessageInfusable
    {
        public bool SuccessFlag { get; private set; }
        public IEnumerable<BaseError> Messages { get; private set; } = new List<BaseError>();

        public void Infuse(BaseApiResponse msg)
        {
            SuccessFlag = msg.SuccessFlag;
            Messages = msg.Messages;
        }
    }
}