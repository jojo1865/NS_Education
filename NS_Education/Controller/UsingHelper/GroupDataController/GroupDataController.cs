using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.GroupData;
using NS_Education.Models.APIItems.Controller.GroupData.GetInfoById;
using NS_Education.Models.APIItems.Controller.GroupData.GetList;
using NS_Education.Models.APIItems.Controller.GroupData.Submit;
using NS_Education.Models.APIItems.Controller.GroupData.SubmitMenuData;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.GroupDataController
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
                        ThisGroupMenu = md.M_Group_Menu.FirstOrDefault(mgm => mgm.GID == entity.GID)
                    })
                    .Select(result => new GroupData_MenuItem_APIItem
                    {
                        MDID = result.MenuData.MDID,
                        Title = result.MenuData.Title ?? "",
                        AddFlag = result.MenuData.AlwaysAllowAdd || (result.ThisGroupMenu?.AddFlag ?? false),
                        ShowFlag = result.MenuData.AlwaysAllowShow || (result.ThisGroupMenu?.ShowFlag ?? false),
                        EditFlag = result.MenuData.AlwaysAllowEdit || (result.ThisGroupMenu?.EditFlag ?? false),
                        DeleteFlag = result.MenuData.AlwaysAllowDelete || (result.ThisGroupMenu?.DeleteFlag ?? false),
                        PrintFlag = result.MenuData.AlwaysAllowPring || (result.ThisGroupMenu?.PringFlag ?? false),
                        AddFlagReadOnly = result.MenuData.AlwaysAllowAdd,
                        ShowFlagReadOnly = result.MenuData.AlwaysAllowShow,
                        EditFlagReadOnly = result.MenuData.AlwaysAllowEdit,
                        DeleteFlagReadOnly = result.MenuData.AlwaysAllowDelete,
                        PrintFlagReadOnly = result.MenuData.AlwaysAllowPring
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
                AddError(CopyNotAllowed($"權限名稱（{kvp.Key}）", nameof(GroupData.Title)));
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
                AddError(CopyNotAllowed($"權限名稱（{aliveSameName}）", nameof(GroupData.Title)));
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
                .Validate(i => i.GID == 0, () => AddError(WrongFormat("權限 ID", nameof(input.GID))))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("權限名稱", nameof(input.Title))))
                .Validate(i => i.Title.HasLengthBetween(1, 50),
                    () => AddError(LengthOutOfRange("權限名稱", nameof(input.Title), 1, 50)))
                .SkipIfAlreadyInvalid()
                .ValidateAsync(
                    async i => !await DC.GroupData.AnyAsync(gd =>
                        !gd.DeleteFlag && gd.Title == i.Title && gd.GID != i.GID),
                    () => AddError(CopyNotAllowed("權限名稱", nameof(input.Title))))
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
                .Validate(i => i.GID.IsAboveZero(), () => AddError(EmptyNotAllowed("權限 ID", nameof(input.GID))))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("權限名稱", nameof(input.Title))))
                .Validate(i => i.Title.HasLengthBetween(1, 50),
                    () => AddError(LengthOutOfRange("權限名稱", nameof(input.Title), 1, 50)))
                .ValidateAsync(
                    async i => !await DC.GroupData.AnyAsync(gd =>
                        !gd.DeleteFlag && gd.Title == i.Title && gd.GID != i.GID),
                    () => AddError(CopyNotAllowed("權限名稱", nameof(input.Title))))
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
            if (!await SubmitMenuDataValidateInput(input))
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

            // 依據輸入的 MDID 建立 MDID to M_GroupMenu 字典
            Dictionary<int, M_Group_Menu> menuIdToGroupMenuDict = await DC.M_Group_Menu
                .Where(mgm => mgm.GID == input.GID)
                .Where(mgm => inputIds.Contains(mgm.MDID))
                .ToDictionaryAsync(mgm => mgm.MDID, mgm => mgm);

            foreach (GroupData_MenuItem_APIItem item in input.GroupItems)
            {
                // 取得 MenuData，用於判定是否為必定顯示的特例值
                MenuData menuData = await DC.MenuData
                    .FirstOrDefaultAsync(md => md.MDID == item.MDID && md.ActiveFlag && !md.DeleteFlag);

                bool needsData = item.AddFlag || item.DeleteFlag || item.EditFlag || item.PrintFlag || item.ShowFlag;

                if (menuIdToGroupMenuDict.TryGetValue(item.MDID, out M_Group_Menu mgm))
                {
                    // 有原資料，則輸入的 ActiveFlag 為
                    // |- a.  true 時：修改資料。
                    // +- b. false 時：刪除資料。
                    if (!needsData)
                    {
                        DC.M_Group_Menu.Remove(mgm);
                        continue;
                    }

                    mgm.ShowFlag = (menuData?.AlwaysAllowShow ?? false) || item.ShowFlag;
                    mgm.AddFlag = (menuData?.AlwaysAllowAdd ?? false) || item.AddFlag;
                    mgm.EditFlag = (menuData?.AlwaysAllowEdit ?? false) || item.EditFlag;
                    mgm.DeleteFlag = (menuData?.AlwaysAllowDelete ?? false) || item.DeleteFlag;
                    mgm.PringFlag = (menuData?.AlwaysAllowPring ?? false) || item.PrintFlag;
                }
                else if (needsData)
                {
                    // 沒有原資料，若 ActiveFlag 為 true，新增一筆 M_Group_Menu
                    M_Group_Menu newEntity = new M_Group_Menu
                    {
                        GID = input.GID,
                        MDID = item.MDID,
                        ShowFlag = (menuData?.AlwaysAllowShow ?? false) || item.ShowFlag,
                        AddFlag = (menuData?.AlwaysAllowAdd ?? false) || item.AddFlag,
                        EditFlag = (menuData?.AlwaysAllowEdit ?? false) || item.EditFlag,
                        DeleteFlag = (menuData?.AlwaysAllowDelete ?? false) || item.DeleteFlag,
                        PringFlag = (menuData?.AlwaysAllowPring ?? false) || item.PrintFlag
                    };

                    await DC.M_Group_Menu.AddAsync(newEntity);
                }
            }
        }

        private async Task<bool> SubmitMenuDataValidateInput(GroupData_SubmitMenuData_Input_APIItem input)
        {
            bool isInputValid = input.StartValidate()
                .Validate(i => i.GID.IsAboveZero(), () => AddError(EmptyNotAllowed("權限 ID", nameof(input.GID))))
                .Validate(i => i.GroupItems.Any(), () => AddError(EmptyNotAllowed("選單權限列表", nameof(input.GroupItems))))
                .Validate(i => i.GroupItems.GroupBy(item => item.MDID).Count() == i.GroupItems.Count,
                    () => AddError(CopyNotAllowed("選單 ID", nameof(input.GroupItems))))
                .IsValid();

            // 檢查是否所有 MDID 都存在
            var inputMdIds = input.GroupItems.Select(gi => gi.MDID);

            Dictionary<int, MenuData> allMenuData = await DC.MenuData.Where(md => md.ActiveFlag && !md.DeleteFlag)
                .Where(md => inputMdIds.Contains(md.MDID))
                .ToDictionaryAsync(md => md.MDID, md => md);

            bool isAllMdIdValid = isInputValid &&
                                  input.GroupItems.StartValidateElements()
                                      .Validate(item => allMenuData.Any(kvp => kvp.Key == item.MDID),
                                          item => AddError(NotFound($"選單 ID {item.MDID}", nameof(item.MDID))))
                                      .IsValid();

            // 檢查所有 MD 的 flag 沒有與 Always flags 衝突
            bool isAllFlagsCorrect = isAllMdIdValid &&
                                     input.GroupItems.StartValidateElements()
                                         .Validate(item => !allMenuData[item.MDID].AlwaysAllowShow || item.ShowFlag,
                                             item => AddError(ExpectedValue($"{allMenuData[item.MDID].Title}是否允許瀏覽",
                                                 nameof(item.ShowFlag),
                                                 "允許")))
                                         .Validate(item => !allMenuData[item.MDID].AlwaysAllowAdd || item.AddFlag,
                                             item => AddError(ExpectedValue($"{allMenuData[item.MDID].Title}是否允許新增",
                                                 nameof(item.AddFlag),
                                                 "允許")))
                                         .Validate(item => !allMenuData[item.MDID].AlwaysAllowEdit || item.EditFlag,
                                             item => AddError(ExpectedValue($"{allMenuData[item.MDID].Title}是否允許更新",
                                                 nameof(item.EditFlag),
                                                 "允許")))
                                         .Validate(item => !allMenuData[item.MDID].AlwaysAllowDelete || item.DeleteFlag,
                                             item => AddError(ExpectedValue($"{allMenuData[item.MDID].Title}是否允許刪除",
                                                 nameof(item.DeleteFlag),
                                                 "允許")))
                                         .Validate(item => !allMenuData[item.MDID].AlwaysAllowPring || item.PrintFlag,
                                             item => AddError(ExpectedValue($"{allMenuData[item.MDID].Title}是否允許匯出",
                                                 nameof(item.PrintFlag),
                                                 "允許")))
                                         .IsValid();

            return isInputValid && isAllMdIdValid && isAllFlagsCorrect;
        }

        #endregion
    }
}