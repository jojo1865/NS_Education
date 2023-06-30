using System;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors
{
    public sealed class SystemError : BaseError
    {
        public SystemError(Exception exception)
        {
            Exception = exception.GetActualMessage();
            ExceptionStack = exception.GetMeaningfulStackTrace();
        }

        public SystemError(string exceptionMessage)
        {
            Exception = exceptionMessage;
        }

        public string Exception { get; }
        public string ExceptionStack { get; }

        public override char ErrorType => 'S';
        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => "系統異常，請連絡系統管理員處理！";
    }
}