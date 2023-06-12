using System;
using System.Collections.Generic;

namespace NS_Education.Tools.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetActualMessage(this Exception exception)
        {
            string result = exception.Message;
            if (exception.InnerException == null) return result;

            // 取得最裡面的 InnerException，但避免無限迴圈
            HashSet<Exception> exceptions = new HashSet<Exception>();
            Exception curr = exception;

            while (exceptions.Add(curr) && curr.InnerException != null)
            {
                curr = curr.InnerException;
            }

            result = curr.Message;

            return result;
        }
    }
}