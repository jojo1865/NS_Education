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
                name: "SiteDataGetListForCalendar",
                url: "SiteData/GetCalendarList/{id}",
                defaults: new { controller = "SiteDataCalendar", action = "GetList", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
