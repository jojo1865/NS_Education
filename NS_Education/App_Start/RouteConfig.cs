using System.Web.Mvc;
using System.Web.Routing;

namespace NS_Education
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ResverGetHistory",
                url: "Resver/GetHistory",
                defaults: new
                    { controller = "ResverHistory", action = "GetList" }
            );

            routes.MapRoute(
                name: "GroupDataGetUniqueNames",
                url: "GroupData/GetUniqueNames",
                defaults: new
                    { controller = "GroupDataUniqueNames", action = "GetList" }
            );

            routes.MapRoute(
                name: "ResverGetUniqueIds",
                url: "Resver/GetUniqueIds",
                defaults: new
                    { controller = "ResverUniqueId", action = "GetList" }
            );

            routes.MapRoute(
                name: "SiteDataGetUniqueNames",
                url: "SiteData/GetUniqueNames",
                defaults: new
                    { controller = "SiteDataUniqueNames", action = "GetList" }
            );


            routes.MapRoute(
                name: "CustomerQuestionGetUniqueAreas",
                url: "CustomerQuestion/GetUniqueAreas",
                defaults: new
                    { controller = "CustomerQuestionUniqueAreas", action = "GetList" }
            );

            routes.MapRoute(
                name: "CustomerGetRankings",
                url: "Customer/GetRankings",
                defaults: new { controller = "CustomerRanking", action = "GetList" }
            );

            routes.MapRoute(
                name: "UserDataGetLogTypes",
                url: "UserData/GetLogTypeList",
                defaults: new { controller = "UserDataLog", action = "GetList" }
            );

            routes.MapRoute(
                name: "UserDataSubmitLogKeepDays",
                url: "UserData/SubmitLogKeepDays",
                defaults: new { controller = "UserDataLog", action = "SubmitLogKeepDays" }
            );

            routes.MapRoute(
                name: "UserDataGetLogKeepDays",
                url: "UserData/GetLogKeepDays",
                defaults: new { controller = "UserDataLog", action = "GetLogKeepDays" }
            );

            routes.MapRoute(
                name: "UserDataGetUserLogList",
                url: "UserData/GetUserLogList",
                defaults: new { controller = "UserDataLog", action = "GetUserLogList" }
            );

            routes.MapRoute(
                name: "PrintReportGetResverListByIDs1",
                url: "PrintReport/GetResverListByIDs1",
                defaults: new { controller = "GetResverListByIDs1", action = "GetList" }
            );

            routes.MapRoute(
                name: "PrintReportGetResverListByIDs2",
                url: "PrintReport/GetResverListByIDs2",
                defaults: new { controller = "GetResverListByIDs2", action = "GetList" }
            );

            routes.MapRoute(
                name: "StaticCodeGetTypeList",
                url: "StaticCode/GetTypeList",
                defaults: new { controller = "StaticCodeTypeList", action = "GetList" }
            );

            routes.MapRoute(
                name: "ResverGetResverSiteList",
                url: "Resver/GetResverSiteList",
                defaults: new { controller = "ResverSite", action = "GetResverSiteList" }
            );

            routes.MapRoute(
                name: "ResverGetAllInfoById",
                url: "Resver/GetAllInfoById",
                defaults: new { controller = "Resver", action = "GetInfoById" }
            );

            routes.MapRoute(
                name: "ResverGetHeadList",
                url: "Resver/GetHeadList",
                defaults: new { controller = "Resver", action = "GetList" }
            );

            routes.MapRoute(
                name: "CategoryGetTypeList",
                url: "Category/GetTypeList",
                defaults: new { controller = "CategoryTypeList", action = "GetList" }
            );

            routes.MapRoute(
                name: "SiteDataGetTableList",
                url: "SiteData/GetTableList",
                defaults: new { controller = "SiteData", action = "GetList" }
            );

            routes.MapRoute(
                name: "SiteDataGetListForCalendar",
                url: "SiteData/GetCalendarList",
                defaults: new { controller = "SiteDataCalendar", action = "GetList" }
            );

            routes.MapRoute(
                name: "MenuDataGetListForApi",
                url: "MenuData/GetAPIList",
                defaults: new { controller = "MenuApi", action = "GetList" }
            );

            routes.MapRoute(
                name: "MenuDataSubmitForApi",
                url: "MenuData/SubmitAPI",
                defaults: new { controller = "MenuApi", action = "Submit" }
            );

            routes.MapRoute(
                name: "MenuDataGetListByUid",
                url: "MenuData/GetListByUID",
                defaults: new { controller = "MenuApiPerUser", action = "GetList" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}