using System.Web.Mvc;
using NS_Education.Tools.Filters.ExceptionHandlerFilter;

namespace NS_Education
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ExceptionHandlerFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
