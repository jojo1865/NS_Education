using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers
{
    /// <summary>
    /// 靜態參數的 Controller。
    /// </summary>
    public class StaticCodeController : PublicClass, IGetTypeList<B_StaticCode>
    {
        private readonly IGetTypeListHelper _helper;

        public StaticCodeController()
        {
            _helper = new GetTypeListHelper<StaticCodeController, B_StaticCode>(this);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetTypeList()
        {
            return await _helper.GetTypeList();
        }

        public IOrderedQueryable<B_StaticCode> GetTypeListQuery()
        {
            return DC.B_StaticCode
                .Where(sc => sc.CodeType == 0 && sc.ActiveFlag)
                .OrderBy(sc => sc.SortNo);
        }

        public async Task<BaseResponseRowForType> GetTypeListEntityToRow(B_StaticCode entity)
        {
            return await Task.FromResult(new BaseResponseRowForType
            {
                ID = entity.SortNo,
                Title = entity.Title
            });
        }
    }
}