using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Tools;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers.BaseClass
{
    public abstract class
        BaseController<TEntity, TGetListRequest, TGetListRow, TGetResponse, TSubmitRequest> : PublicClass
        where TEntity : class
        where TGetListRequest : BaseRequestForList
        where TGetListRow : class
        where TGetResponse : cReturnMessageInfusableAbstract
        where TSubmitRequest : class
    {
        #region 通用

        private static string UpdateFailed(Exception e)
            => $"寫入或更新 DB 時出錯，請確認伺服器狀態：{e.Message}！";

        private const string DefaultAddOrEditKeyFieldName = "ID";

        private const string ActiveFlag = "ActiveFlag";
        private const string DeleteFlag = "DeleteFlag";
        private const string CreUid = "CreUID";
        private const string CreDate = "CreDate";
        private const string UpdUid = "UpdUID";
        private const string UpdDate = "UpdDate";

        private static bool HasActiveFlag { get; } = HasProperty(typeof(TEntity), ActiveFlag);
        private static bool HasDeleteFlag { get; } = HasProperty(typeof(TEntity), DeleteFlag);

        private static bool HasCreUid { get; } = HasProperty(typeof(TEntity), CreUid);
        private static bool HasCreDate { get; } = HasProperty(typeof(TEntity), CreDate);
        private static bool HasUpdUid { get; } = HasProperty(typeof(TEntity), UpdUid);
        private static bool HasUpdDate { get; } = HasProperty(typeof(TEntity), UpdDate);

        private static void SetProperty<T>(T t, string propertyName, object value) =>
            GetProperty<T>(propertyName).SetValue(t, value);

        private static PropertyInfo GetProperty<T>(string propertyName) => typeof(T).GetProperty(propertyName);
        private static PropertyInfo GetProperty(Type type, string propertyName) => type.GetProperty(propertyName);
        private static bool HasProperty(Type type, string propertyName) => !(GetProperty(type, propertyName) is null);

        private static IQueryable<TEntity> FilterDeletedIfHasFlag(IQueryable<TEntity> query)
        {
            if (HasDeleteFlag)
                query = query.Where(entity => EF.Property<bool>(entity, DeleteFlag) == false);
            return query;
        }

        private void SetInfosOnUpdate(TEntity t)
        {
            if (HasUpdUid)
                SetProperty(t, UpdUid, GetUid());

            if (HasUpdDate)
                SetProperty(t, UpdDate, DateTime.Now);
        }

        private void SetInfosOnCreate(TEntity t)
        {
            if (HasCreUid)
                SetProperty(t, CreUid, GetUid());

            if (HasCreDate)
                SetProperty(t, CreDate, DateTime.Now);

            if (HasUpdUid)
                SetProperty(t, UpdUid, 0);

            if (HasCreDate)
                SetProperty(t, UpdDate, DateTime.Now);
        }

        #endregion

        #region GetList

        private static string GetListNotFound => $"{typeof(TEntity).Name} GetList 時查無資料！";

        /// <summary>
        /// 取得列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗，或找不到資料時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public virtual async Task<string> GetList(TGetListRequest input)
        {
            // 1. 驗證輸入
            bool inputValidated = await GetListValidateInput(input);

            if (!inputValidated)
                return GetResponseJson();

            // 2. 執行查詢
            var response = new BaseResponseForList<TGetListRow>();
            response.SetByInput(input);

            var queryResult = await _GetListQueryResult(input, response);

            // 3. 按照格式回傳結果
            if (!queryResult.Any() || HasError())
            {
                AddError(GetListNotFound);
                return GetResponseJson();
            }

            response.Items = queryResult.Select(GetListEntityToRow).ToList();

            return GetResponseJson(response);
        }

        /// <summary>
        /// 驗證取得列表的輸入資料。<br/>
        /// 當此方法回傳 false 時，回到主方法後就會提早回傳。
        /// </summary>
        /// <param name="i">輸入資料</param>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證不通過。
        /// </returns>
        protected abstract Task<bool> GetListValidateInput(TGetListRequest i);

        private async Task<IList<TEntity>> _GetListQueryResult(TGetListRequest t,
            BaseResponseForList<TGetListRow> response)
        {
            IQueryable<TEntity> query = FilterDeletedIfHasFlag(GetListOrderedQuery(t));

            // 1. 先取得總筆數

            int totalRows = await query.CountAsync();
            response.AllItemCt = totalRows;

            // 2. 再回傳實際資料

            return await query
                .Skip(t.GetStartIndex())
                .Take(t.GetTakeRowCount())
                .ToListAsync();
        }

        /// <summary>
        /// 依據取得列表的輸入資料，取得查詢。<br/>
        /// 實作者可以在這個方法中進行 AddError，回到主方法後就不會實際執行查詢，而是提早回傳。<br/>
        /// </summary>
        /// <returns>具備排序的查詢。</returns>
        /// <remarks>若此方法是藉由預設的 GetList 方法被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        protected abstract IOrderedQueryable<TEntity> GetListOrderedQuery(TGetListRequest t);

        /// <summary>
        /// 將取得列表的查詢結果轉換成 Response 所需的子物件類型。。<br/>
        /// 時作者可以在這個方法中進行 AddError，最後回傳結果仍會包含資料，但會告知前端結果並不成功。（Success = false）
        /// </summary>
        /// <param name="t">單筆查詢結果</param>
        /// <returns>Response 所需類型的單筆資料</returns>
        protected abstract TGetListRow GetListEntityToRow(TEntity t);

        #endregion

        #region GetInfoByID

        private const string GetInfoByIdInputIncorrect = "未輸入欲查詢的 ID 或是值不正確！";
        private const string GetInfoByIdNotFound = "查無欲查詢的資料！";

        /// <summary>
        /// 取得單筆資料。
        /// </summary>
        /// <param name="id">查詢用的索引鍵</param>
        /// <returns>
        /// 成功時：包含資料的通用訊息回傳格式。<br/>
        /// 輸入驗證失敗，或查無資料時：不包含資料的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public virtual async Task<string> GetInfoById(int id)
        {
            // 1. 驗證輸入資料
            if (!id.IsValidId())
            {
                AddError(GetInfoByIdInputIncorrect);
                return GetResponseJson();
            }

            // 2. 取得單筆資料
            TEntity t = await _GetInfoByIdQueryResult(id);

            // 3. 有資料時, 轉換成指定格式並回傳
            if (t != null)
                return GetResponseJson(GetInfoByIdConvertEntityToResponse(t));

            // 4. 無資料時, 回傳錯誤
            AddError(GetInfoByIdNotFound);
            return GetResponseJson();
        }

        private async Task<TEntity> _GetInfoByIdQueryResult(int id)
        {
            // 取得實作者的查詢，並檢查刪除狀態
            return await FilterDeletedIfHasFlag(GetInfoByIdQuery(id)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 取得單筆的查詢。
        /// </summary>
        /// <param name="id">使用者輸入的查詢用索引鍵</param>
        /// <returns>查詢。</returns>
        /// <remarks>若此方法是藉由預設的 GetInfoById 方法被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        protected abstract IQueryable<TEntity> GetInfoByIdQuery(int id);

        /// <summary>
        /// 將單筆查詢的結果轉換成 Response 所需類型的物件。
        /// </summary>
        /// <param name="entity">原查詢結果</param>
        /// <returns>Response 所需類型的物件</returns>
        protected abstract TGetResponse GetInfoByIdConvertEntityToResponse(TEntity entity);

        #endregion

        #region ChangeActive

        private const string ChangeActiveNotSupported = "此 Controller 的資料型態不支援啟用/停用功能！";
        private const string ChangeActiveInputIdIncorrect = "未輸入欲更新的 ID 或是不正確！";
        private const string ChangeActiveInputFlagNotFound = "未提供啟用狀態的新值！";
        private const string ChangeActiveNotFound = "查無欲更新的資料！";

        /// <summary>
        /// 修改資料的啟用/停用狀態。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <param name="activeFlag">欲修改成的狀態</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入驗證失敗、查無資料、DB 異常時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他錯誤時：拋錯。
        /// </returns>
        /// <exception cref="NotSupportedException">資料沒有 ActiveFlag 欄位時</exception>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public virtual async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            if (!HasActiveFlag)
                throw new NotSupportedException(ChangeActiveNotSupported);

            // 1. 驗證輸入。
            if (!id.IsValidId())
                AddError(ChangeActiveInputIdIncorrect);

            if (activeFlag is null)
                AddError(ChangeActiveInputFlagNotFound);

            // ReSharper disable once PossibleInvalidOperationException
            bool activeFlagValue = activeFlag.Value;

            if (HasError())
                return GetResponseJson();

            // 2. 查詢資料並確認刪除狀態。
            TEntity t = await _ChangeActiveQueryResult(id);

            if (t == null)
            {
                AddError(ChangeActiveNotFound);
                return GetResponseJson();
            }

            // 3. 實際更新起用狀態與更新者資訊，並寫入 DB。
            try
            {
                SetProperty(t, ActiveFlag, activeFlagValue);
                SetInfosOnUpdate(t);
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                AddError(UpdateFailed(e));
            }

            // 4. 回傳。
            return GetResponseJson();
        }

        private async Task<TEntity> _ChangeActiveQueryResult(int id)
        {
            return await FilterDeletedIfHasFlag(ChangeActiveQuery(id)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 修改啟用/停用狀態的查詢。如果與 GetInfoById 的邏輯相同，可以直接使用 base。
        /// </summary>
        /// <param name="id">欲修改狀態資料的 ID</param>
        /// <returns>查詢</returns>
        /// <remarks>若此方法是藉由預設的 ChangeActive 方法被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        protected virtual IQueryable<TEntity> ChangeActiveQuery(int id)
        {
            return GetInfoByIdQuery(id);
        }

        #endregion

        #region DeleteItem

        private const string DeleteItemNotSupported = "此 Controller 的資料型態不支援刪除功能！";
        private const string DeleteItemInputIncorrect = "未輸入欲刪除的 ID 或是不正確！";
        private const string DeleteItemNotFound = "查無欲刪除的資料！";

        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public virtual async Task<string> DeleteItem(int id)
        {
            if (!HasDeleteFlag)
                throw new NotSupportedException(DeleteItemNotSupported);

            // 1. 驗證輸入。
            if (id.IsValidId())
            {
                AddError(DeleteItemInputIncorrect);
                return GetResponseJson();
            }

            // 2. 查詢資料並確認刪除狀態。
            TEntity t = await _DeleteItemQueryResult(id);

            if (t == null)
            {
                AddError(DeleteItemNotFound);
                return GetResponseJson();
            }

            // 3. 更新刪除狀態與更新者資訊，並存入 DB。
            try
            {
                SetProperty(t, DeleteFlag, true);
                SetInfosOnUpdate(t);
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                AddError(UpdateFailed(e));
            }

            // 3. 回傳通用回傳訊息格式。
            return GetResponseJson();
        }

        private async Task<TEntity> _DeleteItemQueryResult(int id)
        {
            return await FilterDeletedIfHasFlag(DeleteItemQuery(id)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 刪除單筆資料的查詢。如果與 GetInfoById 的邏輯相同，可以直接使用 base。
        /// </summary>
        /// <param name="id">欲刪除資料的 ID</param>
        /// <returns>查詢</returns>
        /// <remarks>若此方法是藉由預設的 DeleteItem 方法被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        protected virtual IQueryable<TEntity> DeleteItemQuery(int id)
        {
            return GetInfoByIdQuery(id);
        }

        #endregion

        #region Submit

        private const string SubmitAddValidateFailed = "欲新增資料的輸入格式不符！";
        private const string SubmitEditValidateFailed = "欲更新資料的輸入格式不符！";
        private const string SubmitEditNotFound = "查無欲更新的資料！";

        /// <summary>
        /// 新增或更新一筆資料。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入驗證失敗時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 新增，但 DB 連線異常時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 修改，但查無資料，或 DB 連線異常時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他錯誤時：拋錯。
        /// </returns>
        /// <remarks>若未覆寫 JwtAuthFilter，則在權限判定時，會要求同時具備 Add 與 Edit 的權限。</remarks>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddFlag | RequirePrivilege.EditFlag, null, null)]
        public virtual async Task<string> Submit(TSubmitRequest input)
        {
            // 1. 依據實作內容判定此次 Submit 為新增還是更新。
            // 2. 依據新增或更新模式，進行個別的輸入驗證。
            // |- a. 驗證通過時：執行邏輯。
            // +- b. 驗證失敗，且無錯誤訊息時：自動加上預設錯誤訊息。
            
            if (SubmitIsAdd(input))
            {
                if (await SubmitAddValidateInput(input))
                    await _SubmitAdd(input);
                else if (!HasError())
                    AddError(SubmitAddValidateFailed);
            }
            else
            {
                if (await SubmitEditValidateInput(input))
                    await _SubmitEdit(input);
                else if (!HasError())
                    AddError(SubmitEditValidateFailed);
            }
            
            // 3. 回傳通用回傳訊息格式。

            return GetResponseJson();
        }

        /// <summary>
        /// 新增或更新一筆資料時，判定此次要求是否為新增。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// true：為新增。<br/>
        /// false：不是新增（視為更新）。
        /// </returns>
        protected abstract bool SubmitIsAdd(TSubmitRequest input);

        #region Submit - Add

        private async Task _SubmitAdd(TSubmitRequest input)
        {
            // 1. 建立資料
            TEntity t = SubmitCreateData(input);
            SetInfosOnCreate(t);

            // 2. 儲存至 DB
            try
            {
                await DC.AddAsync(t);
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                AddError(UpdateFailed(e));
            }
        }

        /// <summary>
        /// 新增一筆資料時，驗證輸入格式。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證錯誤。
        /// </returns>
        protected abstract Task<bool> SubmitAddValidateInput(TSubmitRequest input);
        
        /// <summary>
        /// 新增一筆資料時，依據輸入建立新物件的方法。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>欲新增的物件</returns>
        /// <remarks>若此方法是藉由預設的 Submit 方法被呼叫時，實作者可以忽略 CreUid、CreDate、UpdUid 及 UpdDate 的設定。</remarks>
        protected abstract TEntity SubmitCreateData(TSubmitRequest input);

        #endregion

        #region Submit - Edit

        private async Task _SubmitEdit(TSubmitRequest input)
        {
            // 1. 查詢資料並確認刪除狀態
            TEntity data = await FilterDeletedIfHasFlag(SubmitEditQuery(input)).FirstOrDefaultAsync();

            if (data == null)
            {
                AddError(SubmitEditNotFound);
                return;
            }

            // 2. 覆寫資料
            SubmitEditUpdateDataFields(data, input);
            SetInfosOnUpdate(data);

            // 3. 儲存至 DB
            try
            {
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                AddError(UpdateFailed(e));
            }
        }

        /// <summary>
        /// 更新一筆資料時，驗證輸入格式。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證錯誤。
        /// </returns>
        protected abstract Task<bool> SubmitEditValidateInput(TSubmitRequest input);
        
        /// <summary>
        /// 更新一筆資料時，依據輸入覆寫資料各欄位的方法。
        /// </summary>
        /// <param name="data">DB 資料</param>
        /// <param name="input">輸入</param>
        /// <remarks>若此方法是藉由預設的 Submit 方法被呼叫時，實作者在更新時可以忽略 UpdUid 及 UpdDate 的更新。</remarks>
        protected abstract void SubmitEditUpdateDataFields(TEntity data, TSubmitRequest input);
        
        /// <summary>
        /// 更新一筆資料時，找出原資料的查詢。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>查詢</returns>
        /// <remarks>若此方法是藉由預設的 Submit 方法被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        protected abstract IQueryable<TEntity> SubmitEditQuery(TSubmitRequest input);

        #endregion

        #endregion
    }
}