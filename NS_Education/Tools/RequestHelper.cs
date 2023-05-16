using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using NS_Education.Tools.Filters.ResponsePrivilegeWrapper;

namespace NS_Education.Tools
{
    public static class RequestHelper
    {
        internal static ActionResult CreateWrappedResponse(ControllerContext filterContext, ActionResult originalActionResult, IEnumerable<Privilege> privileges = null)
        {
            // 如果這是 ExceptionContext 來的，把錯誤訊息設到 Status 中
            if (filterContext is ExceptionContext context)
            {
                if (context.Exception is HttpException httpException)
                {
                    context.HttpContext.Response.StatusCode = httpException.GetHttpCode();
                }
                else
                {
                    context.HttpContext.Response.StatusCode = 500;
                }

                context.HttpContext.Response.StatusDescription = GetStatusMessage(context);
            }

            // 取得此次 action 完整的 HTTP Response 並轉成 JObject
            JObject modify = JObject.FromObject(originalActionResult);

            // 轉換成 ActionResult
            ContentResult newActionResult = new ContentResult
            {
                Content = JObject.FromObject(new 
                {
                    Status = modify.SelectToken("Content.Status") ?? filterContext.HttpContext.Response.StatusCode,
                    StatusMessage = modify.SelectToken("Content.StatusMessage") ??
                                    filterContext.HttpContext.Response.StatusDescription,
                    ApiResponse = modify.SelectToken("Content.ApiResponse") 
                                  ?? (modify["Content"] is null 
                                      ? "" 
                                      : JToken.Parse(modify["Content"]?.Value<string>() ?? "")
                                      ),
                    Privileges = modify.SelectToken("Content.ApiResponse") ?? JToken.FromObject(privileges ?? Array.Empty<Privilege>())
                }).ToString(),
                ContentEncoding = Encoding.UTF8,
                ContentType = "Content-type: application/json; charset=utf-8",
                // 修改 Content 結構，修改為制式結構
            };
            
            // 正常化成 200
            filterContext.HttpContext.Response.StatusCode = 200;
            filterContext.HttpContext.Response.StatusDescription = "OK";

            return newActionResult;
        }

        private static string GetStatusMessage(ExceptionContext context)
        {
            string result = context.Exception.Message;
            if (context.Exception.InnerException == null) return result;
            
            // 取得最裡面的 InnerException，但避免無限迴圈
            HashSet<Exception> exceptions = new HashSet<Exception>();
            Exception curr = context.Exception;
            
            while (exceptions.Add(curr) && curr.InnerException != null)
            {
                curr = curr.InnerException;
            }

            result = curr.Message;

            return result;
        }

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
        }
    }
}