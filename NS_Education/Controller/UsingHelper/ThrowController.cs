using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Throw.GetInfoById;
using NS_Education.Models.APIItems.Controller.Throw.GetList;
using NS_Education.Models.APIItems.Controller.Throw.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper
{
    /// <summary>
    /// 掌管行程的控制器。
    /// </summary>
    public class ThrowController : PublicClass,
        IGetListPaged<D_Throw, Throw_GetList_Input_APIItem, Throw_GetList_Output_Row_APIItem>,
        IGetInfoById<D_Throw, Throw_GetInfoById_Output_APIItem>,
        IChangeActive<D_Throw>,
        IDeleteItem<D_Throw>,
        ISubmit<D_Throw, Throw_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Throw_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<Throw_Submit_Input_APIItem> _submitHelper;

        public ThrowController()
        {
            _getListPagedHelper = new GetListPagedHelper<ThrowController, D_Throw, Throw_GetList_Input_APIItem,
                Throw_GetList_Output_Row_APIItem>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<ThrowController, D_Throw, Throw_GetInfoById_Output_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<ThrowController, D_Throw>(this);
            _deleteItemHelper = new DeleteItemHelper<ThrowController, D_Throw>(this);
            _submitHelper = new SubmitHelper<ThrowController, D_Throw, Throw_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Throw_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public Task<bool> GetListPagedValidateInput(Throw_GetList_Input_APIItem input)
        {
            // 沒有需要驗證的輸入。
            return Task.FromResult(true);
        }

        public IOrderedQueryable<D_Throw> GetListPagedOrderedQuery(Throw_GetList_Input_APIItem input)
        {
            var query = DC.D_Throw
                .Include(dt => dt.B_OrderCode)
                .Include(dt => dt.B_StaticCode)
                .AsQueryable();

            if (input.Keyword.HasContent())
                query = query.Where(dt => dt.Title.Contains(input.Keyword));

            return query.OrderBy(dt => dt.BSCID)
                .ThenBy(dt => dt.DTID);
        }

        public Task<Throw_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Throw entity)
        {
            return Task.FromResult(new Throw_GetList_Output_Row_APIItem
            {
                DTID = entity.DTID,
                BOC_Title = entity.B_OrderCode.Title ?? "",
                BSC_Title = entity.B_StaticCode.Title ?? "",
                Title = entity.Title ?? "",
                UnitPrice = entity.UnitPrice,
                FixedPrice = entity.FixedPrice
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

        public IQueryable<D_Throw> GetInfoByIdQuery(int id)
        {
            return DC.D_Throw
                .Include(dt => dt.B_OrderCode)
                .Include(dt => dt.B_StaticCode)
                .Where(dt => dt.DTID == id);
        }

        public Task<Throw_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_Throw entity)
        {
            return Task.FromResult(new Throw_GetInfoById_Output_APIItem
            {
                DTID = entity.DTID,
                BOCID = entity.BOCID,
                BOC_Title = entity.B_OrderCode.Title ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode.Title ?? "",
                Title = entity.Title ?? "",
                UnitPrice = entity.UnitPrice,
                FixedPrice = entity.FixedPrice,
                Remark = entity.Remark ?? ""
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

        public IQueryable<D_Throw> ChangeActiveQuery(int id)
        {
            return DC.D_Throw.Where(dt => dt.DTID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<D_Throw> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.D_Throw.Where(dt => ids.Contains(dt.DTID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Throw_Submit_Input_APIItem.DTID))]
        public async Task<string> Submit(Throw_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Throw_Submit_Input_APIItem input)
        {
            return input.DTID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Throw_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DTID == 0,
                    i => ExpectedValue("行程 ID", nameof(i.DTID), 0))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Throw),
                    i => NotFound("入帳代號", nameof(i.BOCID)))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.ResverThrow),
                    i => NotFound("類別", nameof(i.BSCID)))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    i => LengthOutOfRange("行程名稱", nameof(i.Title), 1, 60))
                .Validate(i => i.UnitPrice.IsZeroOrAbove(),
                    i => OutOfRange("單位成本", nameof(i.UnitPrice), 0))
                .Validate(i => i.FixedPrice.IsZeroOrAbove(),
                    i => OutOfRange("定價", nameof(i.FixedPrice), 0))
                .IsValid();

            return isValid;
        }

        public Task<D_Throw> SubmitCreateData(Throw_Submit_Input_APIItem input)
        {
            return Task.FromResult(new D_Throw
            {
                DTID = input.DTID,
                BOCID = input.BOCID,
                BSCID = input.BSCID,
                Title = input.Title,
                UnitPrice = input.UnitPrice,
                FixedPrice = input.FixedPrice,
                Remark = input.Remark
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Throw_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DTID.IsAboveZero(),
                    i => OutOfRange("行程 ID", nameof(i.DTID), 0))
                .ValidateAsync(async i => await DC.B_OrderCode.ValidateOrderCodeExists(i.BOCID, OrderCodeType.Throw),
                    i => NotFound("入帳代號", nameof(i.BOCID)))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.ResverThrow),
                    i => NotFound("類別", nameof(i.BSCID)))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    i => LengthOutOfRange("行程名稱", nameof(i.Title), 1, 60))
                .Validate(i => i.UnitPrice.IsZeroOrAbove(),
                    i => OutOfRange("單位成本", nameof(i.UnitPrice), 0))
                .Validate(i => i.FixedPrice.IsZeroOrAbove(),
                    i => OutOfRange("定價", nameof(i.FixedPrice), 0))
                .IsValid();

            return isValid;
        }

        public IQueryable<D_Throw> SubmitEditQuery(Throw_Submit_Input_APIItem input)
        {
            return DC.D_Throw.Where(dt => dt.DTID == input.DTID);
        }

        public void SubmitEditUpdateDataFields(D_Throw data, Throw_Submit_Input_APIItem input)
        {
            data.DTID = input.DTID;
            data.BOCID = input.BOCID;
            data.BSCID = input.BSCID;
            data.Title = input.Title ?? data.Title;
            data.UnitPrice = input.UnitPrice;
            data.FixedPrice = input.FixedPrice;
            data.Remark = input.Remark ?? data.Remark;
        }

        #endregion

        #endregion
    }
}