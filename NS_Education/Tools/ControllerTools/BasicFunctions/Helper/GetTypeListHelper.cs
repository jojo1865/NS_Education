using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

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
            
            BaseResponseRowForType[] rows = 
                await Task.WhenAll(queryResult.Select(async entity => await _controller.GetTypeListEntityToRow(entity)));
            
            // 2. 轉換為 BaseResponseForList
            BaseResponseForList<BaseResponseRowForType> response = new BaseResponseForList<BaseResponseRowForType>
            {
                Items = rows.ToList()
            };
            
            // 3. 回傳
            return _controller.GetResponseJson(response);

        }
    }
}