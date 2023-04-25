using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.PartnerItem.GetInfoById;
using NS_Education.Models.APIItems.PartnerItem.GetList;
using NS_Education.Models.APIItems.PartnerItem.Submit;
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
    public class PartnerItemController : PublicClass,
        IGetListPaged<B_PartnerItem, PartnerItem_GetList_Input_APIItem, PartnerItem_GetList_Output_Row_APIItem>,
        IGetInfoById<B_PartnerItem, PartnerItem_GetInfoById_Output_APIItem>,
        IDeleteItem<B_PartnerItem>,
        IChangeActive<B_PartnerItem>,
        ISubmit<B_PartnerItem, PartnerItem_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<PartnerItem_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly ISubmitHelper<PartnerItem_Submit_Input_APIItem> _submitHelper;

        public PartnerItemController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<PartnerItemController, B_PartnerItem, PartnerItem_GetList_Input_APIItem,
                    PartnerItem_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<PartnerItemController, B_PartnerItem, PartnerItem_GetInfoById_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<PartnerItemController, B_PartnerItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<PartnerItemController, B_PartnerItem>(this);

            _submitHelper = new SubmitHelper<PartnerItemController, B_PartnerItem, PartnerItem_Submit_Input_APIItem>(this);
        }

        #endregion
        
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(PartnerItem_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(PartnerItem_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BPID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選的合作廠商 ID")))
                .Validate(i => i.BSCID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選的所屬房型類別 ID")))
                .Validate(i => i.DHID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選的所屬廳別 ID")))
                .Validate(i => i.BOCID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選的所屬入帳代號 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_PartnerItem> GetListPagedOrderedQuery(PartnerItem_GetList_Input_APIItem input)
        {
            var query = DC.B_PartnerItem
                .Include(pi => pi.BP)
                .Include(pi => pi.BSC)
                .Include(pi => pi.BOC)
                .Include(pi => pi.DH)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(pi
                    => pi.BP.Title.Contains(input.Keyword)
                       || pi.BSC.Title.Contains(input.Keyword)
                       || pi.BOC.Title.Contains(input.Keyword)
                       || pi.DH.TitleC.Contains(input.Keyword)
                       || pi.DH.TitleE.Contains(input.Keyword)
                       || pi.BP.Code.Contains(input.Keyword)
                       || pi.BSC.Code.Contains(input.Keyword)
                       || pi.BOC.Code.Contains(input.Keyword)
                       || pi.DH.Code.Contains(input.Keyword));

            if (input.BPID.IsValidId())
                query = query.Where(pi => pi.BPID == input.BPID);

            if (input.BSCID.IsValidId())
                query = query.Where(pi => pi.BSCID == input.BSCID);

            if (input.DHID.IsValidId())
                query = query.Where(pi => pi.DHID == input.DHID);

            if (input.BOCID.IsValidId())
                query = query.Where(pi => pi.BOCID == input.BOCID);

            return query.OrderBy(pi => pi.SortNo)
                .ThenBy(pi => pi.BPIID);
        }

        public async Task<PartnerItem_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_PartnerItem entity)
        {
            return await Task.FromResult(new PartnerItem_GetList_Output_Row_APIItem
            {
                BPIID = entity.BPIID,
                BPID = entity.BPID,
                BP_Title = entity.BP?.Title ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                BOCID = entity.BOCID,
                BOC_Title = entity.BOC?.Title ?? "",
                DHID = entity.DHID,
                DH_Title = entity.DH?.TitleC ?? entity.DH?.TitleE ?? "",
                Ct = entity.Ct,
                Price = entity.Price,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                SortNo = entity.SortNo,
                Note = entity.Note ?? ""
            });
        }
        #endregion

        #region GetInfoById
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public async Task<bool> GetInfoByIdValidateInput(int id)
        {
            bool isValid = id.StartValidate()
                .Validate(i => i.IsValidId(), () => AddError(EmptyNotAllowed("房型 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_PartnerItem> GetInfoByIdQuery(int id)
        {
            return DC.B_PartnerItem
                .Include(pi => pi.BP)
                .Include(pi => pi.BSC)
                .Include(pi => pi.BOC)
                .Include(pi => pi.DH)
                .Where(pi => pi.BPIID == id);
        }

        public async Task<PartnerItem_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_PartnerItem entity)
        {
            return new PartnerItem_GetInfoById_Output_APIItem
            {
                BPIID = entity.BPIID,
                BPID = entity.BPID,
                BP_Title = entity.BP?.Title ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSC?.CodeType, entity.BSCID),
                BOCID = entity.BOCID,
                BOC_Title = entity.BOC?.Title ?? "",
                BOC_List = await DC.B_OrderCode.GetOrderCodeSelectable(entity.BOC?.CodeType, entity.BOCID),
                DHID = entity.DHID,
                DH_Title = entity.DH?.TitleC ?? entity.DH?.TitleE ?? "",
                DH_List = await DC.D_Hall.GetHallSelectable(entity.DHID),
                Ct = entity.Ct,
                Price = entity.Price,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                SortNo = entity.SortNo,
                Note = entity.Note ?? ""
            };
        }
        #endregion

        #region DeleteItem
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<B_PartnerItem> DeleteItemQuery(int id)
        {
            return DC.B_PartnerItem.Where(pi => pi.BPIID == id);
        }
        #endregion

        #region ChangeActive
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<B_PartnerItem> ChangeActiveQuery(int id)
        {
            return DC.B_PartnerItem.Where(pi => pi.BPIID == id);
        }
        #endregion
        
        #region Submit

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(PartnerItem_Submit_Input_APIItem.BPIID))]
        public async Task<string> Submit(PartnerItem_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(PartnerItem_Submit_Input_APIItem input)
        {
            return input.BPIID == 0;
        }

        #region Submit - Add
        
        public async Task<bool> SubmitAddValidateInput(PartnerItem_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BPIID == 0, () => AddError(WrongFormat("房型 ID")))
                .Validate(i => i.BPID.IsValidId(), () => AddError(EmptyNotAllowed("廠商 ID")))
                .Validate(i => i.BSCID.IsValidId(), () => AddError(EmptyNotAllowed("房型類型 ID")))
                .Validate(i => i.BOCID.IsValidId(), () => AddError(EmptyNotAllowed("入帳代號 ID")))
                .Validate(i => i.DHID.IsValidId(), () => AddError(EmptyNotAllowed("廳別 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<B_PartnerItem> SubmitCreateData(PartnerItem_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new B_PartnerItem
            {
                BPID = input.BPID,
                BSCID = input.BSCID,
                BOCID = input.BOCID,
                DHID = input.DHID,
                Ct = input.Ct,
                Price = input.Price,
                UnitPrice = input.UnitPrice,
                InPrice = input.InPrice,
                OutPrice = input.OutPrice,
                SortNo = input.SortNo,
                Note = input.Note
            });
        }
        
        #endregion

        #region Submit - Edit
        
        public async Task<bool> SubmitEditValidateInput(PartnerItem_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BPIID.IsValidId(), () => AddError(EmptyNotAllowed("房型 ID")))
                .Validate(i => i.BPID.IsValidId(), () => AddError(EmptyNotAllowed("廠商 ID")))
                .Validate(i => i.BSCID.IsValidId(), () => AddError(EmptyNotAllowed("房型類型 ID")))
                .Validate(i => i.BOCID.IsValidId(), () => AddError(EmptyNotAllowed("入帳代號 ID")))
                .Validate(i => i.DHID.IsValidId(), () => AddError(EmptyNotAllowed("廳別 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_PartnerItem> SubmitEditQuery(PartnerItem_Submit_Input_APIItem input)
        {
            return DC.B_PartnerItem.Where(pi => pi.BPIID == input.BPIID);
        }

        public void SubmitEditUpdateDataFields(B_PartnerItem data, PartnerItem_Submit_Input_APIItem input)
        {
            data.BPID = input.BPID;
            data.BSCID = input.BSCID;
            data.BOCID = input.BOCID;
            data.DHID = input.DHID;
            data.Ct = input.Ct;
            data.Price = input.Price;
            data.UnitPrice = input.UnitPrice;
            data.InPrice = input.InPrice;
            data.OutPrice = input.OutPrice;
            data.SortNo = input.SortNo;
            data.Note = input.Note;
        }
        
        #endregion
        
        #endregion
    }
}