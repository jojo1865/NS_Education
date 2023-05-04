using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.IdentityModel.Logging;
using NS_Education.Tools.Extensions;

namespace NS_Education
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            #if DEBUG
                // 預設行為會在 Log 不顯示機敏資料 (PII)。
                IdentityModelEventSource.ShowPII = true; 
            #endif
        }

        protected void Application_BeginRequest()
        {
            // 處理 CORS Preflight Request
            string originHeader = "";

            if (Request.HttpMethod != "OPTIONS")
                return;
            
            if (Request.Headers.AllKeys.Contains("Origin"))
            {
                originHeader = Request.Headers["Origin"];
            }
            else if (Request.Headers.AllKeys.Contains("Referer"))
            {
                originHeader = Request.Headers["Referer"];
            }

            if (!string.IsNullOrWhiteSpace(originHeader))
            {
                Response.Headers.Add("Access-Control-Allow-Origin", originHeader);
            }
            
            if (!originHeader.IsNullOrWhiteSpace()) {
                Response.Flush();
            }
        }
    }
}
