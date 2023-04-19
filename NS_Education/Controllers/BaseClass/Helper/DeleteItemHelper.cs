namespace NS_Education.Controllers.BaseClass.Helper
{
    public class DeleteItemHelper
    {
        
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

        #endregion
    }
}