using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems.SiteData.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Controllers.SiteDataController
{
    public class SiteDataController : PublicClass,
        IGetListPaged<B_SiteData, SiteData_GetList_Input_APIItem, SiteData_GetList_Output_Row_APIItem>
    {
        #region GetList
        public async Task<string> GetList(SiteData_GetList_Input_APIItem input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetList_Input_APIItem input)
        {
            throw new System.NotImplementedException();
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetList_Input_APIItem input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<SiteData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_SiteData entity)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}