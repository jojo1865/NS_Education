using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using NS_Education.Models;
using NS_Education.Models.Errors;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools
{
    /// <summary>
    /// 這是專門處理 Response 包裝的工具。<br/>
    /// 主要功能之一是確保 HTTP Response 的 StatusCode 必定為 200。<br/>
    /// 原因是這個 API 以 ASP.NET MVC 5 危機底建成，然後在 IIS Server 上跑，<br/>
    /// 當不是 200 時，IIS 可能會自己回傳 BIG-5 格式的 html，造成編碼有問題。<br/>
    /// 所以，透過這樣的設計，盡量降低 IIS 干涉。
    /// </summary>
    internal static class ResponseHelper
    {
        /// <summary>
        /// 把 API Response 設定成 200 OK，並包裝成以下格式：<br/>
        /// {<br/>
        ///   Status: int, <br/>
        ///   StatusMessage: string, <br/>
        ///   ApiResponse: object
        /// }
        /// </summary>
        /// <remarks>關於格式的說明，詳見 API 文件</remarks>
        internal static ActionResult CreateWrappedResponse(ControllerContext filterContext,
            ActionResult originalActionResult)
        {
            // 如果是 fileContentResult，不做任何事

            if (originalActionResult is FileContentResult)
                return originalActionResult;

            // 如果這是 ExceptionContext 來的，把錯誤訊息設到 Status 中
            Exception exception = null;

            if (filterContext is ExceptionContext context)
            {
                exception = context.Exception;
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
                    // +- c. 如果 Content 是 null：建立預設內容。
                    ApiResponse = modify.SelectToken("Content.ApiResponse")?.Value<object>()
                                  ?? (modify["Content"] != null
                                      ? (object)JToken.Parse(modify["Content"]?.Value<string>() ?? "")
                                      : new CommonApiResponse
                                      {
                                          SuccessFlag = false,
                                          Errors = new[]
                                          {
                                              GetError(filterContext, exception)
                                          }
                                      }
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

        private static BaseError GetError(ControllerContext filterContext, Exception exception)
        {
            if (filterContext.HttpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                return new AuthError(exception);
            return exception != null
                ? new SystemError(exception)
                : new SystemError(filterContext.HttpContext.Response.StatusDescription);
        }

        /// <summary>
        /// 把 API Response 依以下格式回傳：<br/>
        /// {<br/>
        ///   Status: int, <br/>
        ///   StatusMessage: string, <br/>
        ///   ApiResponse: object
        /// }
        /// </summary>
        /// <remarks>關於格式的說明，詳見 API 文件</remarks>
        internal static void SetErrorResponse(HttpResponse response, SystemError error,
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            string content = JObject.FromObject(new FinalizedResponse
            {
                Status = (int)statusCode,
                StatusMessage = error.ErrorMessage,
                ApiResponse = new CommonApiResponse
                {
                    SuccessFlag = false,
                    Errors = new[] { error }
                }
            }).ToString();

            response.ClearContent();
            response.Write(content);

            response.StatusCode = 200;
            response.StatusDescription = "OK";
        }

        private static string GetExceptionActualMessage(ExceptionContext context)
        {
            return context.Exception.GetActualMessage();
        }
    }
}