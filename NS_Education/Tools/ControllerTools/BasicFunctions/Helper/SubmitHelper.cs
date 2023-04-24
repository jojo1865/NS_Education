using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// Submit 功能的預設處理工具。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    /// <typeparam name="TSubmitRequest">傳入物件類型</typeparam>
    public class SubmitHelper<TController, TEntity, TSubmitRequest> : ISubmitHelper<TSubmitRequest>
        where TController : PublicClass, ISubmit<TEntity, TSubmitRequest>
        where TEntity : class
        where TSubmitRequest : BaseRequestForSubmit
    {
        private readonly TController _controller;

        public SubmitHelper(TController controller)
        {
            _controller = controller;
        }
        
        #region Submit
        
        private static string UpdateFailed(Exception e)
            => $"更新 DB 時出錯，請確認伺服器狀態：{e.Message}！";

        private const string SubmitAddValidateFailed = "欲新增資料的輸入格式不符！";
        private const string SubmitEditValidateFailed = "欲更新資料的輸入格式不符！";
        private const string SubmitEditNotFound = "查無欲更新的資料！";
        
        public async Task<string> Submit(TSubmitRequest input)
        {
            // 1. 依據實作內容判定此次 Submit 為新增還是更新。
            // 2. 依據新增或更新模式，進行個別的輸入驗證。
            // |- a. 驗證通過時：執行邏輯。
            // +- b. 驗證失敗，且無錯誤訊息時：自動加上預設錯誤訊息。
            
            if (_controller.SubmitIsAdd(input))
            {
                if (await _controller.SubmitAddValidateInput(input))
                    await _SubmitAdd(input);
                else if (!_controller.HasError())
                    _controller.AddError(SubmitAddValidateFailed);
            }
            else
            {
                if (await _controller.SubmitEditValidateInput(input))
                    await _SubmitEdit(input);
                else if (!_controller.HasError())
                    _controller.AddError(SubmitEditValidateFailed);
            }
            
            // 3. 回傳通用回傳訊息格式。

            return _controller.GetResponseJson();
        }

        #region Submit - Add

        private async Task _SubmitAdd(TSubmitRequest input)
        {
            // 1. 建立資料
            TEntity t = await _controller.SubmitCreateData(input);
            CreUpdHelper.SetInfosOnCreate(_controller, t);
            FlagHelper.SetActiveFlag(_controller, input.ActiveFlag);

            // 2. 儲存至 DB
            try
            {
                await _controller.DC.AddAsync(t);
                await _controller.DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _controller.AddError(UpdateFailed(e));
            }
        }

        #endregion

        #region Submit - Edit

        private async Task _SubmitEdit(TSubmitRequest input)
        {
            // 1. 查詢資料並確認刪除狀態
            TEntity data = await FlagHelper
                .FilterDeletedIfHasFlag(_controller.SubmitEditQuery(input))
                .FirstOrDefaultAsync();

            if (data == null)
            {
                _controller.AddError(SubmitEditNotFound);
                return;
            }

            // 2. 覆寫資料
            _controller.SubmitEditUpdateDataFields(data, input);
            CreUpdHelper.SetInfosOnUpdate(_controller, data);
            FlagHelper.SetActiveFlag(_controller, input.ActiveFlag);

            // 3. 儲存至 DB
            try
            {
                await _controller.DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _controller.AddError(UpdateFailed(e));
            }
        }

        #endregion

        #endregion
    }
}