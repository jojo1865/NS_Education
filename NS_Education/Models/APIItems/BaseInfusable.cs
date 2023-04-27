namespace NS_Education.Models.APIItems
{
    public abstract class BaseInfusable : IReturnMessageInfusable
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public void Infuse(cReturnMessage msg)
        {
            Success = msg.Success;
            Message = msg.Message;
        }
    }
}