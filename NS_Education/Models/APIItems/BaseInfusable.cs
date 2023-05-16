using System.Collections.Generic;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseInfusable : IReturnMessageInfusable
    {
        public bool SuccessFlag { get; private set; }
        public IEnumerable<string> Messages { get; private set; } = new List<string>();
        public void Infuse(BaseApiResponse msg)
        {
            SuccessFlag = msg.SuccessFlag;
            Messages = msg.Messages;
        }
    }
}