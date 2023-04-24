using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.FoodCategory;
using NS_Education.Models.APIItems.FoodCategory.GetList;
using NS_Education.Models.Entities;
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
        IDeleteItem<D_FoodCategory>
    {
        #region Intialization

        private readonly IGetListPagedHelper<FoodCategory_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        public FoodCategoryController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<FoodCategoryController, D_FoodCategory, FoodCategory_GetList_Input_APIItem,
                    FoodCategory_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<FoodCategoryController, D_FoodCategory>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(FoodCategory_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(FoodCategory_GetList_Input_APIItem input)
        {
            // 輸入無須驗證
            return true;
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_FoodCategory.DFCID))]
        public async Task<string> Submit(D_FoodCategory N)
        {
            Error = "";
            if (N.DFCID == 0)
            {
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.UnitPrice < 0)
                    Error += "請輸入成本的數字;";
                if (N.Price < 0)
                    Error += "請輸入價格的數字;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_FoodCategory.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == N.DFCID && !q.DeleteFlag);
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.UnitPrice < 0)
                    Error += "請輸入成本的數字;";
                if (N.Price < 0)
                    Error += "請輸入價格的數字;";

                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {

                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.UnitPrice = N.UnitPrice;
                    N_.Price = N.Price;

                    N_.ActiveFlag = N.ActiveFlag;
                    N_.DeleteFlag = N.DeleteFlag;
                    N_.UpdUID = GetUid();
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion

    }
}