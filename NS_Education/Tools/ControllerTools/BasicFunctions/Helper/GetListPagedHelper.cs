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
    public class GetListPagedHelper<TController, TEntity, TGetListRequest, TGetListRow> : IGetListPagedHelper<TGetListRequest>
        where TController : PublicClass, IGetListPaged<TEntity, TGetListRequest, TGetListRow>
        where TEntity : class
        where TGetListRequest : BaseRequestForList
        where TGetListRow : BaseResponseWithCreUpd<TEntity>
    {
        private readonly TController _controller;

        public GetListPagedHelper(TController controller)
        {
            _controller = controller;
        }

        #region GetPagedList

        private const string GetPagedListInputIncorrect = "輸入格式錯誤或缺少欄位，請檢查資料內容！";

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetPagedList(TGetListRequest input)
        {
            // 1. 驗證輸入
            bool inputValidated = await _controller.GetListPagedValidateInput(input);

            if (!inputValidated && !_controller.HasError())
                _controller.AddError(GetPagedListInputIncorrect);
            
            if (_controller.HasError())
                return _controller.GetResponseJson();

            // 2. 執行查詢
            var response = new BaseResponseForPagedList<TGetListRow>();
            response.SetByInput(input);

            var queryResult = await _GetListQueryResult(input, response);

            // 3. 有錯誤時提早返回
            if (_controller.HasError())
                return _controller.GetResponseJson();

            // 4. 按指定格式回傳結果
            // 如果實作者有再用 DB 查值，會造成多重 Connection 異常，所以這邊不能使用 Task.WhenAll。（如：取得 Username）
            List<TGetListRow> rows = new List<TGetListRow>();
            foreach (var entity in queryResult)
            {
                var row = Task.Run(() => _controller.GetListPagedEntityToRow(entity)).Result;
                await row.SetInfoFromEntity(entity, _controller);
                rows.Add(row);
            }

            response.Items = rows;

            return _controller.GetResponseJson(response);
        }

        private async Task<IList<TEntity>> _GetListQueryResult(TGetListRequest t
            , BaseResponseForPagedList<TGetListRow> response)
        {
            IQueryable<TEntity> query = FlagHelper.FilterDeletedIfHasFlag(_controller.GetListPagedOrderedQuery(t));

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