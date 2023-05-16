namespace NS_Education.Models.APIItems
{
    public abstract class BaseInfusable : IReturnMessageInfusable
    {
        public bool SuccessFlag { get; private set; }
        public string Message { get; private set; }
        public void Infuse(BaseApiResponse msg)
        {
            SuccessFlag = msg.SuccessFlag;
            Message = msg.Message;
        }
    }
}