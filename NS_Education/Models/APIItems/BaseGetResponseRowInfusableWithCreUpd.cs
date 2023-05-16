using System.Collections.Generic;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseGetResponseRowInfusableWithCreUpd : BaseGetResponseRowWithCreUpd, IReturnMessageInfusable
    {
        public bool SuccessFlag { get; private set; }
        public IEnumerable<string> Messages { get; private set; } = new List<string>();

        public void Infuse(BaseApiResponse message)
        {
            SuccessFlag = message.SuccessFlag;
            Messages = message.Messages;
        }
    }
}