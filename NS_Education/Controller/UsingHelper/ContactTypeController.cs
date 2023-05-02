using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.ContactType.GetList;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.Legacy
{
    public class ContactTypeController : 
        PublicClass, 
        IGetListLocal<ContactType_GetList_Output_Row_APIItem>
    {
        private readonly IGetListLocalHelper _getListLocalHelper;

        public ContactTypeController()
        {
            _getListLocalHelper = new GetListLocalHelper<ContactTypeController, ContactType_GetList_Output_Row_APIItem>(this);
        }
        
        #region GetList

        private static readonly ICollection<ContactType_GetList_Output_Row_APIItem> ContactTypes =
            new List<ContactType_GetList_Output_Row_APIItem>
            {
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = 0,
                    Title = "電話"
                },
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = 1,
                    Title = "傳真"
                },
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = 2,
                    Title = "手機"
                },
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = 3,
                    Title = "LINE"
                },
            }.AsReadOnly();
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            return await _getListLocalHelper.GetListLocal();
        }

        public async Task<ICollection<ContactType_GetList_Output_Row_APIItem>> GetListLocalResults()
        {
            return await Task.FromResult(ContactTypes);
        }
        
        #endregion
    }
}