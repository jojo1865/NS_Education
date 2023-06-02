using System.Web.Mvc;

namespace NS_Education.Tools.Filters.ExceptionHandlerFilter
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = ResponseHelper.CreateWrappedResponse(filterContext, filterContext.Result);
            filterContext.ExceptionHandled = true;
        }
    }
}