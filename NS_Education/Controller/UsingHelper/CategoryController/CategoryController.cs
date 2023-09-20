using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using Microsoft.Ajax.Utilities;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Category.GetInfoById;
using NS_Education.Models.APIItems.Controller.Category.GetList;
using NS_Education.Models.APIItems.Controller.Category.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.CategoryController
{
    public class CategoryController : PublicClass,
        IGetListPaged<B_Category, Category_GetList_Input_APIItem, Category_GetList_Output_Row_APIItem>,
        IGetInfoById<B_Category, Category_GetInfoById_Output_APIItem>,
        IDeleteItem<B_Category>,
        ISubmit<B_Category, Category_Submit_Input_APIItem>,
        IChangeActive<B_Category>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Category_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<Category_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public CategoryController()
        {
            _getListPagedHelper = new GetListPagedHelper<CategoryController, B_Category, Category_GetList_Input_APIItem,
                Category_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<CategoryController, B_Category>(this);
            _submitHelper = new SubmitHelper<CategoryController, B_Category, Category_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<CategoryController, B_Category>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<CategoryController, B_Category, Category_GetInfoById_Output_APIItem>(this);
        }

        #endregion

        #region GetList

        //取得分類列表
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Category_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Category_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.CategoryType >= -1,
                    () => AddError(EmptyNotAllowed("分類類別", nameof(input.CategoryType))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_Category> GetListPagedOrderedQuery(Category_GetList_Input_APIItem input)
        {
            var query = DC.B_Category.AsQueryable();

            if (!AjaxMinExtensions.IsNullOrWhiteSpace(input.Keyword))
                query = query.Where(c =>
                    c.TitleC.Contains(input.Keyword) || c.TitleE.Contains(input.Keyword) ||
                    c.Code.Contains(input.Keyword));

            if (input.CategoryType > -1)
                query = query.Where(c => c.CategoryType == input.CategoryType);

            return query.OrderBy(c => c.CategoryType)
                .ThenBy(c => c.SortNo)
                .ThenBy(c => c.BCID);
        }

        public async Task<Category_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_Category entity)
        {
            B_Category parent = entity.ParentID.IsZeroOrAbove()
                ? await DC.B_Category.FirstOrDefaultAsync(c => c.BCID == entity.ParentID)
                : null;

            return await Task.FromResult(new Category_GetList_Output_Row_APIItem
            {
                BCID = entity.BCID,
                iCategoryType = entity.CategoryType,
                sCategoryType = entity.CategoryType < CategoryTypes.Length ? CategoryTypes[entity.CategoryType] : "",
                ParentID = entity.ParentID,
                ParentTitleC = parent?.TitleC ?? "",
                ParentTitleE = parent?.TitleE ?? "",
                Code = entity.Code,
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                SortNo = entity.SortNo
            });
        }

        #endregion

        #region GetInfoById

        //取得分類的內容
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            // 特殊邏輯：如果 id 為 0，回傳一個空的
            return id == 0
                ? await GetInfoByIdZero()
                : await _getInfoByIdHelper.GetInfoById(id);
        }

        private async Task<string> GetInfoByIdZero()
        {
            Category_GetInfoById_Output_APIItem response = new Category_GetInfoById_Output_APIItem
            {
                CategoryTypeList = CategoryTypeListController.GetCategoryTypeList(CategoryTypes)
            };

            return await Task.FromResult(GetResponseJson(response));
        }

        public IQueryable<B_Category> GetInfoByIdQuery(int id)
        {
            return DC.B_Category.Where(c => c.BCID == id);
        }

        public async Task<Category_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_Category entity)
        {
            return await Task.FromResult(new Category_GetInfoById_Output_APIItem
            {
                BCID = entity.BCID,
                iCategoryType = entity.CategoryType,
                sCategoryType = entity.CategoryType < CategoryTypes.Length ? CategoryTypes[entity.CategoryType] : "",
                CategoryTypeList = CategoryTypeListController.GetCategoryTypeList(CategoryTypes),
                ParentID = entity.ParentID,
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                SortNo = entity.SortNo
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

        public IQueryable<B_Category> ChangeActiveQuery(int id)
        {
            return DC.B_Category.Where(c => c.BCID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<B_Category> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.B_Category.Where(c => ids.Contains(c.BCID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Category_Submit_Input_APIItem.BCID))]
        public async Task<string> Submit(Category_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Category_Submit_Input_APIItem input)
        {
            return input.BCID == 0;
        }

        private async Task<int> GetNewSortNo(Category_Submit_Input_APIItem input)
        {
            int newSortNo = await DC.B_Category
                .Where(c => c.CategoryType == input.CategoryType)
                .OrderByDescending(c => c.SortNo)
                .Select(c => c.SortNo)
                .FirstOrDefaultAsync() + 1;
            return newSortNo;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Category_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.BCID == 0,
                    () => AddError(WrongFormat("分類 ID", nameof(Category_Submit_Input_APIItem.BCID))))
                .Validate(i => i.ParentID == 0 || i.ParentID != i.BCID,
                    () => AddError(UnsupportedValue("上層 ID", nameof(input.ParentID), "不允許將自己設為上層！")))
                .ValidateAsync(async i => i.ParentID == 0 || i.ParentID > 0 &&
                        await DC.B_Category.ValidateIdExists(i.ParentID, nameof(B_Category.BCID)),
                    () => AddError(NotFound("上層 ID", nameof(input.ParentID))))
                .Validate(i => i.CategoryType.IsInBetween(0, 9),
                    () => AddError(OutOfRange("分類所屬類別", nameof(Category_Submit_Input_APIItem.CategoryType), 0, 9)))
                .Validate(i => i.TitleC.HasContent() || i.TitleE.HasContent(),
                    () => AddError(EmptyNotAllowed("分類名稱", nameof(Category_Submit_Input_APIItem.TitleC))))
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("編碼", nameof(Category_Submit_Input_APIItem.Code), 0, 10)))
                .Validate(i => i.TitleC.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("中文名稱", nameof(Category_Submit_Input_APIItem.TitleC), 0, 50)))
                .Validate(i => i.TitleE.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("英文名稱", nameof(Category_Submit_Input_APIItem.TitleE), 0, 50)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<B_Category> SubmitCreateData(Category_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new B_Category
            {
                CategoryType = input.CategoryType,
                ParentID = input.ParentID,
                Code = input.Code,
                TitleC = input.TitleC,
                TitleE = input.TitleE,
                SortNo = await GetNewSortNo(input)
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Category_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.BCID.IsAboveZero(),
                    () => AddError(EmptyNotAllowed("分類 ID", nameof(Category_Submit_Input_APIItem.BCID))))
                .Validate(i => i.ParentID == 0 || i.ParentID != i.BCID,
                    () => AddError(UnsupportedValue("上層 ID", nameof(input.ParentID), "不允許將自己設為上層！")))
                .ValidateAsync(async i => i.ParentID == 0 || i.ParentID > 0 &&
                        await DC.B_Category.ValidateIdExists(i.ParentID, nameof(B_Category.BCID)),
                    () => AddError(NotFound("上層 ID", nameof(input.ParentID))))
                .Validate(i => i.CategoryType.IsInBetween(0, 9),
                    () => AddError(OutOfRange("分類所屬類別", nameof(Category_Submit_Input_APIItem.CategoryType), 0, 9)))
                .Validate(i => i.TitleC.HasContent() || i.TitleE.HasContent(),
                    () => AddError(EmptyNotAllowed("分類名稱", nameof(Category_Submit_Input_APIItem.TitleC))))
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("編碼", nameof(Category_Submit_Input_APIItem.Code), 0, 10)))
                .Validate(i => i.TitleC.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("中文名稱", nameof(Category_Submit_Input_APIItem.TitleC), 0, 50)))
                .Validate(i => i.TitleE.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("英文名稱", nameof(Category_Submit_Input_APIItem.TitleE), 0, 50)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_Category> SubmitEditQuery(Category_Submit_Input_APIItem input)
        {
            return DC.B_Category.Where(c => c.BCID == input.BCID);
        }

        public void SubmitEditUpdateDataFields(B_Category data, Category_Submit_Input_APIItem input)
        {
            data.CategoryType = input.CategoryType;
            data.ParentID = input.ParentID;
            data.Code = input.Code ?? data.Code;
            data.TitleC = input.TitleC ?? data.TitleC;
            data.TitleE = input.TitleE ?? data.TitleE;
            // 只在 CategoryType 變更時才生成新的 SortNo
            data.SortNo = input.CategoryType == data.CategoryType
                ? data.SortNo
                : Task.Run(() => GetNewSortNo(input)).Result;
        }

        #endregion

        #endregion
    }
}