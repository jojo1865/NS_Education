using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Tools.Extensions
{
    /// <summary>
    /// 針對 Exception 類型提供的一系列擴充方法。
    /// </summary>
    public static class ExceptionExtensionMethods
    {
        /// <summary>
        /// 從 Exception 中取得最裡面一層的錯誤訊息。
        /// </summary>
        /// <param name="e">錯誤</param>
        /// <returns>錯誤訊息</returns>
        public static string GetActualMessage(this Exception e)
        {
            if (e.InnerException == null)
                return e.Message;

            HashSet<Exception> traversed = new HashSet<Exception>();
            Exception current = e;

            while (traversed.Add(current) && current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }

        public static string GetMeaningfulStackTrace(this Exception e)
        {
            string stackTrace = e.StackTrace;
            // 以專案起始點的 namespace 為基準
            string nameSpace = typeof(WebApiApplication).Namespace ?? "";

            string[] traces = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return String.Join(Environment.NewLine, traces.Where(t => t.Contains(nameSpace)));
        }
    }
}