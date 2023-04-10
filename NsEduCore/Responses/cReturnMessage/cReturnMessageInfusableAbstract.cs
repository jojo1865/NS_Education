namespace NsEduCore.Responses.cReturnMessage
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