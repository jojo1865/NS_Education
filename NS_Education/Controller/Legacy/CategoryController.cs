using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Category;
using NS_Education.Models.APIItems.Category.GetList;
using NS_Education.Models.APIItems.Category.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Tools.Filters.ResponsePrivilegeWrapper;

namespace NS_Education.Controller.Legacy
{
    public class CategoryController : PublicClass,
        IGetListPaged<B_Category, Category_GetList_Input_APIItem, Category_GetList_Output_Row_APIItem>,
        IDeleteItem<B_Category>,
        ISubmit<B_Category, Category_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Category_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<Category_Submit_Input_APIItem> _submitHelper;

        public CategoryController()
        {
            _getListPagedHelper = new GetListPagedHelper<CategoryController, B_Category, Category_GetList_Input_APIItem,
                Category_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<CategoryController, B_Category>(this);
            _submitHelper = new SubmitHelper<CategoryController, B_Category, Category_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetTypeList

        //取得分類的類別列表
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetTypeList()
        {
            List<cSelectItem> TIs = new List<cSelectItem>();
            await Task.Run(() =>
                TIs.AddRange(sCategoryTypes.Select((t, i) => new cSelectItem { ID = i, Title = t })));
            return ChangeJson(TIs);
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
                .Validate(i => i.CategoryType >= -1, () => AddError(EmptyNotAllowed("分類類別")))
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
            B_Category parent = entity.ParentID.IsValidIdOrZero()
                ? await DC.B_Category.FirstOrDefaultAsync(c => c.BCID == entity.ParentID)
                : null;

            return await Task.FromResult(new Category_GetList_Output_Row_APIItem
            {
                BCID = entity.BCID,
                iCategoryType = entity.CategoryType,
                sCategoryType = entity.CategoryType < sCategoryTypes.Length ? sCategoryTypes[entity.CategoryType] : "",
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
        [ResponsePrivilegeWrapperFilter]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.B_Category.FirstOrDefaultAsync(q => !q.DeleteFlag && q.BCID == ID);
            List<cSelectItem> TIs = new List<cSelectItem>();
            for (int i = 0; i < sCategoryTypes.Length; i++)
                TIs.Add(new cSelectItem
                    { ID = i, Title = sCategoryTypes[i], SelectFlag = (N == null ? i == 0 : i == N.CategoryType) });
            B_Category_APIItem Item = new B_Category_APIItem();
            if (N != null)
            {
                B_Category BC_P = null;
                List<cSelectItem> BC_Ps = null;
                if (N.ParentID > 0)
                {
                    BC_P = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == N.ParentID && !q.DeleteFlag);
                    if (BC_P != null)
                    {
                        BC_Ps = new List<cSelectItem>();
                        var _BC_Ps = DC.B_Category.Where(q => q.ParentID == BC_P.ParentID && !q.DeleteFlag)
                            .OrderBy(q => q.SortNo);
                        foreach (var _BC in await _BC_Ps.ToListAsync())
                            BC_Ps.Add(new cSelectItem
                                { ID = _BC.BCID, Title = _BC.TitleC, SelectFlag = _BC.BCID == BC_P.BCID });
                    }
                }

                Item = new B_Category_APIItem()
                {
                    BCID = N.BCID,
                    iCategoryType = N.CategoryType,
                    sCategoryType = sCategoryTypes[N.CategoryType],
                    CategoryTypeList = TIs,
                    ParentID = N.ParentID,
                    ParentList = BC_Ps,
                    ParentTitleC = (BC_P != null ? BC_P.TitleC : ""),
                    ParentTitleE = (BC_P != null ? BC_P.TitleE : ""),
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    SortNo = N.SortNo,
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }
            else
            {
                Item.CategoryTypeList = TIs;
            }

            return ChangeJson(Item);
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesWithLogAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<B_Category> DeleteItemQuery(int id)
        {
            return DC.B_Category.Where(c => c.BCID == id);
        }

        #endregion

        #region Submit

        private const string SubmitCategoryTypeNotSupported = "不支援此分類類別！";
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
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID == 0, () => AddError(WrongFormat("分類 ID")))
                .Validate(i => i.CategoryType < sCategoryTypes.Length, () => AddError(SubmitCategoryTypeNotSupported))
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
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("分類 ID")))
                .Validate(i => i.CategoryType < sCategoryTypes.Length, () => AddError(SubmitCategoryTypeNotSupported))
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
            data.SortNo = input.CategoryType == data.CategoryType ? data.SortNo : Task.Run(() => GetNewSortNo(input)).Result;
        }

        #endregion

        #endregion
    }
}