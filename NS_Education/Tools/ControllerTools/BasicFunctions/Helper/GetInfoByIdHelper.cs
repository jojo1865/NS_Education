using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
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
        where TGetResponse : BaseGetResponseRowInfusableWithCreUpd
    {
        private readonly TController _controller;

        public GetInfoByIdHelper(TController controller)
        {
            _controller = controller;
        }

        #region GetInfoByID

        public async Task<string> GetInfoById(int id)
        {
            // 1. 驗證輸入資料
            if (!id.IsAboveZero())
            {
                _controller.AddError(_controller.EmptyNotAllowed("欲查詢的 ID", nameof(id)));
                return _controller.GetResponseJson();
            }

            // 2. 取得單筆資料
            TEntity t = await _GetInfoByIdQueryResult(id);

            // 3. 寫 UserLog
            await _controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, _controller.GetUid(),
                HttpContext.Current.Request);

            // 4. 有資料時, 轉換成指定格式並回傳
            if (t != null)
            {
                TGetResponse response = await _controller.GetInfoByIdConvertEntityToResponse(t);
                await response.SetInfoFromEntity(t, _controller);
                return _controller.GetResponseJson(response);
            }

            // 5. 無資料時, 回傳錯誤
            _controller.AddError(_controller.NotFound());
            return _controller.GetResponseJson();
        }

        private async Task<TEntity> _GetInfoByIdQueryResult(int id)
        {
            // 取得實作者的查詢，並檢查刪除狀態
            return await FlagHelper.FilterDeletedIfHasFlag(_controller.GetInfoByIdQuery(id)).AsNoTracking()
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}