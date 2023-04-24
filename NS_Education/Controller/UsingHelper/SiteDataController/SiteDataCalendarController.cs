using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using NS_Education.Models.APIItems.SiteData.GetListForCalendar;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.SiteDataController
{
    public class SiteDataCalendarController : PublicClass, 
        IGetListPaged<B_SiteData, SiteData_GetListForCalendar_Input_APIItem, SiteData_GetListForCalendar_Output_APIItem>
    {
        #region GetList - For calendar
        
        // route 請參見 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(SiteData_GetListForCalendar_Input_APIItem input)
        {
            return ChangeJson(new {Ayo = "Hello world!"});
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetListForCalendar_Input_APIItem input)
        {
            throw new System.NotImplementedException();
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetListForCalendar_Input_APIItem input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<SiteData_GetListForCalendar_Output_APIItem> GetListPagedEntityToRow(B_SiteData entity)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}