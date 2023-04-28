using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.FoodCategory;
using NS_Education.Models.APIItems.FoodCategory.GetList;
using NS_Education.Models.APIItems.FoodCategory.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.Legacy
{
    public class FoodCategoryController : PublicClass,
        IGetListPaged<D_FoodCategory, FoodCategory_GetList_Input_APIItem, FoodCategory_GetList_Output_Row_APIItem>,
        IDeleteItem<D_FoodCategory>,
        ISubmit<D_FoodCategory, FoodCategory_Submit_Input_APIItem>
    {
        #region Intialization

        private readonly IGetListPagedHelper<FoodCategory_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<FoodCategory_Submit_Input_APIItem> _submitHelper;

        public FoodCategoryController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<FoodCategoryController, D_FoodCategory, FoodCategory_GetList_Input_APIItem,
                    FoodCategory_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<FoodCategoryController, D_FoodCategory>(this);
            _submitHelper = new SubmitHelper<FoodCategoryController, D_FoodCategory, FoodCategory_Submit_Input_APIItem>(this);
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
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == ID && !q.DeleteFlag);
            D_FoodCategory_APIItem Item = null;
            if (N != null)
            {
                Item = new D_FoodCategory_APIItem
                {
                    DFCID = N.DFCID,
                    Code = N.Code,
                    Title = N.Title,

                    UnitPrice = N.UnitPrice,
                    Price = N.Price,

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
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
            var N_ = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                await DC.SaveChangesStandardProcedureAsync(GetUid());
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

        public IQueryable<D_FoodCategory> DeleteItemQuery(int id)
        {
            return DC.D_FoodCategory.Where(fc => fc.DFCID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(FoodCategory_Submit_Input_APIItem.DFCID))]
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