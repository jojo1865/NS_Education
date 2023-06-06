using System.Linq;
using System.Web;

namespace NS_Education.Tools
{
    public static class RequestHelper
    {
        public static void AddCorsHeaders(HttpResponseBase httpResponse = null)
        {
            var httpRequest = HttpContext.Current.Request;
            httpResponse = httpResponse ?? new HttpResponseWrapper(HttpContext.Current.Response);

            string originHeader = "";

            if (httpRequest.Headers.AllKeys.Contains("Origin"))
            {
                originHeader = httpRequest.Headers["Origin"];
            }
            else if (httpRequest.Headers.AllKeys.Contains("Referer"))
            {
                originHeader = httpRequest.Headers["Referer"];
            }

            if (!string.IsNullOrWhiteSpace(originHeader))
            {
                if (originHeader.EndsWith("/"))
                    originHeader = originHeader.Substring(0, originHeader.Length - 1);
                httpResponse.Headers.Remove("Access-Control-Allow-Origin");
                httpResponse.Headers.Add("Access-Control-Allow-Origin", originHeader);
            }

            httpResponse.Headers.Remove("Access-Control-Allow-Credentials");
            httpResponse.Headers.Remove("Access-Control-Allow-Headers");
            httpResponse.Headers.Remove("Access-Control-Allow-Method");
            httpResponse.Headers.Add("Access-Control-Allow-Credentials", "true");
            httpResponse.Headers.Add("Access-Control-Allow-Headers",
                "Accepts, Content-Type, Origin, X-My-Header, Pragma, Authorization");
            httpResponse.Headers.Add("Access-Control-Allow-Method", "GET, POST");
            httpResponse.Headers.Add("Content-Type", "application/json; charset=UTF-8");
        }
    }
}