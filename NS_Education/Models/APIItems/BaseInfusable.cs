namespace NS_Education.Models.APIItems
{
    public abstract class BaseInfusable : CommonApiResponse, IReturnMessageInfusable
    {
        public void Infuse(CommonApiResponse msg)
        {
            SuccessFlag = msg.SuccessFlag;
            Errors = msg.Errors;
        }
    }
}