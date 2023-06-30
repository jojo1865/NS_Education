using System;
using System.Web.Mvc;
using NS_Education.Models.Entities;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools.Filters.ExceptionHandlerFilter
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            try
            {
                WriteToDb(filterContext);
            }
            catch
            {
                // ignored
                // Because there can be exception about connection with DB.
            }

            filterContext.Result = ResponseHelper.CreateWrappedResponse(filterContext, filterContext.Result);

            Console.WriteLine(filterContext.Exception.Message);
            Console.WriteLine(filterContext.Exception.StackTrace);

            filterContext.ExceptionHandled = true;
        }

        private static void WriteToDb(ExceptionContext filterContext)
        {
            using (NsDbContext context = new NsDbContext())
            {
                // here we don't use project-customized SaveChanges method,
                // to avoid possibilities where that fails too.

                context.ErrorLog.Add(new ErrorLog
                {
                    JWT = FilterStaticTools.GetJwtToken(filterContext.HttpContext.Request),
                    RequestUrl = filterContext.HttpContext.Request.RawUrl,
                    Payload = Uri.UnescapeDataString(filterContext.HttpContext.Request.Form.ToString()),
                    ExceptionMessage = filterContext.Exception.GetActualMessage(),
                    ExceptionTrace = filterContext.Exception.GetMeaningfulStackTrace(),
                    CreDate = DateTime.Now
                });

                context.SaveChanges();
            }
        }
    }
}