namespace NS_Education.Models.APIItems
{
    public abstract class BaseGetResponseInfusableWithCreUpd : BaseGetResponseWithCreUpd, IReturnMessageInfusable
    {
        public bool SuccessFlag { get; private set; }
        public string Message { get; private set; }

        public void Infuse(cReturnMessage message)
        {
            SuccessFlag = message.SuccessFlag;
            Message = message.Message;
        }
    }
}