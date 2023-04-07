using NS_Education.Models;

namespace NS_Education.Tools
{
    public abstract class cReturnMessageInfusableAbstract : cReturnMessage
    {
        public void Infuse(cReturnMessage msg)
        {
            Success = msg.Success;
            Message = msg.Message;
        }
    }
}