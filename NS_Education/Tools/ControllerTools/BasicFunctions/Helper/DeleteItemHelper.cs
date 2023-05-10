using System;
using System.Data.Entity;
using System.Threading.Tasks;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// DeleteItem 功能的預設處理工具。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    public class DeleteItemHelper<TController, TEntity> : IDeleteItemHelper
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
            => e is null ? "更新 DB 時失敗！" : $"更新 DB 時失敗：{e.Message}；Inner:{e.InnerException?.Message}！";
        
        private const string DeleteItemNotSupported = "此 Controller 的資料型態不支援設定刪除狀態功能！";
        private const string DeleteItemInputIncorrect = "未輸入欲設定刪除狀態的 ID 或是不正確！";
        private const string DeleteItemNotFound = "查無欲設定刪除狀態的資料！";

        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <param name="deleteFlag">欲設定成的刪除狀態</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            if (!FlagHelper<TEntity>.HasDeleteFlag)
                throw new NotSupportedException(DeleteItemNotSupported);

            // 1. 驗證輸入。
            if (!id.IsAboveZero())
                _controller.AddError(DeleteItemInputIncorrect);

            if (deleteFlag == null)
                _controller.AddError(_controller.EmptyNotAllowed(nameof(deleteFlag)));
            
            if (_controller.HasError())
                return _controller.GetResponseJson();

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
                FlagHelper.SetDeleteFlag(t, deleteFlag ?? throw new ArgumentNullException(nameof(deleteFlag)));
                await _controller.DC.SaveChangesStandardProcedureAsync(_controller.GetUid());
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
            return await _controller.DeleteItemQuery(id).FirstOrDefaultAsync();
        }

        #endregion
    }
}