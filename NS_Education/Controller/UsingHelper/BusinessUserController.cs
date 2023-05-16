using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.BusinessUser.GetInfoById;
using NS_Education.Models.APIItems.Controller.BusinessUser.GetList;
using NS_Education.Models.APIItems.Controller.BusinessUser.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
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
    public class BusinessUserController : PublicClass,
        IGetListPaged<BusinessUser, BusinessUser_GetList_Input_APIItem, BusinessUser_GetList_Output_Row_APIItem>,
        IGetInfoById<BusinessUser, BusinessUser_GetInfoById_Output_APIItem>,
        IDeleteItem<BusinessUser>,
        IChangeActive<BusinessUser>,
        ISubmit<BusinessUser, BusinessUser_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<BusinessUser_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly ISubmitHelper<BusinessUser_Submit_Input_APIItem> _submitHelper;

        public BusinessUserController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<BusinessUserController, BusinessUser, BusinessUser_GetList_Input_APIItem,
                    BusinessUser_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<BusinessUserController, BusinessUser, BusinessUser_GetInfoById_Output_APIItem>(
                    this);

            _deleteItemHelper =
                new DeleteItemHelper<BusinessUserController, BusinessUser>(this);

            _changeActiveHelper =
                new ChangeActiveHelper<BusinessUserController, BusinessUser>(this);

            _submitHelper =
                new SubmitHelper<BusinessUserController, BusinessUser, BusinessUser_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(BusinessUser_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(BusinessUser_GetList_Input_APIItem input)
        {
            // 此功能不需驗證輸入
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<BusinessUser> GetListPagedOrderedQuery(BusinessUser_GetList_Input_APIItem input)
        {
            var query = DC.BusinessUser
                .Include(bu => bu.M_Customer_BusinessUser)
                .Include(bu => bu.M_Customer_BusinessUser.Select(cbu => cbu.Customer))
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(bu => bu.Name.Contains(input.Keyword) || bu.Code.Contains(input.Keyword));

            if (input.MKType.IsInBetween(0, 1))
                query = query.Where(bu => bu.MKsalesFlag == (input.MKType == 1));

            if (input.OPType.IsInBetween(0, 1))
                query = query.Where(bu => bu.OPsalesFlag == (input.OPType == 1));

            return query.OrderBy(bu => bu.Code)
                .ThenBy(bu => bu.Name)
                .ThenBy(bu => bu.BUID);
        }

        public async Task<BusinessUser_GetList_Output_Row_APIItem> GetListPagedEntityToRow(BusinessUser entity)
        {
            return await Task.FromResult(new BusinessUser_GetList_Output_Row_APIItem
            {
                BUID = entity.BUID,
                Code = entity.Code ?? "",
                Name = entity.Name ?? "",
                Phone = entity.Phone ?? "",
                MKsalesFlag = entity.MKsalesFlag,
                OPsalesFlag = entity.OPsalesFlag,
                Items = entity.M_Customer_BusinessUser.Select(cbu => new BusinessUser_GetList_Customer_APIItem
                {
                    CID = cbu.CID,
                    Code = cbu.Customer?.Code ?? "",
                    TitleC = cbu.Customer?.TitleC ?? "",
                    TitleE = cbu.Customer?.TitleE ?? ""
                }).ToList()
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

        public IQueryable<BusinessUser> GetInfoByIdQuery(int id)
        {
            return DC.BusinessUser
                .Include(bu => bu.M_Customer_BusinessUser)
                .Where(bu => bu.BUID == id);
        }

        public async Task<BusinessUser_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            BusinessUser entity)
        {
            return await Task.FromResult(new BusinessUser_GetInfoById_Output_APIItem
                {
                    BUID = entity.BUID,
                    Code = entity.Code ?? "",
                    Name = entity.Name ?? "",
                    Phone = entity.Phone ?? "",
                    MKsalesFlag = entity.MKsalesFlag,
                    OPsalesFlag = entity.OPsalesFlag,
                    CustomerCt = entity.M_Customer_BusinessUser.Count(cbu => cbu.ActiveFlag && !cbu.DeleteFlag)
                }
            );
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<BusinessUser> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.BusinessUser.Where(bu => ids.Contains(bu.BUID));
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<BusinessUser> ChangeActiveQuery(int id)
        {
            return DC.BusinessUser.Where(bu => bu.BUID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(BusinessUser_Submit_Input_APIItem.BUID))]
        public async Task<string> Submit(BusinessUser_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(BusinessUser_Submit_Input_APIItem input)
        {
            return input.BUID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(BusinessUser_Submit_Input_APIItem input)
        {
            bool isInputValid = input.StartValidate()
                .Validate(i => i.BUID == 0, () => AddError(WrongFormat("業務 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("員工編號")))
                .Validate(i => i.Name.HasContent(), () => AddError(EmptyNotAllowed("姓名")))
                .Validate(i => !i.Items.Any() || i.Items.GroupBy(item => item.CID).Count() == input.Items.Count,
                    () => AddError(CopyNotAllowed("負責客戶列表", "客戶 ID")))
                .IsValid();

            // 驗證所有 CID 都實際存在於資料庫。
            bool isValid = isInputValid &&
                           input.Items.Aggregate(true, (result, item) =>
                               result & item.StartValidate()
                                   .Validate(_ => 
                                           DC.Customer.Any(c => c.ActiveFlag && !c.DeleteFlag && c.CID == item.CID),
                                       () => AddError(NotFound($"客戶 ID {item.CID}")))
                                   .IsValid()
                           );

            return await Task.FromResult(isValid);
        }

        public async Task<BusinessUser> SubmitCreateData(BusinessUser_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new BusinessUser
            {
                Code = input.Code,
                Name = input.Name,
                Phone = input.Phone,
                MKsalesFlag = input.MKsalesFlag,
                OPsalesFlag = input.OPsalesFlag,
                M_Customer_BusinessUser = input.Items.Select((item, index) => new M_Customer_BusinessUser
                {
                    CID = item.CID,
                    MappingType = input.OPsalesFlag ? DbConstants.OpSalesMappingType : input.MKsalesFlag ? DbConstants.MkSalesMappingType : 0,
                    SortNo = index + 1
                }).ToList()
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(BusinessUser_Submit_Input_APIItem input)
        {
            bool isInputValid = input.StartValidate()
                .Validate(i => i.BUID.IsAboveZero(), () => AddError(EmptyNotAllowed("業務 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("員工編號")))
                .Validate(i => i.Name.HasContent(), () => AddError(EmptyNotAllowed("姓名")))
                .Validate(i => !i.Items.Any() || i.Items.GroupBy(item => item.CID).Count() == input.Items.Count,
                    () => AddError(CopyNotAllowed("負責客戶列表", "客戶 ID")))
                .IsValid();

            // 驗證所有 CID 都實際存在於資料庫。
            bool isValid = isInputValid &&
                           input.Items.Aggregate(true, (result, item) =>
                               result & item.StartValidate()
                                   .Validate(_ => 
                                           DC.Customer.Any(c => c.ActiveFlag && !c.DeleteFlag && c.CID == item.CID),
                                       () => AddError(NotFound($"客戶 ID {item.CID}")))
                                   .IsValid()
                           );

            return await Task.FromResult(isValid);
        }

        public IQueryable<BusinessUser> SubmitEditQuery(BusinessUser_Submit_Input_APIItem input)
        {
            return DC.BusinessUser
                .Where(bu => bu.BUID == input.BUID);
        }

        public void SubmitEditUpdateDataFields(BusinessUser data, BusinessUser_Submit_Input_APIItem input)
        {
            // 先刪除所有舊有的 M_Customer_BusinessUser
            DC.M_Customer_BusinessUser.RemoveRange(DC.M_Customer_BusinessUser.Where(cbu => cbu.ActiveFlag && !cbu.DeleteFlag && cbu.CID == data.BUID));
            
            data.Code = input.Code;
            data.Name = input.Name;
            data.Phone = input.Phone;
            data.MKsalesFlag = input.MKsalesFlag;
            data.OPsalesFlag = input.OPsalesFlag;
            data.M_Customer_BusinessUser = input.Items.Select((item, index) => new M_Customer_BusinessUser
            {
                CID = item.CID,
                MappingType = input.OPsalesFlag ? DbConstants.OpSalesMappingType :
                    input.MKsalesFlag ? DbConstants.MkSalesMappingType : 0,
                SortNo = index + 1,
                ActiveFlag = true
            }).ToList();
        }

        #endregion

        #endregion
    }
}