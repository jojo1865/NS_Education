using System.Threading.Tasks;
using System.Web;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    public class GetListLocalHelper<TController, TGetListRow> : IGetListLocalHelper
        where TController : PublicClass, IGetListLocal<TGetListRow>
        where TGetListRow : class
    {
        private readonly TController _controller;

        public GetListLocalHelper(TController controller)
        {
            _controller = controller;
        }
        
        #region GetListLocal

        public async Task<string> GetListLocal()
        {
            // 寫一筆 UserLog
            await _controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, _controller.GetUid(), HttpContext.Current.Request);
            
            BaseResponseForList<TGetListRow> response = new BaseResponseForList<TGetListRow>
            {
                Items = await _controller.GetListLocalResults()
            };
            
            return _controller.GetResponseJson(response);
        }
        
        #endregion
    }
}