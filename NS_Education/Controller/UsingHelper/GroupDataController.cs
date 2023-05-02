using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.GroupData;
using NS_Education.Models.APIItems.GroupData.GetInfoById;
using NS_Education.Models.APIItems.GroupData.GetList;
using NS_Education.Models.APIItems.GroupData.Submit;
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
    public class GroupDataController : PublicClass,
        IGetListPaged<GroupData, GroupData_GetList_Input_APIItem, GroupData_GetList_Output_Row_APIItem>,
        IGetInfoById<GroupData, GroupData_GetInfoById_Output_APIItem>,
        IDeleteItem<GroupData>,
        ISubmit<GroupData, GroupData_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<GroupData_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<GroupData_Submit_Input_APIItem> _submitHelper;

        public GroupDataController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<GroupDataController, GroupData, GroupData_GetList_Input_APIItem,
                    GroupData_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<GroupDataController, GroupData, GroupData_GetInfoById_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<GroupDataController, GroupData>(this);
            _submitHelper = new SubmitHelper<GroupDataController, GroupData, GroupData_Submit_Input_APIItem>(this);
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
                    .Select(md => new
                        { MenuData = md, ThisGroupMenu = md.M_Group_Menu.FirstOrDefault(mgm => mgm.GID == entity.GID) })
                    .Select(result => new GroupData_MenuItem_APIItem
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

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<GroupData> DeleteItemQuery(int id)
        {
            return DC.GroupData.Where(gd => gd.GID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.AddOrEdit, null, nameof(GroupData_Submit_Input_APIItem.GID))]
        public async Task<string> Submit(GroupData_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(GroupData_Submit_Input_APIItem input)
        {
            return input.GID == 0;
        }
        
        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(GroupData_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.GID == 0, () => AddError(WrongFormat("權限 ID")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("權限名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<GroupData> SubmitCreateData(GroupData_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new GroupData
            {
                Title = input.Title
            });
        }
        
        #endregion

        #region Submit - Edit
        public async Task<bool> SubmitEditValidateInput(GroupData_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.GID.IsAboveZero(), () => AddError(EmptyNotAllowed("權限 ID")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("權限名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<GroupData> SubmitEditQuery(GroupData_Submit_Input_APIItem input)
        {
            return DC.GroupData.Where(gd => gd.GID == input.GID);
        }

        public void SubmitEditUpdateDataFields(GroupData data, GroupData_Submit_Input_APIItem input)
        {
            data.Title = input.Title;
        }
        
        #endregion
        #endregion
    }
}