using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using NS_Education.Tools.Encryption;
using NS_Education.Variables;

namespace NS_Education.Tools.Filters
{
    public class JwtAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            JwtAuth(actionContext);
            base.OnActionExecuting(actionContext);
        }

        private static void JwtAuth(ActionExecutingContext actionContext)
        {
            try
            {
                if (HasValidToken(actionContext, JwtConstants.Secret)) return;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"JwtAuth failed: {e}");
            }

            actionContext.Result = new HttpUnauthorizedResult("JWT 驗證錯誤");
        }

        private static bool HasValidToken(ActionExecutingContext actionContext, string secret)
        {
            return HasBearerAuthorization(actionContext) 
                   // 跳過開頭的 [Bearer ] 共 7 個字元
                   && JwtHelper.ValidateToken(actionContext.HttpContext.Request.Headers["Authorization"].Substring(7), secret);
        }

        private static bool HasBearerAuthorization(ActionExecutingContext actionContext)
        {
            return actionContext.HttpContext.Request.Headers["Authorization"]?.StartsWith("Bearer") ?? false;
        }
    }
}