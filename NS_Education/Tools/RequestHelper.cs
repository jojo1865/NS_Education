using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools
{
    public static class RequestHelper
    {
        private static readonly IEnumerable<string> UrlsFromRouteTable = RouteTable.Routes
            .Select(r => ((Route)r).Url)
            .Where(url => !url.Contains("{"));

        private static readonly HashSet<string> Urls = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(System.Web.Mvc.Controller).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods())
            .Where(m => m.IsPublic && m.CustomAttributes.Any(ca =>
                typeof(ActionMethodSelectorAttribute).IsAssignableFrom(ca.AttributeType)))
            .Select(m => $"{m.DeclaringType?.Name.Replace("Controller", "")}/{m.Name}")
            // 去除所有 RouteTable 中的 controller,action，因為 RouteTable 表示以自訂網址覆蓋
            .Except(RouteTable.Routes
                .Select(r => r as Route)
                .Where(r => r != null
                            && r.Defaults != null
                            && r.Defaults.ContainsKey("controller")
                            && r.Defaults.ContainsKey("action"))
                .Select(r => $"{r.Defaults["controller"]}/{r.Defaults["action"]}"))
            // 和 RouteTable 中覆蓋的 Paths 結合
            .Concat(UrlsFromRouteTable)
            .Where(s => s.HasContent())
            .Distinct()
            .Select(s => s.ToLowerInvariant())
            .ToHashSet();

        public static void AddCorsHeaders(HttpResponseBase httpResponse = null)
        {
            var httpRequest = HttpContext.Current.Request;
            httpResponse = httpResponse ?? new HttpResponseWrapper(HttpContext.Current.Response);

            string origin = "";

            if (httpRequest.Headers.AllKeys.Contains("Origin"))
            {
                origin = httpRequest.Headers["Origin"];
            }
            else if (httpRequest.Headers.AllKeys.Contains("Referer"))
            {
                origin = httpRequest.Headers["Referer"];
            }

            if (!string.IsNullOrWhiteSpace(origin))
            {
                origin = origin.EndsWith("/") ? origin.Substring(0, origin.Length - 1) : origin;

                httpResponse.Headers.Remove("Access-Control-Allow-Origin");
                httpResponse.Headers.Add("Access-Control-Allow-Origin", origin);
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

        public static bool HasEndpoint()
        {
            var httpRequest = HttpContext.Current.Request;
            string pathToFind = httpRequest.Path.StartsWith("/") ? httpRequest.Path.Substring(1) : httpRequest.Path;

            return Urls.Contains(pathToFind.ToLowerInvariant());
        }
    }
}