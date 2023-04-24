using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.PayType;
using NS_Education.Models.APIItems.PayType.GetList;
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
    public class PayTypeController : PublicClass,
        IGetListPaged<D_PayType, PayType_GetList_Input_APIItem, PayType_GetList_Output_APIItem>,
        IDeleteItem<D_PayType>
    {
        #region Initialization

        private IGetListPagedHelper<PayType_GetList_Input_APIItem> _getListPagedHelper;
        private IDeleteItemHelper _deleteItemHelper;

        public PayTypeController()
        {
            _getListPagedHelper = new GetListPagedHelper<PayTypeController, D_PayType, PayType_GetList_Input_APIItem,
                PayType_GetList_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<PayTypeController, D_PayType>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(PayType_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(PayType_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsValidIdOrZero(), () => AddError(EmptyNotAllowed("所屬分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_PayType> GetListPagedOrderedQuery(PayType_GetList_Input_APIItem input)
        {
            var query = DC.D_PayType
                .Include(pt => pt.BC)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(pt => pt.Title.Contains(input.Keyword) || pt.Code.Contains(input.Keyword));

            if (input.BCID > 0)
                query = query.Where(pt => pt.BCID == input.BCID);

            return query.OrderBy(pt => pt.DPTID);
        }

        public async Task<PayType_GetList_Output_APIItem> GetListPagedEntityToRow(D_PayType entity)
        {
            return await Task.FromResult(new PayType_GetList_Output_APIItem
            {
                DPTID = entity.DPTID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC.TitleC ?? "",
                BC_TitleE = entity.BC.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                AccountingNo = entity.AccountingNo,
                CustomerNo = entity.CustormerNo,
                InvoiceFlag = entity.InvoiceFlag,
                DepositFlag = entity.DepositFlag,
                RestaurantFlag = entity.RestaurantFlag,
                SimpleCheckoutFlag = entity.SimpleCheckoutFlag,
                SimpleDepositFlag = entity.DepositFlag
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == ID && !q.DeleteFlag);
            D_PayType_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Cats = DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == 8).OrderBy(q => q.SortNo);
                foreach (var Cat in await Cats.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Cat.BCID, Title = Cat.TitleC, SelectFlag = N.BCID == Cat.BCID });
                Item = new D_PayType_APIItem
                {
                    DPTID = N.DPTID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    BC_List = SIs,
                    Code = N.Code,
                    Title = N.Title,

                    AccountingNo = N.AccountingNo,
                    CustormerNo = N.CustormerNo,
                    InvoiceFlag = N.InvoiceFlag,
                    DepositFlag = N.DepositFlag,
                    RestaurantFlag = N.RestaurantFlag,
                    SimpleCheckoutFlag = N.SimpleCheckoutFlag,
                    SimpleDepositFlag = N.SimpleDepositFlag,

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
            var N_ = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == ID && !q.DeleteFlag);
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

        public IQueryable<D_PayType> DeleteItemQuery(int id)
        {
            return DC.D_PayType.Where(pt => pt.DPTID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_PayType.DPTID))]
        public async Task<string> Submit(D_PayType N)
        {
            Error = "";
            if (N.DPTID == 0)
            {
                if (N.BCID <= 0)
                    Error += "請選擇付款方式所屬分類;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_PayType.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == N.DPTID && !q.DeleteFlag);
                if (N.BCID <= 0)
                    Error += "請選擇付款方式所屬分類;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.BCID = N.BCID;
                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.AccountingNo = N.AccountingNo;
                    N_.CustormerNo = N.CustormerNo;
                    N_.InvoiceFlag = N.InvoiceFlag;
                    N_.DepositFlag = N.DepositFlag;
                    N_.RestaurantFlag = N.RestaurantFlag;
                    N_.SimpleCheckoutFlag = N.SimpleCheckoutFlag;
                    N_.SimpleDepositFlag = N.SimpleDepositFlag;
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