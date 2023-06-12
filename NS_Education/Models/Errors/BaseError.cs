using System.Collections.Generic;
using Newtonsoft.Json;

namespace NS_Education.Models.Errors
{
    public abstract class BaseError
    {
        public virtual char ErrorType { get; }

        public string ErrorCode => ErrorCodeInt.ToString("D3");

        public virtual string ErrorMessage { get; set; }
        public IDictionary<string, object> AdditionalValues { get; } = new Dictionary<string, object>();

        [JsonIgnore] public virtual int ErrorCodeInt { get; }

        protected void AddAdditionalValues(ErrorField field, object value)
        {
            AdditionalValues[nameof(field)] = value;
        }

        protected object GetAdditionalValue(ErrorField field)
        {
            return AdditionalValues.ContainsKey(nameof(field)) ? AdditionalValues[nameof(field)].ToString() : null;
        }

        protected string GetAdditionalValueFormatted(ErrorField field)
        {
            object value = GetAdditionalValue(field);
            return value != null ? $"「{GetAdditionalValue(field)}」" : "";
        }
    }
}