using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// ChangeActive 功能的預設處理工具。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    public class ChangeActiveHelper<TController, TEntity> : IChangeActiveHelper
        where TController : PublicClass, IChangeActive<TEntity>
        where TEntity : class
    {
        private static string UpdateFailed(Exception e)
            => $"更新 DB 時出錯，請確認伺服器狀態：{e.Message}！";
        
        private readonly TController _controller;

        public ChangeActiveHelper(TController controller)
        {
            _controller = controller;
        }

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
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            if (!FlagHelper<TEntity>.HasActiveFlag)
                throw new NotSupportedException(ChangeActiveNotSupported);

            // 1. 驗證輸入。
            if (!id.IsValidId())
                _controller.AddError(ChangeActiveInputIdIncorrect);

            if (activeFlag is null)
                _controller.AddError(ChangeActiveInputFlagNotFound);

            if (_controller.HasError())
                return _controller.GetResponseJson();
            
            // ReSharper disable once PossibleInvalidOperationException
            bool activeFlagValue = activeFlag.Value;

            // 2. 查詢資料並確認刪除狀態。
            TEntity t = await _ChangeActiveQueryResult(id);

            if (t == null)
            {
                _controller.AddError(ChangeActiveNotFound);
                return _controller.GetResponseJson();
            }

            // 3. 實際更新起用狀態與更新者資訊，並寫入 DB。
            try
            {
                FlagHelper.SetActiveFlag(t, activeFlagValue);
                CreUpdHelper.SetInfosOnUpdate(_controller, t);
                await _controller.DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _controller.AddError(UpdateFailed(e));
            }

            // 4. 回傳。
            return _controller.GetResponseJson();
        }

        private async Task<TEntity> _ChangeActiveQueryResult(int id)
        {
            return await FlagHelper.FilterDeletedIfHasFlag(_controller.ChangeActiveQuery(id)).FirstOrDefaultAsync();
        }

        #endregion
    }
}