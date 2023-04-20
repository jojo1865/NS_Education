using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// GetList 功能的預設處理工具。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    /// <typeparam name="TGetListRequest">傳入物件類型</typeparam>
    /// <typeparam name="TGetListRow">回傳時，List 中子物件的類型</typeparam>
    public class GetPagedListHelper<TController, TEntity, TGetListRequest, TGetListRow> : IGetPagedListHelper<TGetListRequest>
        where TController : PublicClass, IGetPagedList<TEntity, TGetListRequest, TGetListRow>
        where TEntity : class
        where TGetListRequest : BaseRequestForList
        where TGetListRow : class
    {
        private readonly TController _controller;

        public GetPagedListHelper(TController controller)
        {
            _controller = controller;
        }

        #region GetPagedList

        private static string GetListNotFound => $"{typeof(TController).Name} GetList 時查無資料！";
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetPagedList(TGetListRequest input)
        {
            // 1. 驗證輸入
            bool inputValidated = await _controller.GetListValidateInput(input);

            if (!inputValidated)
                return _controller.GetResponseJson();

            // 2. 執行查詢
            var response = new BaseResponseForPagedList<TGetListRow>();
            response.SetByInput(input);

            var queryResult = await _GetListQueryResult(input, response);

            // 3. 按照格式回傳結果
            if (!queryResult.Any() || _controller.HasError())
            {
                _controller.AddError(GetListNotFound);
                return _controller.GetResponseJson();
            }

            TGetListRow[] rows =
                await Task.WhenAll(queryResult.Select(async c => await _controller.GetListEntityToRow(c)));
            response.Items = rows.ToList();

            return _controller.GetResponseJson(response);
        }

        private async Task<IList<TEntity>> _GetListQueryResult(TGetListRequest t
            , BaseResponseForPagedList<TGetListRow> response)
        {
            IQueryable<TEntity> query = FlagHelper.FilterDeletedIfHasFlag(_controller.GetListOrderedQuery(t));

            // 1. 先取得總筆數

            int totalRows = await query.CountAsync();
            response.AllItemCt = totalRows;

            // 2. 再回傳實際資料

            return await query
                .Skip(t.GetStartIndex())
                .Take(t.GetTakeRowCount())
                .ToListAsync();
        }

        #endregion
    }
}