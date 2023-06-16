using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.IdentityModel.Logging;
using NS_Education.Models.Errors;
using NS_Education.Tools;

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

            MvcHandler.DisableMvcResponseHeader = true;

#if DEBUG
            // 預設行為會在 Log 不顯示機敏資料 (PII)。
            IdentityModelEventSource.ShowPII = true;
#endif
        }

        protected void Application_BeginRequest()
        {
            RequestHelper.AddCorsHeaders();

            // 檢查是否存在此端點
            bool hasEndpoint = RequestHelper.HasEndpoint();
            bool isOptions = Request.HttpMethod == "OPTIONS";

            if (!hasEndpoint)
            {
                if (!isOptions)
                {
                    ResponseHelper.SetErrorResponse(Response,
                        new SystemError(new HttpException((int)HttpStatusCode.NotFound,
                            $"查無此端點：「{HttpContext.Current.Request.Url.AbsolutePath}」！")),
                        HttpStatusCode.NotFound);
                }

                Response.Flush();
                Response.Close();
                return;
            }

            if (isOptions)
            {
                // 如果是 OPTIONS，即刻回傳
                Response.Flush();
            }
        }
    }
}