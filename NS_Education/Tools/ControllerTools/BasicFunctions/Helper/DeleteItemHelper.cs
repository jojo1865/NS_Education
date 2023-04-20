using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    public class DeleteItemHelper<TController, TEntity>
        where TController : PublicClass, IDeleteItem<TEntity>
        where TEntity : class
    {
        private readonly TController _controller;

        public DeleteItemHelper(TController controller)
        {
            _controller = controller;
        }
        
        #region DeleteItem

        private static string UpdateFailed(Exception e)
            => $"更新 DB 時出錯，請確認伺服器狀態：{e.Message}！";
        
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
        public async Task<string> DeleteItem(int id)
        {
            if (!FlagHelper<TEntity>.HasDeleteFlag)
                throw new NotSupportedException(DeleteItemNotSupported);

            // 1. 驗證輸入。
            if (id.IsValidId())
            {
                _controller.AddError(DeleteItemInputIncorrect);
                return _controller.GetResponseJson();
            }

            // 2. 查詢資料並確認刪除狀態。
            TEntity t = await _DeleteItemQueryResult(id);

            if (t == null)
            {
                _controller.AddError(DeleteItemNotFound);
                return _controller.GetResponseJson();
            }

            // 3. 更新刪除狀態與更新者資訊，並存入 DB。
            try
            {
                FlagHelper.SetDeleteFlag(t, true);
                CreUpdHelper.SetInfosOnUpdate(_controller, t);
                await _controller.DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _controller.AddError(UpdateFailed(e));
            }

            // 3. 回傳通用回傳訊息格式。
            return _controller.GetResponseJson();
        }

        private async Task<TEntity> _DeleteItemQueryResult(int id)
        {
            return await FlagHelper.FilterDeletedIfHasFlag(_controller.DeleteItemQuery(id)).FirstOrDefaultAsync();
        }

        #endregion
    }
}