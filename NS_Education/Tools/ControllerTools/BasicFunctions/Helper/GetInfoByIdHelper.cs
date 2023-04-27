using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// GetInfoById 功能的預設處理工具。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    /// <typeparam name="TGetResponse">回傳物件類型</typeparam>
    public class GetInfoByIdHelper<TController, TEntity, TGetResponse> : IGetInfoByIdHelper
        where TController : PublicClass, IGetInfoById<TEntity, TGetResponse>
        where TEntity : class
        where TGetResponse : BaseGetResponseInfusableWithCreUpd
    {
        private readonly TController _controller;

        public GetInfoByIdHelper(TController controller)
        {
            _controller = controller;
        }
        
        #region GetInfoByID

        private const string GetInfoByIdInputIncorrect = "未輸入欲查詢的 ID 或是值不正確！";
        private const string GetInfoByIdNotFound = "查無欲查詢的資料！";
        
        public async Task<string> GetInfoById(int id)
        {
            // 1. 驗證輸入資料
            if (!id.IsValidId())
            {
                _controller.AddError(GetInfoByIdInputIncorrect);
                return _controller.GetResponseJson();
            }

            // 2. 取得單筆資料
            TEntity t = await _GetInfoByIdQueryResult(id);
            
            // 3. 寫 UserLog
            _controller.DC.WriteUserLog<TEntity>(id, UserLogControlType.Show);
            
            // 4. 有資料時, 轉換成指定格式並回傳
            if (t != null)
            {
                TGetResponse response = await _controller.GetInfoByIdConvertEntityToResponse(t);
                await response.SetInfoFromEntity(t, _controller);
                return _controller.GetResponseJson(response);
            }

            // 5. 無資料時, 回傳錯誤
            _controller.AddError(GetInfoByIdNotFound);
            return _controller.GetResponseJson();
        }

        private async Task<TEntity> _GetInfoByIdQueryResult(int id)
        {
            // 取得實作者的查詢，並檢查刪除狀態
            return await FlagHelper.FilterDeletedIfHasFlag(_controller.GetInfoByIdQuery(id)).FirstOrDefaultAsync();
        }

        #endregion
    }
}