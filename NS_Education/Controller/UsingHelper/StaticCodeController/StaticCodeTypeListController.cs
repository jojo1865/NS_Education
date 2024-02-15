using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.StaticCode.GetTypeList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

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
            // 如果是樓層別，需要依據代號來排序
            // 排序方式比較特別，無法用 EF LINQ 表示

            if (input.Id == (int)StaticCodeType.Floor)
            {
                CommonResponseForList<StaticCode_GetTypeList_Output_Row_APIItem>
                    results = await GetFloorResponse(input);
                return GetResponseJson(results);
            }

            // 其他情況，用 helper
            return await _getListAllHelper.GetAllList(input);
        }

        private async Task<CommonResponseForList<StaticCode_GetTypeList_Output_Row_APIItem>> GetFloorResponse(
            StaticCode_GetTypeList_Input_APIItem input)
        {
            // 查回來並變形

            StaticCode_GetTypeList_Output_Row_APIItem[] results = (await GetListAllOrderedQuery(input).ToArrayAsync())
                .Select(Transform)

                // 1. 開頭為數字升序
                // 2. 開頭不為數字時降序
                // 3. 開頭為數字的資料優先顯示
                .OrderByDescending(r =>
                {
                    Decimal.TryParse(new string(r.Title
                        .Replace('B', '-')
                        .Where(c => Char.IsDigit(c) || c == '-')
                        .ToArray()), out decimal parsed);

                    return parsed;
                })
                .ThenBy(r => r.Title)
                .ToArray();

            return new CommonResponseForList<StaticCode_GetTypeList_Output_Row_APIItem>
            {
                Items = results
            };
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
            return await Task.FromResult(Transform(entity));
        }

        private static StaticCode_GetTypeList_Output_Row_APIItem Transform(B_StaticCode entity)
        {
            return new StaticCode_GetTypeList_Output_Row_APIItem
            {
                BSCID = entity.BSCID,
                CodeType = entity.CodeType,
                Code = entity.Code ?? "",
                Title = entity.Title,
                SortNo = entity.SortNo
            };
        }

        #endregion
    }
}