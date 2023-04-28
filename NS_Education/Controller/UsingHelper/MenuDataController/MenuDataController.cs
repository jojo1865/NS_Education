using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.MenuData.MenuData.GetInfoById;
using NS_Education.Models.APIItems.MenuData.MenuData.GetList;
using NS_Education.Models.APIItems.MenuData.MenuData.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.MenuDataController
{
    public class MenuDataController : PublicClass
        , IGetListAll<MenuData, MenuData_GetList_Input_APIItem, MenuData_GetList_Output_Row_APIItem>
        , IGetInfoById<MenuData, MenuData_GetInfoById_Output_APIItem>
        , ISubmit<MenuData, MenuData_Submit_Input_APIItem>
    {
        #region Common

        private static string UpdateDbError(Exception e) => $"更新資料庫時失敗，請確認伺服器狀態：{e.Message}";
        private const string DataNotFound = "查無符合條件的資料！";

        #endregion

        #region Initialization

        private readonly IGetListAllHelper<MenuData_GetList_Input_APIItem> _getListAllHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        private readonly ISubmitHelper<MenuData_Submit_Input_APIItem> _submitHelper;

        public MenuDataController()
        {
            _getListAllHelper =
                new GetListAllHelper<MenuDataController, MenuData, MenuData_GetList_Input_APIItem,
                    MenuData_GetList_Output_Row_APIItem>(
                    this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<MenuDataController, MenuData, MenuData_GetInfoById_Output_APIItem>(this);
            _submitHelper = new SubmitHelper<MenuDataController, MenuData, MenuData_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(MenuData_GetList_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(MenuData_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.ParentID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之選單上層 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<MenuData> GetListAllOrderedQuery(MenuData_GetList_Input_APIItem input)
        {
            IQueryable<MenuData> query = DC.MenuData.AsQueryable();

            if (input.ParentID.IsAboveZero())
                query = query.Where(md => md.ParentID == input.ParentID);

            return query.OrderBy(md => md.SortNo)
                .ThenBy(md => md.URL)
                .ThenBy(md => md.Title)
                .ThenBy(md => md.MDID);
        }

        public async Task<MenuData_GetList_Output_Row_APIItem> GetListAllEntityToRow(MenuData entity)
        {
            return await Task.FromResult(new MenuData_GetList_Output_Row_APIItem
            {
                MDID = entity.MDID,
                Title = entity.Title ?? "",
                URL = entity.URL ?? "",
                SortNo = entity.SortNo
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

        public IQueryable<MenuData> GetInfoByIdQuery(int id)
        {
            return DC.MenuData.Where(md => md.MDID == id);
        }

        public async Task<MenuData_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(MenuData entity)
        {
            return await Task.FromResult(new MenuData_GetInfoById_Output_APIItem
            {
                MDID = entity.MDID,
                Title = entity.Title ?? "",
                URL = entity.URL ?? "",
                SortNo = entity.SortNo
            });
        }

        #endregion

        #region ChangeActive

        /// <summary>
        /// 更新單一選單的啟用 / 關閉狀態。設為關閉時，將連同下層選單一同設定。
        /// </summary>
        /// <param name="id">對象資料 ID</param>
        /// <param name="activeFlag">
        /// true：設為啟用<br/>
        /// false：設為關閉
        /// </param>
        /// <returns>通用回傳格式訊息</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            // 有特殊邏輯：關閉時，需要同步關閉下層所有選單，所以這裡不用 helper。
            await _changeActive(id, activeFlag);

            return GetResponseJson();
        }

        private async Task _changeActive(int id, bool? activeFlag)
        {
            // 1. 驗證輸入
            bool isValid = this.StartValidate()
                .Validate(_ => id.IsAboveZero(), () => AddError(EmptyNotAllowed("欲更新的預約 ID")))
                .Validate(_ => activeFlag != null, () => AddError(EmptyNotAllowed("ActiveFlag")))
                .IsValid();

            if (!isValid)
                return;

            // 2. 執行更新
            List<MenuData> menuData = await DC.MenuData
                .Where(md => !md.DeleteFlag)
                // 僅在關閉時才連同下層一同關閉
                .Where(md => md.MDID == id || activeFlag == false && md.ParentID == id)
                .ToListAsync();

            if (!menuData.Any())
            {
                AddError(DataNotFound);
            }

            foreach (MenuData data in menuData)
            {
                data.ActiveFlag = activeFlag ?? throw new ArgumentNullException(nameof(activeFlag));
            }

            // 3. 寫入 DB
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                AddError(UpdateDbError(e));
            }
        }

        #endregion

        #region DeleteItem

        /// <summary>
        /// 刪除單一選單以及其下層所有選單。
        /// </summary>
        /// <param name="id">對象資料 ID</param>
        /// <param name="deleteFlag">
        /// true：刪除<br/>
        /// false：取消刪除
        /// </param>
        /// <returns>通用回傳格式訊息</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            // 有特殊邏輯：刪除時，需要連同子選單一同刪除，所以這裡不使用 Helper。
            await _deleteItem(id, deleteFlag);

            return GetResponseJson();
        }

        private async Task _deleteItem(int id, bool? deleteFlag)
        {
            // 1. 驗證輸入
            bool isValid = this.StartValidate()
                .Validate(_ => id.IsAboveZero(), () => AddError(EmptyNotAllowed("欲更新的預約 ID")))
                .Validate(_ => deleteFlag != null, () => AddError(EmptyNotAllowed("DeleteFlag")))
                .IsValid();

            if (!isValid)
                return;

            // 2. 找出資料
            var menuData = await DC.MenuData.Where(md => md.MDID == id || md.ParentID == id).ToListAsync();

            if (!menuData.Any())
            {
                AddError(DataNotFound);
                return;
            }

            // 3. 設定資料
            foreach (MenuData data in menuData)
            {
                data.DeleteFlag = deleteFlag ?? throw new ArgumentNullException(nameof(deleteFlag));
            }

            // 4. 儲存至 DB
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                AddError(UpdateDbError(e));
            }
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.AddOrEdit, null, nameof(MenuData_Submit_Input_APIItem.MDID))]
        public async Task<string> Submit(MenuData_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(MenuData_Submit_Input_APIItem input)
        {
            return input.MDID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(MenuData_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MDID == 0, () => AddError(WrongFormat("選單 ID")))
                .Validate(i => i.ParentId.IsZeroOrAbove(), () => AddError(WrongFormat("上層選單 ID")))
                .Validate(i => i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("選單名稱")))
                .Validate(i => i.SortNo.IsZeroOrAbove(), () => AddError(WrongFormat("選單排序")))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.StartsWith("/") && !i.Url.EndsWith("/"), () => AddError(WrongFormat("選單目標網址")))
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.Length.IsInBetween(0, 300), () => AddError(TooLong("選單目標網址")))
                .Validate(i => i.Title.Length.IsInBetween(0, 50), () => AddError(TooLong("選單名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<MenuData> SubmitCreateData(MenuData_Submit_Input_APIItem input)
        {
            return new MenuData
            {
                ParentID = input.ParentId,
                Title = input.Title,
                URL = input.Url,
                SortNo = input.SortNo.IsAboveZero()
                    ? input.SortNo
                    : await DC.MenuData
                        .OrderByDescending(md => md.SortNo)
                        .Select(md => md.SortNo).FirstOrDefaultAsync() + 1
            };
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(MenuData_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MDID.IsAboveZero(), () => AddError(EmptyNotAllowed("選單 ID")))
                .Validate(i => i.ParentId.IsZeroOrAbove(), () => AddError(WrongFormat("上層選單 ID")))
                .Validate(i => i.Title.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("選單名稱")))
                .Validate(i => i.SortNo.IsZeroOrAbove(), () => AddError(WrongFormat("選單排序")))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.StartsWith("/") && !i.Url.EndsWith("/"), () => AddError(WrongFormat("選單目標網址")))
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.Length.IsInBetween(0, 300), () => AddError(TooLong("選單目標網址")))
                .Validate(i => i.Title.Length.IsInBetween(0, 50), () => AddError(TooLong("選單名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<MenuData> SubmitEditQuery(MenuData_Submit_Input_APIItem input)
        {
            return DC.MenuData.Where(md => md.MDID == input.MDID);
        }

        public void SubmitEditUpdateDataFields(MenuData data, MenuData_Submit_Input_APIItem input)
        {
            data.ParentID = input.ParentId;
            data.Title = input.Title ?? data.Title;
            data.URL = input.Url ?? data.URL;
            data.SortNo = input.SortNo.IsAboveZero()
                ? input.SortNo
                : data.SortNo;
        }

        #endregion

        #endregion
        
        #region ChangeSortNo

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeSortNo(int id, int sortNo)
        {
            await _changeSortNo(id, sortNo);

            return GetResponseJson();
        }

        private async Task _changeSortNo(int id, int sortNo)
        {
            // 1. 驗證輸入
            bool isValid = this.StartValidate()
                .Validate(_ => id.IsAboveZero(), () => AddError(EmptyNotAllowed("欲更新的選單 ID")))
                .Validate(_ => sortNo.IsAboveZero(), () => AddError(EmptyNotAllowed("新的排序數字")))
                .IsValid();

            if (!isValid)
                return;
            
            // 2. 查出資料
            MenuData menuData = await DC.MenuData.FirstOrDefaultAsync(md => md.MDID == id && !md.DeleteFlag);

            if (menuData == null)
            {
                AddError(DataNotFound);
                return;
            }
            
            // 3. 修改資料並儲存
            try
            {
                menuData.SortNo = sortNo;
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                AddError(UpdateDbError(e));
            }
        }

        #endregion
    }
}