using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NS_Education.Models.APIItems;
using NS_Education.Models.Errors.InputValidationErrors;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// GetList 功能的預設處理工具，處理有分頁的回傳。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    /// <typeparam name="TGetListRequest">傳入物件類型</typeparam>
    /// <typeparam name="TGetListRow">回傳時，List 中子物件的類型</typeparam>
    public class
        GetListPagedHelper<TController, TEntity, TGetListRequest, TGetListRow> : IGetListPagedHelper<TGetListRequest>
        where TController : PublicClass, IGetListPaged<TEntity, TGetListRequest, TGetListRow>
        where TEntity : class
        where TGetListRequest : BaseRequestForPagedList
        where TGetListRow : IGetResponseRow
    {
        private readonly TController _controller;

        public GetListPagedHelper(TController controller)
        {
            _controller = controller;
        }

        #region GetPagedList

        public async Task<string> GetPagedList(TGetListRequest input)
        {
            // 1. 驗證輸入
            bool inputValidated = await _controller.GetListPagedValidateInput(input);

            if (!inputValidated && !_controller.HasError())
                _controller.AddError(new WrongFormatError());

            if (_controller.HasError())
                return _controller.GetResponseJson();

            // 2. 執行查詢
            var response = new CommonResponseForPagedList<TGetListRow>();
            response.SetByInput(input);

            (int startIndex, IList<TEntity> results) queryResult = await _GetListQueryResult(input, response);

            // 3. 有錯誤時提早返回
            if (_controller.HasError())
                return _controller.GetResponseJson();

            // 4. 寫一筆 UserLog
            await _controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, _controller.GetUid(),
                HttpContext.Current.Request);

            // 5. 按指定格式回傳結果
            // 如果實作者有再用 DB 查值，會造成多重 Connection 異常，所以這邊不能使用 Task.WhenAll。（如：取得 Username）
            List<TGetListRow> rows = new List<TGetListRow>();
            foreach (var entity in queryResult.results)
            {
                var row = Task.Run(() => _controller.GetListPagedEntityToRow(entity)).Result;
                row.SetIndex(queryResult.startIndex++);
                await row.SetInfoFromEntity(entity, _controller);

                // 在這裡才實作 ReverseOrder，比較好計算 index 的值
                if (input.ReverseOrder)
                    rows.Insert(0, row);
                else
                    rows.Add(row);
            }

            response.Items = rows;

            return _controller.GetResponseJson(response);
        }

        private async Task<(int, IList<TEntity>)> _GetListQueryResult(TGetListRequest input
            , CommonResponseForPagedList<TGetListRow> response)
        {
            IQueryable<TEntity> query = _controller.GetListPagedOrderedQuery(input);

            // Filter by ActiveFlag
            if (input.ActiveFlag.IsInBetween(0, 1))
                query = FlagHelper.FilterByInputActiveFlag(query, input.ActiveFlag == 1);

            // Filter by DeleteFlag
            query = FlagHelper.FilterByInputDeleteFlag(query, input.DeleteFlag == 1);

            // 1. 先取得總筆數

            int totalRows = await query.CountAsync();
            response.AllItemCt = totalRows;

            // 2. 再回傳實際資料

            // 當查詢頁數超出總頁數時，直接回傳空清單
            if (totalRows == 0 || input.NowPage > response.AllPageCt)
                return (0, new List<TEntity>());

            (int skip, int take) = input.CalculateSkipAndTake(totalRows);

            var resultList = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (skip, resultList);
        }

        #endregion
    }
}