using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.OtherPayItem;
using NS_Education.Models.APIItems.OtherPayItem.GetList;
using NS_Education.Models.APIItems.OtherPayItem.Submit;
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
    public class OtherPayItemController : PublicClass,
        IGetListPaged<D_OtherPayItem, OtherPayItem_GetList_Input_APIItem, OtherPayItem_GetList_Output_Row_APIItem>,
        IDeleteItem<D_OtherPayItem>,
        ISubmit<D_OtherPayItem, OtherPayItem_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<OtherPayItem_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<OtherPayItem_Submit_Input_APIItem> _submitHelper;

        public OtherPayItemController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<OtherPayItemController, D_OtherPayItem, OtherPayItem_GetList_Input_APIItem,
                    OtherPayItem_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new DeleteItemHelper<OtherPayItemController, D_OtherPayItem>(this);
            _submitHelper =
                new SubmitHelper<OtherPayItemController, D_OtherPayItem, OtherPayItem_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(OtherPayItem_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(OtherPayItem_GetList_Input_APIItem input)
        {
            // 此功能無須驗證輸入
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<D_OtherPayItem> GetListPagedOrderedQuery(OtherPayItem_GetList_Input_APIItem input)
        {
            var query = DC.D_OtherPayItem.AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(opi => opi.Title.Contains(input.Keyword) || opi.Code.Contains(input.Keyword));

            return query.OrderBy(opi => opi.Code)
                .ThenBy(opi => opi.DOPIID);
        }

        public async Task<OtherPayItem_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_OtherPayItem entity)
        {
            return await Task.FromResult(new OtherPayItem_GetList_Output_Row_APIItem
            {
                DOPIID = entity.DOPIID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                PaidType = entity.PaidType,
                BSCID = entity.BSCID,
                BOCID = entity.BOCID
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_OtherPayItem.FirstOrDefaultAsync(q => q.DOPIID == ID && !q.DeleteFlag);
            D_OtherPayItem_APIItem Item = null;
            if (N != null)
            {
                Item = new D_OtherPayItem_APIItem
                {
                    DOPIID = N.DOPIID,
                    Code = N.Code,
                    Title = N.Title,

                    Ct = N.Ct,
                    UnitPrice = N.UnitPrice,
                    InPrice = N.InPrice,
                    OutPrice = N.OutPrice,
                    PaidType = N.PaidType,
                    BSCID = N.BSCID,
                    BOCID = N.BOCID,

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate
                        ? N.UpdDate.ToString(DateTimeFormat)
                        : ""),
                    UpdUser = (N.CreDate != N.UpdDate
                        ? await GetUserNameByID(N.UpdUID)
                        : ""),
                    UpdUID = (N.CreDate != N.UpdDate
                        ? N.UpdUID
                        : 0),

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
            var N_ = await DC.D_OtherPayItem.FirstOrDefaultAsync(q => q.DOPIID == ID && !q.DeleteFlag);
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_OtherPayItem> DeleteItemQuery(int id)
        {
            return DC.D_OtherPayItem.Where(opi => opi.DOPIID == id);
        }

        #endregion

        #region Submit

        private HashSet<string> SubmitAllowedBSCID = new HashSet<string>();
        private const string SubmitBSCIDNotSupported = "不支援此單位 ID！";
        
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null,
            nameof(OtherPayItem_Submit_Input_APIItem.DOPIID))]
        public async Task<string> Submit(OtherPayItem_Submit_Input_APIItem input)
        {
            // 如果還沒有 BSCID 的資料，撈一次
            SubmitPopulateBSCIDSet();

            return await _submitHelper.Submit(input);
        }

        private void SubmitPopulateBSCIDSet()
        {
            if (SubmitAllowedBSCID?.Any() ?? true)
            {
                SubmitAllowedBSCID = DC.B_StaticCode
                    .Where(sc => sc.CodeType == 2 && sc.ActiveFlag && !sc.DeleteFlag)
                    .Select(sc => sc.Code)
                    .ToHashSet();
            }
        }

        public bool SubmitIsAdd(OtherPayItem_Submit_Input_APIItem input)
        {
            return input.DOPIID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(OtherPayItem_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DOPIID == 0, () => AddError(WrongFormat("項目 ID")))
                .Validate(i => i.PaidType.IsInBetween(0, 1), () => AddError(WrongFormat("計價方式")))
                .Validate(i => SubmitAllowedBSCID.Contains(i.Code), () => AddError(SubmitBSCIDNotSupported))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_OtherPayItem> SubmitCreateData(OtherPayItem_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_OtherPayItem
            {
                DOPIID = 0,
                Code = input.Code,
                Title = input.Title,
                Ct = input.Ct,
                UnitPrice = input.UnitPrice,
                InPrice = input.InPrice,
                OutPrice = input.OutPrice,
                PaidType = input.PaidType,
                BSCID = input.BSCID,
                BOCID = input.BOCID
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(OtherPayItem_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DOPIID.IsValidId(), () => AddError(EmptyNotAllowed("項目 ID")))
                .Validate(i => i.PaidType.IsInBetween(0, 1), () => AddError(WrongFormat("計價方式")))
                .Validate(i => SubmitAllowedBSCID.Contains(i.Code), () => AddError(SubmitBSCIDNotSupported))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_OtherPayItem> SubmitEditQuery(OtherPayItem_Submit_Input_APIItem input)
        {
            return DC.D_OtherPayItem.Where(opi => opi.DOPIID == input.DOPIID);
        }

        public void SubmitEditUpdateDataFields(D_OtherPayItem data, OtherPayItem_Submit_Input_APIItem input)
        {
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.Ct = input.Ct;
            data.UnitPrice = input.UnitPrice;
            data.InPrice = input.InPrice;
            data.OutPrice = input.OutPrice;
            data.PaidType = input.PaidType;
            data.BSCID = input.BSCID;
            data.BOCID = input.BOCID;
        }

        #endregion

        #endregion
    }
}