using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.MenuData.MenuData.GetInfoById;
using NS_Education.Models.APIItems.Controller.MenuData.MenuData.GetList;
using NS_Education.Models.APIItems.Controller.MenuData.MenuData.Submit;
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
        , IGetInfoById<MenuData, MenuData_GetInfoById_Output_APIItem>
        , ISubmit<MenuData, MenuData_Submit_Input_APIItem>
    {
        #region Common

        private static string UpdateDbError(Exception e) => $"更新資料庫時失敗，請確認伺服器狀態：{e.Message}";
        private const string DataNotFound = "查無符合條件的資料！";

        #endregion

        #region Initialization
        
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        private readonly ISubmitHelper<MenuData_Submit_Input_APIItem> _submitHelper;

        public MenuDataController()
        {
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
            // 特殊邏輯：
            // 假設有下列這些資料：1 ParentId 0 SortNo 1
            //                 2 ParentId 0 SortNo 2
            //                 3 ParentId 1 SortNo 1
            //                 4 ParentId 2 SortNo 1
            //                 5 ParentId 2 SortNo 2
            // 則最後結果必須排列為：
            // 1 ParentId 0 SortNo 1
            // 3 ParentId 1 SortNo 1
            // 2 ParentId 0 SortNo 2
            // 4 ParentId 2 SortNo 1
            // 5 ParentId 2 SortNo 2
            //
            // 因為每筆資料需要取得 Parent，不符合域設步驟
            // 所以，無法使用 helper

            // 1. 驗證輸入
            if (!await GetListAllValidateInput(input))
                return GetResponseJson();
            
            // 2. 查詢資料
            // 依照 MDID 做成 dictionary, 並依照 parentId 做成 lookup
            var list = await GetListAllQuery(input).ToListAsync();
            var dictionary = list.ToDictionary(md => md.MDID, md => md);
            var lookup = dictionary.ToLookup(md => md.Value.ParentID, md => md.Value);

            // 建立 最後的 response
            BaseResponseForList<MenuData_GetList_Output_Row_APIItem> response =
                new BaseResponseForList<MenuData_GetList_Output_Row_APIItem>();

            // 3. 重新排序
            // 如果 parent 在 dictionary 中找不到，表示這是一個 parent，所以由 parent 開始長
            foreach (var parent in lookup.Where(grouping => !dictionary.ContainsKey(grouping.Key)).OrderBy(g => g.Key).SelectMany(g => g))
            {
                // 先把 parent 放進去
                response.Items.Add(await GetListAllEntityToRow(parent));

                // 再把 children 放進去
                foreach (var children in lookup[parent.MDID]
                             .OrderBy(md => md.SortNo)
                             .ThenBy(md => md.URL)
                             .ThenBy(md => md.Title)
                             .ThenBy(md => md.MDID))
                {
                    response.Items.Add(await GetListAllEntityToRow(children));
                }
            }
            
            // 4. 回傳結果
            return GetResponseJson(response);
        }

        public async Task<bool> GetListAllValidateInput(MenuData_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.ParentID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之選單上層 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<MenuData> GetListAllQuery(MenuData_GetList_Input_APIItem input)
        {
            IQueryable<MenuData> query = DC.MenuData.AsQueryable();

            if (input.ParentID.IsAboveZero())
                query = query.Where(md => md.ParentID == input.ParentID);

            if (input.ActiveFlag > -1)
                query = query.Where(md => md.ActiveFlag == (input.ActiveFlag == 1));
            
            return query.Where(md => md.DeleteFlag == (input.DeleteFlag == 1));
        }

        public async Task<MenuData_GetList_Output_Row_APIItem> GetListAllEntityToRow(MenuData entity)
        {
            var item = new MenuData_GetList_Output_Row_APIItem
            {
                MDID = entity.MDID,
                Title = entity.Title ?? "",
                URL = entity.URL ?? "",
                SortNo = entity.SortNo
            };

            await item.SetInfoFromEntity(entity, this);

            return item;
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
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbError(e));
            }
        }

        #endregion

        #region DeleteItem

        /// <summary>
        /// 刪除單筆或多筆選單以及其下層所有選單。
        /// </summary>
        /// <param name="input">輸入資料。參照 <see cref="DeleteItem_Input_APIItem"/></param>
        /// <returns>通用回傳格式訊息</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            // 有特殊邏輯：刪除時，需要連同子選單一同刪除，所以這裡不使用 Helper。
            await _deleteItem(input);

            return GetResponseJson();
        }

        private async Task _deleteItem(DeleteItem_Input_APIItem input)
        {
            // 1. 驗證輸入
            
            // 驗證是否皆為獨特 ID
            bool isCollectionValid = input.Items.ToArray().StartValidate()
                .Validate(items => items.GroupBy(i => i.Id).Count() == items.Length)
                .IsValid();
            
            // 驗證輸入內容
            bool isEveryElementValid = input.Items.StartValidateElements()
                .Validate(i => i.Id != null && i.Id.IsAboveZero(), i => AddError(EmptyNotAllowed($"欲更新的預約 ID（{i.Id}）")))
                .Validate(i => i.DeleteFlag != null, i => AddError(EmptyNotAllowed($"ID {i.Id} 的 DeleteFlag")))
                .IsValid();
            
            if (!isCollectionValid || !isEveryElementValid)
                return;
            
            foreach (DeleteItem_Input_Row_APIItem item in input.Items)
            {
                // 2. 找出資料
                var menuData = await DC.MenuData.Where(md => md.MDID == item.Id || md.ParentID == item.Id).ToListAsync();

                if (!menuData.Any())
                {
                    AddError(NotFound($"選單 ID {item.Id}"));
                }

                // 3. 設定資料
                foreach (MenuData data in menuData)
                {
                    data.DeleteFlag = item.DeleteFlag ?? throw new ArgumentNullException(nameof(DeleteItem_Input_Row_APIItem.DeleteFlag));
                }
            }
            
            // 4. 儲存至 DB
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
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
            bool isValid = await input.StartValidate()
                .Validate(i => i.MDID == 0, () => AddError(WrongFormat("選單 ID")))
                .Validate(i => i.ParentId.IsZeroOrAbove(), () => AddError(OutOfRange("上層選單 ID", 0)))
                .Validate(i => i.ParentId == 0 || i.ParentId != i.MDID, () => AddError(UnsupportedValue("上層選單 ID")))
                .ValidateAsync(
                    async i => i.ParentId == 0 || await DC.MenuData.ValidateIdExists(i.ParentId, nameof(MenuData.MDID)),
                    () => AddError(NotFound("上層選單 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("選單名稱")))
                .Validate(i => i.SortNo.IsZeroOrAbove(), () => AddError(WrongFormat("選單排序")))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.StartsWith("/") && !i.Url.EndsWith("/"),
                    () => AddError(WrongFormat("選單目標網址")))
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.Length.IsInBetween(0, 300),
                    () => AddError(TooLong("選單目標網址")))
                .Validate(i => i.Title.Length.IsInBetween(0, 50), () => AddError(TooLong("選單名稱")))
                .IsValid();

            return isValid;
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
                        .Where(md => md.ParentID == input.ParentId && md.ActiveFlag && !md.DeleteFlag)
                        .OrderByDescending(md => md.SortNo)
                        .Select(md => md.SortNo).FirstOrDefaultAsync() + 1
            };
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(MenuData_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.MDID.IsAboveZero(), () => AddError(EmptyNotAllowed("選單 ID")))
                .Validate(i => i.ParentId.IsZeroOrAbove(), () => AddError(OutOfRange("上層選單 ID", 0)))
                .Validate(i => i.ParentId == 0 || i.ParentId != i.MDID, () => AddError(UnsupportedValue("上層選單 ID")))
                .ValidateAsync(
                    async i => i.ParentId == 0 || await DC.MenuData.ValidateIdExists(i.ParentId, nameof(MenuData.MDID)),
                    () => AddError(NotFound("上層選單 ID")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("選單名稱")))
                .Validate(i => i.SortNo.IsZeroOrAbove(), () => AddError(WrongFormat("選單排序")))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.StartsWith("/") && !i.Url.EndsWith("/"),
                    () => AddError(WrongFormat("選單目標網址")))
                .Validate(i => i.Url.IsNullOrWhiteSpace() || i.Url.Length.IsInBetween(0, 300),
                    () => AddError(TooLong("選單目標網址")))
                .Validate(i => i.Title.Length.IsInBetween(0, 50), () => AddError(TooLong("選單名稱")))
                .IsValid();

            return isValid;
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
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbError(e));
            }
        }

        #endregion
    }
}