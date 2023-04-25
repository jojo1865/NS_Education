using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.BusinessUser.GetInfoById;
using NS_Education.Models.APIItems.BusinessUser.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    public class BusinessUserController : PublicClass,
        IGetListPaged<BusinessUser, BusinessUser_GetList_Input_APIItem, BusinessUser_GetList_Output_Row_APIItem>,
        IGetInfoById<BusinessUser, BusinessUser_GetInfoById_Output_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<BusinessUser_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public BusinessUserController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<BusinessUserController, BusinessUser, BusinessUser_GetList_Input_APIItem,
                    BusinessUser_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<BusinessUserController, BusinessUser, BusinessUser_GetInfoById_Output_APIItem>(
                    this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
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
                Items = entity.M_Customer_BusinessUser.Select(cbu => new BusinessUser_GetList_Output_Customer_APIItem
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
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
    }
}