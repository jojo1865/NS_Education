using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NS_Education.Models.Errors
{
    public abstract class BaseError
    {
        public virtual char ErrorType { get; protected set; }

        public string ErrorCode => ErrorCodeInt.ToString("D3");

        public virtual string ErrorMessage { get; set; }
        public IDictionary<string, object> AdditionalValues { get; } = new Dictionary<string, object>();

        [JsonIgnore] public virtual int ErrorCodeInt { get; protected set; }

        protected void AddAdditionalValues(ErrorField field, object value)
        {
            AdditionalValues[GetValueName(field)] = value;
        }

        private string GetValueName(ErrorField field)
        {
            return Enum.GetName(typeof(ErrorField), field) ?? $"AdditionalValue{AdditionalValues.Count}";
        }

        protected object GetAdditionalValue(ErrorField field)
        {
            string valueName = GetValueName(field);
            return AdditionalValues.ContainsKey(valueName) ? AdditionalValues[valueName].ToString() : null;
        }

        protected string GetAdditionalValueFormatted(ErrorField field)
        {
            object value = GetAdditionalValue(field);
            return value != null ? $"「{GetAdditionalValue(field)}」" : "";
        }
    }
}