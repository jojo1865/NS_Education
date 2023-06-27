namespace NS_Education.Models.APIItems
{
    public abstract class BaseInfusable : BaseApiResponse, IReturnMessageInfusable
    {
        public void Infuse(BaseApiResponse msg)
        {
            SuccessFlag = msg.SuccessFlag;
            Errors = msg.Errors;
        }
    }
}