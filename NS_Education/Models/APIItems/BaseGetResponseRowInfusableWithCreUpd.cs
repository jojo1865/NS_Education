using System.Collections.Generic;
using NS_Education.Models.Errors;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseGetResponseRowInfusableWithCreUpd : BaseGetResponseRowWithCreUpd, IReturnMessageInfusable
    {
        public bool SuccessFlag { get; private set; }
        public IEnumerable<BaseError> Messages { get; private set; } = new List<BaseError>();

        public void Infuse(CommonApiResponse message)
        {
            SuccessFlag = message.SuccessFlag;
            Messages = message.Errors;
        }
    }
}