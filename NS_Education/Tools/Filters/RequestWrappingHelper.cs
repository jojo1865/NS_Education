using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using NS_Education.Tools.Filters.ResponsePrivilegeWrapper;

namespace NS_Education.Tools.Filters
{
    public static class RequestWrappingHelper
    {
        internal static ActionResult CreateWrappedResponse(ControllerContext filterContext, ActionResult originalActionResult, IEnumerable<Privilege> privileges = null)
        {
            // 如果這是 ExceptionContext 來的，把錯誤訊息設到 Status 中
            if (filterContext is ExceptionContext context)
            {
                context.HttpContext.Response.StatusCode = 500;
                context.HttpContext.Response.StatusDescription =
                    context.Exception.Message;
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
    }
}