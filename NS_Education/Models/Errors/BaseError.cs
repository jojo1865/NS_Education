using Newtonsoft.Json;

namespace NS_Education.Models.Errors
{
    public abstract class BaseError
    {
        public virtual char ErrorType { get; protected set; }

        public string ErrorCode => ErrorCodeInt.ToString("D3");

        public virtual string ErrorMessage { get; set; }

        [JsonIgnore] public virtual int ErrorCodeInt { get; protected set; }
    }
}