using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.BusinessUser.GetInfoById;
using NS_Education.Models.APIItems.BusinessUser.GetList;
using NS_Education.Models.APIItems.BusinessUser.Submit;
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
                .ThenInclude(cbu => cbu.C)
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
                    Code = cbu.C?.Code ?? "",
                    TitleC = cbu.C?.TitleC ?? "",
                    TitleE = cbu.C?.TitleE ?? ""
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
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<BusinessUser> DeleteItemQuery(int id)
        {
            return DC.BusinessUser.Where(bu => bu.BUID == id);
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
            bool isValid = input.StartValidate()
                .Validate(i => i.BUID == 0, () => AddError(WrongFormat("業務 ID")))
                .IsValid();

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
                    SortNo = index + 1,
                    ActiveFlag = true,
                    DeleteFlag = false,
                    CreDate = DateTime.Now,
                    CreUID = GetUid(),
                    UpdDate = DateTime.Now,
                    UpdUID = 0
                }).ToList()
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(BusinessUser_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BUID.IsAboveZero(), () => AddError(EmptyNotAllowed("業務 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<BusinessUser> SubmitEditQuery(BusinessUser_Submit_Input_APIItem input)
        {
            return DC.BusinessUser
                .Include(bu => bu.M_Customer_BusinessUser)
                .Where(bu => bu.BUID == input.BUID);
        }

        public void SubmitEditUpdateDataFields(BusinessUser data, BusinessUser_Submit_Input_APIItem input)
        {
            // 1. 刪除所有原本的 M_Customer_BusinessUser 資料
            foreach (M_Customer_BusinessUser cbu in DC.M_Customer_BusinessUser.Where(cbu => cbu.BUID == data.BUID && cbu.ActiveFlag && !cbu.DeleteFlag).ToList())
            {
                cbu.DeleteFlag = true;
                cbu.UpdDate = DateTime.Now;
                cbu.UpdUID = GetUid();
            }

            DC.SaveChangesWithLog();
            
            // 2. 修改資料
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
                ActiveFlag = true,
                DeleteFlag = false,
                CreDate = DateTime.Now,
                CreUID = GetUid(),
                UpdDate = DateTime.Now,
                UpdUID = 0
            }).ToList();
        }

        #endregion

        #endregion
    }
}