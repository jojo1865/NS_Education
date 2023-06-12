using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using NS_Education.Models;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools
{
    internal static class ResponseHelper
    {
        /// <summary>
        /// 把 API Response 設定成 200 OK，並包裝成以下格式：<br/>
        /// {<br/>
        ///   Status: int, <br/>
        ///   StatusMessage: string, <br/>
        ///   ApiResponse: object, <br/>
        ///   Privileges: array<br/>
        /// }
        /// </summary>
        /// <remarks>關於格式的說明，詳見 API 文件</remarks>
        internal static ActionResult CreateWrappedResponse(ControllerContext filterContext,
            ActionResult originalActionResult)
        {
            // 如果這是 ExceptionContext 來的，把錯誤訊息設到 Status 中
            if (filterContext is ExceptionContext context)
            {
                string forceMessage = null;
                if (context.Exception is HttpException httpException)
                {
                    context.HttpContext.Response.StatusCode = httpException.GetHttpCode();
                }
                else if (context.Exception is DbEntityValidationException dbEntityValidationException)
                {
                    context.HttpContext.Response.StatusCode = 200;
                    forceMessage = String.Join(", ",
                        dbEntityValidationException.EntityValidationErrors
                            .SelectMany(e => e.ValidationErrors)
                            .Select(e => e.ErrorMessage));
                }
                else
                {
                    context.HttpContext.Response.StatusCode = 500;
                }

                context.HttpContext.Response.StatusDescription = forceMessage ??
                                                                 GetExceptionActualMessage(context);
            }

            // 取得此次 action 完整的 HTTP Response 並轉成 JObject
            JObject modify = JObject.FromObject(originalActionResult);

            // 轉換成 ActionResult
            ContentResult newActionResult = new ContentResult
            {
                Content = JObject.FromObject(new FinalizedResponse
                {
                    Status = modify.SelectToken("Content.Status")?.Value<int>() ??
                             filterContext.HttpContext.Response.StatusCode,
                    StatusMessage = modify.SelectToken("Content.StatusMessage")?.Value<string>() ??
                                    filterContext.HttpContext.Response.StatusDescription,
                    // ApiResponse 視可以取得的值決定：
                    // |- a. 如果已經有 ApiResponse：用 ApiResponse。
                    // |- b. 否則：視為還沒經過 Wrap，把原有的整個 Response Content 轉換成 ApiResponse。
                    // +- c. 如果 Content 是 null：放入空字串。
                    ApiResponse = modify.SelectToken("Content.ApiResponse")?.Value<object>()
                                  ?? (modify["Content"] != null
                                      ? JToken.Parse(modify["Content"]?.Value<string>() ?? "")
                                      : null
                                  )
                }).ToString(),
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json; charset=utf-8",
                // 修改 Content 結構，修改為制式結構
            };

            // 正常化成 200
            filterContext.HttpContext.Response.StatusCode = 200;
            filterContext.HttpContext.Response.StatusDescription = "OK";

            return newActionResult;
        }

        private static string GetExceptionActualMessage(ExceptionContext context)
        {
            return context.Exception.GetActualMessage();
        }
    }
}