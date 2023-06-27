using System;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using NS_Education.Models.APIItems;
using NS_Education.Models.Errors.DataValidationErrors;
using NS_Education.Models.Errors.InputValidationErrors;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

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

        private IsolationLevel? _isolationLevel;

        /// <inheritdoc />
        public async Task<string> Submit(TSubmitRequest input, IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
            return await Submit(input);
        }

        /// <inheritdoc />
        public async Task<string> Submit(TSubmitRequest input)
        {
            // 1. 依據實作內容判定此次 Submit 為新增還是更新。
            // 2. 依據新增或更新模式，進行個別的輸入驗證。
            // |- a. 驗證通過時：執行邏輯。
            // +- b. 驗證失敗，且無錯誤訊息時：自動加上預設錯誤訊息。

            using (var transaction = _isolationLevel != null
                       ? _controller.DC.Database.BeginTransaction(_isolationLevel.Value)
                       : _controller.DC.Database.BeginTransaction())
            {
                try
                {
                    if (_controller.SubmitIsAdd(input))
                    {
                        if (await _controller.SubmitAddValidateInput(input))
                            await _SubmitAdd(input);
                        else if (!_controller.HasError())
                            _controller.AddError(new WrongFormatError());
                    }
                    else
                    {
                        if (await _controller.SubmitEditValidateInput(input))
                            await _SubmitEdit(input);
                        else if (!_controller.HasError())
                            _controller.AddError(new WrongFormatError());
                    }

                    if (!_controller.HasError())
                        transaction.Commit();
                }
                catch (Exception e)
                {
                    _controller.AddError(new UpdateDbFailedError(e));
                }

                if (_controller.HasError())
                    transaction.Rollback();
            }

            // 3. 回傳通用回傳訊息格式。

            return _controller.GetResponseJson();
        }

        #region Submit - Add

        private async Task _SubmitAdd(TSubmitRequest input)
        {
            // 1. 建立資料
            TEntity t = await _controller.SubmitCreateData(input);
            FlagHelper.SetActiveFlag(t, input.ActiveFlag);

            // 2. 如果 ID 不是 0，表示此前已有手動儲存至 DB，折返。
            // 否則，儲存至DB。
            if (_controller.DC.GetPrimaryKeyFromEntity(t) != 0)
                return;

            try
            {
                await Task.Run(() => _controller.DC.Set<TEntity>().Add(t));
                await _controller.DC.SaveChangesStandardProcedureAsync(_controller.GetUid(),
                    HttpContext.Current.Request);
            }
            catch (Exception e)
            {
                _controller.AddError(_controller.UpdateDbFailed(e));
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
                _controller.AddError(_controller.NotFound());
                return;
            }

            // 2. 覆寫資料
            _controller.SubmitEditUpdateDataFields(data, input);
            FlagHelper.SetActiveFlag(data, input.ActiveFlag);

            // 3. 儲存至 DB
            try
            {
                await _controller.DC.SaveChangesStandardProcedureAsync(_controller.GetUid(),
                    HttpContext.Current.Request);
            }
            catch (Exception e)
            {
                _controller.AddError(_controller.UpdateDbFailed(e));
            }
        }

        #endregion

        #endregion
    }
}