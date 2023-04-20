using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

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

        public async Task<string> GetListLocal()
        {
            BaseResponseForList<TGetListRow> response = new BaseResponseForList<TGetListRow>
            {
                Items = await _controller.GetListLocalResults()
            };

            return _controller.GetResponseJson(response);
        }
    }
}