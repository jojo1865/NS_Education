using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass.FunctionInterface;
using NS_Education.Tools;
using NS_Education.Tools.Extensions;

namespace NS_Education.Controllers.BaseClass.Helper
{
    public class GetInfoByIdHelper<TController, TEntity, TGetResponse>
        where TController : PublicClass, IGetInfoById<TEntity, TGetResponse>
        where TEntity : class
        where TGetResponse : cReturnMessageInfusableAbstract
    {
        private readonly TController _controller;

        public GetInfoByIdHelper(TController controller)
        {
            _controller = controller;
        }
        
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
        public async Task<string> GetInfoById( int id)
        {
            // 1. 驗證輸入資料
            if (!id.IsValidId())
            {
                _controller.AddError(GetInfoByIdInputIncorrect);
                return _controller.GetResponseJson();
            }

            // 2. 取得單筆資料
            TEntity t = await _GetInfoByIdQueryResult(id);

            // 3. 有資料時, 轉換成指定格式並回傳
            if (t != null)
                return _controller.GetResponseJson(_controller.GetInfoByIdConvertEntityToResponse(t));

            // 4. 無資料時, 回傳錯誤
            _controller.AddError(GetInfoByIdNotFound);
            return _controller.GetResponseJson();
        }

        private async Task<TEntity> _GetInfoByIdQueryResult(int id)
        {
            // 取得實作者的查詢，並檢查刪除狀態
            return await CommonEntityHelper<TEntity>.FilterDeletedIfHasFlag(_controller.GetInfoByIdQuery(id)).FirstOrDefaultAsync();
        }

        #endregion
    }
}