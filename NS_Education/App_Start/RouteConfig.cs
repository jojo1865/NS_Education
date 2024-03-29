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
                name: "PrintReport14Excel",
                url: "PrintReport/Excel/14",
                defaults: new
                    { controller = "Report14", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport18Excel",
                url: "PrintReport/Excel/18",
                defaults: new
                    { controller = "Report18", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport18",
                url: "PrintReport/18",
                defaults: new
                    { controller = "Report18", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport17Excel",
                url: "PrintReport/Excel/17",
                defaults: new
                    { controller = "Report17", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport16Excel",
                url: "PrintReport/Excel/16",
                defaults: new
                    { controller = "Report16", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport15Excel",
                url: "PrintReport/Excel/15",
                defaults: new
                    { controller = "Report15", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport13Excel",
                url: "PrintReport/Excel/13",
                defaults: new
                    { controller = "Report13", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport12Excel",
                url: "PrintReport/Excel/12",
                defaults: new
                    { controller = "Report12", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport11Excel",
                url: "PrintReport/Excel/11",
                defaults: new
                    { controller = "Report11", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport10Excel",
                url: "PrintReport/Excel/10",
                defaults: new
                    { controller = "Report10", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport9Excel",
                url: "PrintReport/Excel/9",
                defaults: new
                    { controller = "Report9", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport8Excel",
                url: "PrintReport/Excel/8",
                defaults: new
                    { controller = "Report8", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport7Excel",
                url: "PrintReport/Excel/7",
                defaults: new
                    { controller = "Report7", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport6Excel",
                url: "PrintReport/Excel/6",
                defaults: new
                    { controller = "Report6", action = "GetExcel" }
            );
            routes.MapRoute(
                name: "PrintReport5Excel",
                url: "PrintReport/Excel/5",
                defaults: new
                    { controller = "Report5", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport3Excel",
                url: "PrintReport/Excel/3",
                defaults: new
                    { controller = "Report3", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport2Excel",
                url: "PrintReport/Excel/2",
                defaults: new
                    { controller = "Report2", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport1Excel",
                url: "PrintReport/Excel/1",
                defaults: new
                    { controller = "GetResverListByIDs1", action = "GetExcel" }
            );

            routes.MapRoute(
                name: "PrintReport3Pdf",
                url: "PrintReport/Pdf/3",
                defaults: new
                    { controller = "Report3", action = "GetPdf" }
            );

            routes.MapRoute(
                name: "PrintReport8Pdf",
                url: "PrintReport/Pdf/8",
                defaults: new
                    { controller = "Report8", action = "GetPdf" }
            );

            routes.MapRoute(
                name: "PrintReport8",
                url: "PrintReport/8",
                defaults: new
                    { controller = "Report8", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport17Pdf",
                url: "PrintReport/Pdf/17",
                defaults: new
                    { controller = "Report17", action = "GetPdf" }
            );

            routes.MapRoute(
                name: "PrintReport17",
                url: "PrintReport/17",
                defaults: new
                    { controller = "Report17", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport2Pdf",
                url: "PrintReport/Pdf/2",
                defaults: new
                    { controller = "Report2", action = "GetPdf" }
            );

            routes.MapRoute(
                name: "PrintReport2",
                url: "PrintReport/2",
                defaults: new
                    { controller = "Report2", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport12Pdf",
                url: "PrintReport/Pdf/12",
                defaults: new
                    { controller = "Report12", action = "GetPdf" }
            );

            routes.MapRoute(
                name: "PrintReport12",
                url: "PrintReport/12",
                defaults: new
                    { controller = "Report12", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport9",
                url: "PrintReport/9",
                defaults: new
                    { controller = "Report9", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport7",
                url: "PrintReport/7",
                defaults: new
                    { controller = "Report7", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport3",
                url: "PrintReport/3",
                defaults: new
                    { controller = "Report3", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport5Pdf",
                url: "PrintReport/Pdf/5",
                defaults: new
                    { controller = "Report5", action = "GetPdf" }
            );

            routes.MapRoute(
                name: "PrintReport5",
                url: "PrintReport/5",
                defaults: new
                    { controller = "Report5", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport6",
                url: "PrintReport/6",
                defaults: new
                    { controller = "Report6", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport10",
                url: "PrintReport/10",
                defaults: new
                    { controller = "Report10", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport11",
                url: "PrintReport/11",
                defaults: new
                    { controller = "Report11", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport16",
                url: "PrintReport/16",
                defaults: new
                    { controller = "Report16", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport15",
                url: "PrintReport/15",
                defaults: new
                    { controller = "Report15", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport14",
                url: "PrintReport/14",
                defaults: new
                    { controller = "Report14", action = "Get" }
            );

            routes.MapRoute(
                name: "PrintReport13",
                url: "PrintReport/13",
                defaults: new
                    { controller = "Report13", action = "Get" }
            );


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
                name: "ResverCopy",
                url: "Resver/GetAllInfoById/Copy",
                defaults: new { controller = "Resver", action = "Copy" }
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