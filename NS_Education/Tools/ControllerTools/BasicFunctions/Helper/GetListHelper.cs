using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    public class GetListHelper<TController, TGetListRow> : IGetListHelper
        where TController : PublicClass, IGetList<TGetListRow>
        where TGetListRow : class
    {
        private readonly TController _controller;

        public GetListHelper(TController controller)
        {
            _controller = controller;
        }

        public async Task<string> GetList()
        {
            BaseResponseForList<TGetListRow> response = new BaseResponseForList<TGetListRow>
            {
                Items = await _controller.GetListResults()
            };

            return _controller.GetResponseJson(response);
        }
    }
}