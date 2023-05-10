using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    public class GetTypeListHelper<TController, TEntity> : IGetTypeListHelper
      where TController : PublicClass, IGetTypeList<TEntity>
      where TEntity : class
    {
        private readonly TController _controller;

        public GetTypeListHelper(TController controller)
        {
            _controller = controller;
        }
        
        public async Task<string> GetTypeList()
        {
            // 1. 找出所有類別的 ID 與名稱
            List<TEntity> queryResult = await FlagHelper.FilterDeletedIfHasFlag(_controller.GetTypeListQuery()).ToListAsync();

            if (!queryResult.Any() || _controller.HasError())
                return _controller.GetResponseJson();

            // 2. 轉換為 BaseResponseForList
            BaseResponseForList<BaseResponseRowIdTitle> response = new BaseResponseForList<BaseResponseRowIdTitle>
            {
                // 如果實作者有再用 DB 查值，會造成多重 Connection 異常，所以這邊不能使用 Task.WhenAll。
                Items = queryResult.Select(entity => Task.Run(() => _controller.GetTypeListEntityToRow(entity)).Result).ToList()
            };
            
            // 3. 寫一筆 UserLog
            await _controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, _controller.GetUid());
            
            // 4. 回傳
            return _controller.GetResponseJson(response);

        }
    }
}