using System;
using System.Web.Mvc;

namespace NS_Education.Tools.Filters.FinalizeResponseFilter
{
    /// <summary>
    /// 在 Action 執行完之後， Response 外面多包一層後回傳。<br/>
    /// 同時，把 Response 轉成 200，列入欄位表示 Status 和錯誤訊息。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class FinalizeResponseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            WrapResponse(filterContext);

            base.OnActionExecuted(filterContext);
        }

        private static void WrapResponse(ActionExecutedContext filterContext)
        {
            // 取得此次 action 完整的 HTTP Response 並轉成 JObject
            filterContext.Result =
                ResponseHelper.CreateWrappedResponse(filterContext, filterContext.Result);
        }
    }
}