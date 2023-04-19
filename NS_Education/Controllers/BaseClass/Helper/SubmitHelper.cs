namespace NS_Education.Controllers.BaseClass.Helper
{
    public class SubmitHelper
    {
        
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

        #endregion

        #endregion
    }
}