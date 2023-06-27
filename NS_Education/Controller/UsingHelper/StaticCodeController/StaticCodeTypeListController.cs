using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.StaticCode.GetTypeList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.StaticCodeController
{
    /// <summary>
    /// 處理 StaticCode.GetTypeList 的 Controller。<br/>
    /// 雖然 Route 是 GetTypeList，實際上功能較符合 GetListAll。
    /// </summary>
    public class StaticCodeTypeListController : PublicClass,
        IGetListAll<B_StaticCode, StaticCode_GetTypeList_Input_APIItem, StaticCode_GetTypeList_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<StaticCode_GetTypeList_Input_APIItem> _getListAllHelper;

        public StaticCodeTypeListController()
        {
            _getListAllHelper =
                new GetListAllHelper<StaticCodeTypeListController, B_StaticCode, StaticCode_GetTypeList_Input_APIItem,
                    StaticCode_GetTypeList_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetTypeList

        // 實際 Route 請參考 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(StaticCode_GetTypeList_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(StaticCode_GetTypeList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.Id.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之類別 ID", nameof(input.Id))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_StaticCode> GetListAllOrderedQuery(StaticCode_GetTypeList_Input_APIItem input)
        {
            var query = DC.B_StaticCode.AsQueryable();

            if (input.Id.IsZeroOrAbove())
                query = query.Where(sc => sc.CodeType == input.Id);

            return query.OrderBy(sc => sc.SortNo)
                .ThenBy(sc => sc.Code.Length)
                .ThenBy(sc => sc.Code)
                .ThenBy(sc => sc.BSCID);
        }

        public async Task<StaticCode_GetTypeList_Output_Row_APIItem> GetListAllEntityToRow(B_StaticCode entity)
        {
            return await Task.FromResult(new StaticCode_GetTypeList_Output_Row_APIItem
            {
                BSCID = entity.BSCID,
                CodeType = entity.CodeType,
                Code = entity.Code ?? "",
                Title = entity.Title,
                SortNo = entity.SortNo
            });
        }

        #endregion
    }
}