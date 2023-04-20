using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    public class GetListHelper<TController, TEntity, TGetListRequest, TGetListRow>
        where TController : PublicClass, IGetList<TEntity, TGetListRequest, TGetListRow>
        where TEntity : class
        where TGetListRequest : BaseRequestForList
        where TGetListRow : class
    {
        private readonly TController _controller;

        public GetListHelper(TController controller)
        {
            _controller = controller;
        }

        #region GetList

        private static string GetListNotFound => $"{typeof(TController).Name} GetList 時查無資料！";

        /// <summary>
        /// 取得列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗，或找不到資料時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(TGetListRequest input)
        {
            // 1. 驗證輸入
            bool inputValidated = await _controller.GetListValidateInput(input);

            if (!inputValidated)
                return _controller.GetResponseJson();

            // 2. 執行查詢
            var response = new BaseResponseForList<TGetListRow>();
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
            , BaseResponseForList<TGetListRow> response)
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