namespace NS_Education.Models.APIItems
{
    public abstract class BaseResponseWithCreUpdInfusable<TEntity> : BaseResponseWithCreUpd<TEntity>, IReturnMessageInfusable
      where TEntity : class
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public void Infuse(cReturnMessage message)
        {
            Success = message.Success;
            Message = message.Message;
        }
    }
}