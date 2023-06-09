using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.GroupData;
using NS_Education.Models.APIItems.Controller.GroupData.GetInfoById;
using NS_Education.Models.APIItems.Controller.GroupData.GetList;
using NS_Education.Models.APIItems.Controller.GroupData.Submit;
using NS_Education.Models.APIItems.Controller.GroupData.SubmitMenuData;
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
                    {
                        MenuData = md,
                        ThisGroupMenu = md.M_Group_Menu.FirstOrDefault(mgm => mgm.GID == entity.GID),
                        IsSpecialMenu = DbConstants.AlwaysShowAddEditMenuUrls.Contains(md.URL)
                    })
                    .Select(result => new GroupData_MenuItem_APIItem
                    {
                        MDID = result.MenuData.MDID,
                        Title = result.MenuData.Title ?? "",
                        ActiveFlag = result.IsSpecialMenu || result.ThisGroupMenu != null,
                        AddFlag = result.IsSpecialMenu || (result.ThisGroupMenu?.AddFlag ?? false),
                        ShowFlag = result.IsSpecialMenu || (result.ThisGroupMenu?.ShowFlag ?? false),
                        EditFlag = result.IsSpecialMenu || (result.ThisGroupMenu?.EditFlag ?? false),
                        DeleteFlag = result.ThisGroupMenu?.DeleteFlag ?? false,
                        PrintFlag = result.ThisGroupMenu?.PringFlag ?? false,
                        AddFlagReadOnly = result.IsSpecialMenu,
                        ShowFlagReadOnly = result.IsSpecialMenu,
                        EditFlagReadOnly = result.IsSpecialMenu,
                        DeleteFlagReadOnly = false,
                        PrintFlagReadOnly = false
                    })
                    .ToList()
            });
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            if (!await DeleteItemValidateReviveNoSameNameData(input))
            {
                return GetResponseJson();
            }

            await _deleteItemHelper.DeleteItem(input);

            return GetResponseJson();
        }

        private async Task<bool> DeleteItemValidateReviveNoSameNameData(DeleteItem_Input_APIItem input)
        {
            // 1. 查詢所有要復活的資料
            IEnumerable<DeleteItem_Input_Row_APIItem> toRevive = input.Items.Where(i => i.DeleteFlag == false);
            IEnumerable<int> toReviveIds = toRevive.Where(r => r.Id != null).Select(r => r.Id.Value);

            GroupData[] toReviveData = await DC.GroupData.Where(gd => gd.DeleteFlag)
                .Where(gd => toReviveIds.Contains(gd.GID))
                .ToArrayAsync();

            string[] toReviveNames = toReviveData.Select(rd => rd.Title).ToArray();

            foreach (KeyValuePair<string, int> kvp in toReviveNames.GroupBy(s => s)
                         .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                         .Where(g => g.Value > 1))
            {
                AddError(CopyNotAllowed($"權限名稱（{kvp.Key}）"));
            }

            if (HasError())
                return false;

            string[] aliveSameNames = await DC.GroupData.Where(gd => !gd.DeleteFlag)
                .Where(gd => !toReviveIds.Contains(gd.GID))
                .Where(gd => toReviveNames.Contains(gd.Title))
                .Select(gd => gd.Title)
                .ToArrayAsync();

            foreach (string aliveSameName in aliveSameNames)
            {
                AddError(CopyNotAllowed($"權限名稱（{aliveSameName}）"));
            }

            return !HasError();
        }

        public IQueryable<GroupData> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.GroupData.Where(gd => ids.Contains(gd.GID));
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
            bool isValid = await input.StartValidate()
                .Validate(i => i.GID == 0, () => AddError(WrongFormat("權限 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("權限名稱")))
                .Validate(i => i.Title.HasLengthBetween(1, 50), () => AddError(LengthOutOfRange("權限名稱", 1, 50)))
                .SkipIfAlreadyInvalid()
                .ValidateAsync(
                    async i => !await DC.GroupData.AnyAsync(gd =>
                        !gd.DeleteFlag && gd.Title == i.Title && gd.GID != i.GID),
                    () => AddError(CopyNotAllowed("權限名稱")))
                .IsValid();

            return isValid;
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
            bool isValid = await input.StartValidate()
                .Validate(i => i.GID.IsAboveZero(), () => AddError(EmptyNotAllowed("權限 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("權限名稱")))
                .Validate(i => i.Title.HasLengthBetween(1, 50), () => AddError(LengthOutOfRange("權限名稱", 1, 50)))
                .ValidateAsync(
                    async i => !await DC.GroupData.AnyAsync(gd =>
                        !gd.DeleteFlag && gd.Title == i.Title && gd.GID != i.GID),
                    () => AddError(CopyNotAllowed("權限名稱")))
                .IsValid();

            return isValid;
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

        #region SubmitMenuData

        private const string SameMdIdDetected = "發現重覆的 MDID，請檢查輸入內容！";

        /// <summary>
        /// 新增/更新權限對應單一選單的 API 權限。
        /// </summary>
        /// <param name="input">輸入值。參照 <see cref="GroupData_SubmitMenuData_Input_APIItem"/>。</param>
        /// <returns>通用回傳訊息格式</returns>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin,
            RequirePrivilege.AddFlag | RequirePrivilege.EditFlag | RequirePrivilege.DeleteFlag)]
        public async Task<string> SubmitMenuData(GroupData_SubmitMenuData_Input_APIItem input)
        {
            // 1. 驗證輸入
            if (!SubmitMenuDataValidateInput(input))
                return GetResponseJson();

            // 2. 更新 M_Group_Menu
            await SubmitMenuDataUpdateMGroupMenu(input);

            // 3. 更新 DB
            await SubmitMenuDataWriteDb();

            // 4. 回傳資料
            return GetResponseJson();
        }

        private async Task SubmitMenuDataWriteDb()
        {
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private async Task SubmitMenuDataUpdateMGroupMenu(GroupData_SubmitMenuData_Input_APIItem input)
        {
            // 輸入的 MDID
            IEnumerable<int> inputIds = input.GroupItems.Select(item => item.MDID).ToHashSet();

            // 依據輸入的 MDID 建立 MDID to MenuData 字典
            Dictionary<int, M_Group_Menu> menuIdToGroupMenuDict = await DC.M_Group_Menu
                .Where(mgm => mgm.GID == input.GID)
                .Where(mgm => inputIds.Contains(mgm.MDID))
                .ToDictionaryAsync(mgm => mgm.MDID, mgm => mgm);

            foreach (GroupData_MenuItem_APIItem item in input.GroupItems)
            {
                // 取得 MenuData，用於判定是否為必定顯示的特例值
                MenuData menuData = await DC.MenuData
                    .FirstOrDefaultAsync(md => md.MDID == item.MDID && md.ActiveFlag && !md.DeleteFlag);

                bool isAlwaysShow = menuData != null && DbConstants.AlwaysShowAddEditMenuUrls.Contains(menuData.URL);

                if (menuIdToGroupMenuDict.TryGetValue(item.MDID, out M_Group_Menu mgm))
                {
                    // 有原資料，則輸入的 ActiveFlag 為
                    // |- a.  true 時：修改資料。
                    // +- b. false 時：刪除資料。
                    if (!item.ActiveFlag)
                    {
                        DC.M_Group_Menu.Remove(mgm);
                        continue;
                    }

                    mgm.ShowFlag = isAlwaysShow || item.ShowFlag;
                    mgm.AddFlag = item.AddFlag;
                    mgm.EditFlag = item.EditFlag;
                    mgm.DeleteFlag = item.DeleteFlag;
                    mgm.PringFlag = item.PrintFlag;
                }
                else if (item.ActiveFlag)
                {
                    // 沒有原資料，若 ActiveFlag 為 true，新增一筆 M_Group_Menu
                    M_Group_Menu newEntity = new M_Group_Menu
                    {
                        GID = input.GID,
                        MDID = item.MDID,
                        ShowFlag = isAlwaysShow || item.ShowFlag,
                        AddFlag = item.AddFlag,
                        EditFlag = item.EditFlag,
                        DeleteFlag = item.DeleteFlag,
                        PringFlag = item.PrintFlag
                    };

                    await DC.M_Group_Menu.AddAsync(newEntity);
                }
            }
        }

        private bool SubmitMenuDataValidateInput(GroupData_SubmitMenuData_Input_APIItem input)
        {
            bool isInputValid = input.StartValidate()
                .Validate(i => i.GID.IsAboveZero(), () => AddError(EmptyNotAllowed("權限 ID")))
                .Validate(i => i.GroupItems.Any(), () => AddError(EmptyNotAllowed("選單權限列表")))
                .Validate(i => i.GroupItems.GroupBy(item => item.MDID).Count() == i.GroupItems.Count,
                    () => AddError(SameMdIdDetected))
                .IsValid();

            // 檢查是否所有 MDID 都存在
            bool isValid = isInputValid &&
                           input.GroupItems.Aggregate(true, (result, item) => result &
                                                                              item.StartValidate()
                                                                                  .Validate(_ =>
                                                                                          DC.MenuData.Any(md =>
                                                                                              md.ActiveFlag &&
                                                                                              !md.DeleteFlag &&
                                                                                              md.MDID == item.MDID),
                                                                                      () => AddError(
                                                                                          NotFound(
                                                                                              $"選單 ID {item.MDID}")))
                                                                                  .IsValid());

            return isValid;
        }

        #endregion
    }
}