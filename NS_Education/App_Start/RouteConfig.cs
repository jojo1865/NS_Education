﻿using System.Web.Mvc;
using System.Web.Routing;

namespace NS_Education
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ResverGetAllInfoById",
                url: "Resver/GetAllInfoById/{id}",
                defaults: new { controller = "Resver", action = "GetInfoById", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "ResverGetHeadList",
                url: "Resver/GetHeadList/{id}",
                defaults: new { controller = "Resver", action = "GetList", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "CategoryGetTypeList",
                url: "Category/GetTypeList/{id}",
                defaults: new { controller = "CategoryTypeList", action = "GetList", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "SiteDataGetListForCalendar",
                url: "SiteData/GetCalendarList/{id}",
                defaults: new { controller = "SiteDataCalendar", action = "GetList", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "UserDataGetUserLogList",
                url: "UserData/GetUserLogList/{id}",
                defaults: new { controller = "UserDataLog", action = "GetList", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MenuDataGetListForApi",
                url: "MenuData/GetAPIList/{id}",
                defaults: new { controller = "MenuApi", action = "GetList", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "MenuDataSubmitForApi",
                url: "MenuData/SubmitAPI/{id}",
                defaults: new { controller = "MenuApi", action = "Submit", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "MenuDataGetListByUid",
                url: "MenuData/GetListByUID/{id}",
                defaults: new { controller = "MenuApiPerUser", action = "GetList", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
