using Newtonsoft.Json;

namespace NS_Education.Models.Errors
{
    public abstract class BaseError
    {
        [JsonIgnore] public virtual char ErrorType { get; protected set; }

        public virtual string ErrorMessage { get; set; }

        [JsonIgnore] public virtual int ErrorCodeInt { get; protected set; }
        [JsonIgnore] public string ErrorCodePadded => ErrorCodeInt.ToString("D3");

        public string ErrorCode => $"{ErrorType}-{ErrorCodePadded}";
    }
}