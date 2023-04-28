using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.MenuData.MenuApi.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.MenuDataController
{
    /// <summary>
    /// 處理子選單相關的 API。<br/>
    /// 因為目前開的 Route 為 MenuData，因此還是歸類在 MenuDataController，<br/>
    /// 但處理的內容會與 MenuAPI 有關。
    /// </summary>
    public class MenuApiController : PublicClass,
        IGetListAll<MenuAPI, MenuApi_GetList_Input_APIItem, MenuApi_GetList_Output_Row_APIItem>
    {
        #region Common

        private static readonly string[] ApiTypes = {"瀏覽", "新增", "修改", "刪除", "匯出", "登入/登出"}; 
        
        #endregion
        
        #region Initialization

        private readonly IGetListAllHelper<MenuApi_GetList_Input_APIItem> _getListAllHelper;

        public MenuApiController()
        {
            _getListAllHelper = new GetListAllHelper<MenuApiController, MenuAPI, MenuApi_GetList_Input_APIItem, MenuApi_GetList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetList
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> GetList(MenuApi_GetList_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(MenuApi_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MDID.IsZeroOrAbove(), () => AddError(EmptyNotAllowed("選單 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<MenuAPI> GetListAllOrderedQuery(MenuApi_GetList_Input_APIItem input)
        {
            var query = DC.MenuAPI.AsQueryable();

            if (input.MDID.IsAboveZero())
                query = query.Where(ma => ma.MDID == input.MDID);

            return query.OrderBy(ma => ma.APIType)
                .ThenBy(ma => ma.APIURL)
                .ThenBy(ma => ma.SeqNo);
        }

        public async Task<MenuApi_GetList_Output_Row_APIItem> GetListAllEntityToRow(MenuAPI entity)
        {
            return await Task.FromResult(new MenuApi_GetList_Output_Row_APIItem
            {
                SeqNo = entity.SeqNo,
                ApiUrl = entity.APIURL ?? "",
                iApiType = entity.APIType,
                sApiType = entity.APIType < ApiTypes.Length ? ApiTypes[entity.APIType] : "",
                Note = entity.Note ?? ""
            });
        }
        #endregion
    }
}