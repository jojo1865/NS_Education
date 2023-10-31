using System;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors
{
    public sealed class AuthError : BaseError
    {
        public AuthError(Exception exception)
        {
            Exception = exception.GetActualMessage();
        }

        public AuthError(string exceptionMessage)
        {
            Exception = exceptionMessage;
        }

        public string Exception { get; }

        public override char ErrorType => 'S';
        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => "您的帳號無權限進行此操作，或是登入已過期！請先嘗試重新登入，若問題未解決，請與系統管理員聯絡。";
    }
}