using System;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors
{
    public sealed class SystemError : BaseError
    {
        public SystemError(Exception exception)
        {
            AddAdditionalValues(ErrorField.Exception, exception.GetActualMessage());
            AddAdditionalValues(ErrorField.ExceptionStack, exception.StackTrace);
        }

        public SystemError(string exceptionMessage)
        {
            AddAdditionalValues(ErrorField.Exception, exceptionMessage);
        }

        public override char ErrorType => 'S';
        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => "系統異常";
    }
}