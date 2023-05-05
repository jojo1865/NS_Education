using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Controller
{
    public class ErrorController : PublicClass {
        // 404
        public string NotFound() {
            var statusCode = (int)System.Net.HttpStatusCode.NotFound;
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;
            HttpContext.Response.StatusCode = statusCode;
            HttpContext.Response.TrySkipIisCustomErrors = true;
            return GetResponseJson();
        }

        // 500
        public string InternalError() {
            Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            Response.TrySkipIisCustomErrors = true;
            return GetResponseJson();
        }
    }
}