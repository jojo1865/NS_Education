using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.GroupData.GetInfoById;
using NS_Education.Models.APIItems.GroupData.GetList;
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
    public class GroupDataController : PublicClass,
        IGetListPaged<GroupData, GroupData_GetList_Input_APIItem, GroupData_GetList_Output_Row_APIItem>,
        IGetInfoById<GroupData, GroupData_GetInfoById_Output_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<GroupData_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public GroupDataController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<GroupDataController, GroupData, GroupData_GetList_Input_APIItem,
                    GroupData_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<GroupDataController, GroupData, GroupData_GetInfoById_Output_APIItem>(this);
        }

        #endregion
        #region GetList
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(GroupData_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(GroupData_GetList_Input_APIItem input)
        {
            // 此輸入無須驗證
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<GroupData> GetListPagedOrderedQuery(GroupData_GetList_Input_APIItem input)
        {
            var query = DC.GroupData
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(gd => gd.Title.Contains(input.Keyword));

            return query.OrderBy(gd => gd.GID);
        }

        public async Task<GroupData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(GroupData entity)
        {
            return await Task.FromResult(new GroupData_GetList_Output_Row_APIItem
            {
                GID = entity.GID,
                Title = entity.Title ?? ""
            });
        }
        #endregion

        #region GetInfoById
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<GroupData> GetInfoByIdQuery(int id)
        {
            return DC.GroupData
                .Where(gd => gd.GID == id);
        }

        public async Task<GroupData_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(GroupData entity)
        {
            return await Task.FromResult(new GroupData_GetInfoById_Output_APIItem
            {
                GID = entity.GID,
                Title = entity.Title ?? "",
                GroupItems = DC.MenuData
                    .Include(md => md.M_Group_Menu)
                    .Where(md => md.ActiveFlag && !md.DeleteFlag)
                    .AsEnumerable()
                    .Select(md => new { MenuData = md, ThisGroupMenu = md.M_Group_Menu.FirstOrDefault(mgm => mgm.GID == entity.GID)})
                    .Select(result => new GroupData_GetInfoById_Output_MenuItem_APIItem
                    {
                        MDID = result.MenuData.MDID,
                        Title = result.MenuData.Title ?? "",
                        ActiveFlag = result.ThisGroupMenu != null,
                        AddFlag = result.ThisGroupMenu?.AddFlag ?? false,
                        ShowFlag = result.ThisGroupMenu?.ShowFlag ?? false,
                        EditFlag = result.ThisGroupMenu?.EditFlag ?? false,
                        DeleteFlag = result.ThisGroupMenu?.DeleteFlag ?? false,
                        PrintFlag = result.ThisGroupMenu?.PringFlag ?? false
                    })
                    .ToList()
            });
        }
        #endregion
    }
}