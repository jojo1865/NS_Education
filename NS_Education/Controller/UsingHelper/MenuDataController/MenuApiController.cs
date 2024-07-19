using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.MenuData.MenuApi.GetList;
using NS_Education.Models.APIItems.Controller.MenuData.MenuApi.Submit;
using NS_Education.Models.Entities;
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
        IGetListAll<MenuAPI, MenuApi_GetList_Input_APIItem, MenuApi_GetList_Output_Row_APIItem>,
        ISubmit<MenuAPI, MenuApi_Submit_Input_APIItem>
    {
        #region Common

        private static readonly string[] ApiTypes = { "瀏覽", "新增", "修改", "刪除", "匯出", "登入/登出" };

        #endregion

        #region Initialization

        private readonly IGetListAllHelper<MenuApi_GetList_Input_APIItem> _getListAllHelper;

        private readonly ISubmitHelper<MenuApi_Submit_Input_APIItem> _submitHelper;

        public MenuApiController()
        {
            _getListAllHelper =
                new GetListAllHelper<MenuApiController, MenuAPI, MenuApi_GetList_Input_APIItem,
                    MenuApi_GetList_Output_Row_APIItem>(this);
            _submitHelper = new SubmitHelper<MenuApiController, MenuAPI, MenuApi_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> GetList(MenuApi_GetList_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(MenuApi_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MDID.IsZeroOrAbove(), () => AddError(EmptyNotAllowed("選單 ID", nameof(input.MDID))))
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

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(MenuApi_Submit_Input_APIItem.SeqNo))]
        public async Task<string> Submit(MenuApi_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(MenuApi_Submit_Input_APIItem input)
        {
            return input.SeqNo == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(MenuApi_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .ValidateAsync(async i => await DC.MenuData.ValidateIdExists(input.MDID, nameof(MenuData.MDID)),
                    () => AddError(NotFound("選單 ID", nameof(input.MDID))))
                .Validate(i => i.SeqNo == 0, () => AddError(WrongFormat("API 流水號", nameof(input.SeqNo))))
                .Validate(i => i.ApiUrl.HasContent(), () => AddError(EmptyNotAllowed("API 網址", nameof(input.ApiUrl))))
                .Validate(i => i.ApiUrl.HasLengthBetween(1, 100),
                    () => AddError(LengthOutOfRange("API 網址", nameof(input.ApiUrl), 1, 100)))
                .Validate(i => i.APIType.IsInBetween(0, ApiTypes.Length),
                    () => AddError(WrongFormat("API 屬性 ID", nameof(input.APIType))))
                .IsValid();

            return isValid;
        }

        public async Task<MenuAPI> SubmitCreateData(MenuApi_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new MenuAPI
            {
                MDID = input.MDID,
                APIURL = input.ApiUrl,
                APIType = input.APIType,
                Note = input.Note
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(MenuApi_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .ValidateAsync(async i => await DC.MenuData.ValidateIdExists(input.MDID, nameof(MenuData.MDID)),
                    () => AddError(NotFound("選單 ID", nameof(input.MDID))))
                .Validate(i => i.SeqNo.IsAboveZero(), () => AddError(WrongFormat("API 流水號", nameof(input.SeqNo))))
                .Validate(i => i.ApiUrl.HasContent(), () => AddError(EmptyNotAllowed("API 網址", nameof(input.ApiUrl))))
                .Validate(i => i.ApiUrl.HasLengthBetween(1, 100),
                    () => AddError(LengthOutOfRange("API 網址", nameof(input.ApiUrl), 1, 100)))
                .Validate(i => i.APIType.IsInBetween(0, ApiTypes.Length),
                    () => AddError(WrongFormat("API 屬性 ID", nameof(input.APIType))))
                .IsValid();

            return isValid;
        }

        public IQueryable<MenuAPI> SubmitEditQuery(MenuApi_Submit_Input_APIItem input)
        {
            return DC.MenuAPI.Where(api => api.SeqNo == input.SeqNo);
        }

        public void SubmitEditUpdateDataFields(MenuAPI data, MenuApi_Submit_Input_APIItem input)
        {
            data.MDID = input.MDID;
            data.APIURL = input.ApiUrl ?? data.APIURL;
            data.APIType = input.APIType;
            data.Note = input.Note ?? "";
        }

        #endregion

        #endregion
    }
}