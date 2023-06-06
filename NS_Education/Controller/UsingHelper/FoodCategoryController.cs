using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.FoodCategory.GetInfoById;
using NS_Education.Models.APIItems.Controller.FoodCategory.GetList;
using NS_Education.Models.APIItems.Controller.FoodCategory.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    public class FoodCategoryController : PublicClass,
        IGetListPaged<D_FoodCategory, FoodCategory_GetList_Input_APIItem, FoodCategory_GetList_Output_Row_APIItem>,
        IGetInfoById<D_FoodCategory, FoodCategory_GetInfoById_Output_APIItem>,
        IDeleteItem<D_FoodCategory>,
        ISubmit<D_FoodCategory, FoodCategory_Submit_Input_APIItem>,
        IChangeActive<D_FoodCategory>
    {
        #region Intialization

        private readonly IGetListPagedHelper<FoodCategory_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<FoodCategory_Submit_Input_APIItem> _submitHelper;

        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public FoodCategoryController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<FoodCategoryController, D_FoodCategory, FoodCategory_GetList_Input_APIItem,
                    FoodCategory_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<FoodCategoryController, D_FoodCategory>(this);
            _submitHelper =
                new SubmitHelper<FoodCategoryController, D_FoodCategory, FoodCategory_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<FoodCategoryController, D_FoodCategory>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<FoodCategoryController, D_FoodCategory, FoodCategory_GetInfoById_Output_APIItem>(
                    this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(FoodCategory_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(FoodCategory_GetList_Input_APIItem input)
        {
            // 輸入無須驗證
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<D_FoodCategory> GetListPagedOrderedQuery(FoodCategory_GetList_Input_APIItem input)
        {
            var query = DC.D_FoodCategory.AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(fc => fc.Title.Contains(input.Keyword) || fc.Code.Contains(input.Keyword));

            return query.OrderBy(fc => fc.DFCID);
        }

        public async Task<FoodCategory_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_FoodCategory entity)
        {
            return await Task.FromResult(new FoodCategory_GetList_Output_Row_APIItem
            {
                DFCID = entity.DFCID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                UnitPrice = entity.UnitPrice,
                Price = entity.Price
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<D_FoodCategory> GetInfoByIdQuery(int id)
        {
            return DC.D_FoodCategory.Where(fc => fc.DFCID == id);
        }

        public async Task<FoodCategory_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            D_FoodCategory entity)
        {
            return await Task.FromResult(new FoodCategory_GetInfoById_Output_APIItem
            {
                DFCID = entity.DFCID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                UnitPrice = entity.UnitPrice,
                Price = entity.Price
            });
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<D_FoodCategory> ChangeActiveQuery(int id)
        {
            return DC.D_FoodCategory.Where(fc => fc.DFCID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<D_FoodCategory> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.D_FoodCategory.Where(fc => ids.Contains(fc.DFCID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null,
            nameof(FoodCategory_Submit_Input_APIItem.DFCID))]
        public async Task<string> Submit(FoodCategory_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(FoodCategory_Submit_Input_APIItem input)
        {
            return input.DFCID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(FoodCategory_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DFCID == 0, () => AddError(WrongFormat("餐種 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.Code.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("編碼", 0, 10)))
                .Validate(i => i.Title.HasLengthBetween(1, 60), () => AddError(LengthOutOfRange("中文名稱", 1, 60)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_FoodCategory> SubmitCreateData(FoodCategory_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_FoodCategory
            {
                Code = input.Code,
                Title = input.Title,
                UnitPrice = input.UnitPrice,
                Price = input.Price
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(FoodCategory_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DFCID.IsAboveZero(), () => AddError(EmptyNotAllowed("餐種 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.Code.HasLengthBetween(0, 10), () => AddError(LengthOutOfRange("編碼", 0, 10)))
                .Validate(i => i.Title.HasLengthBetween(1, 60), () => AddError(LengthOutOfRange("中文名稱", 1, 60)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_FoodCategory> SubmitEditQuery(FoodCategory_Submit_Input_APIItem input)
        {
            return DC.D_FoodCategory.Where(fc => fc.DFCID == input.DFCID);
        }

        public void SubmitEditUpdateDataFields(D_FoodCategory data, FoodCategory_Submit_Input_APIItem input)
        {
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.UnitPrice = input.UnitPrice;
            data.Price = input.Price;
        }

        #endregion

        #endregion
    }
}