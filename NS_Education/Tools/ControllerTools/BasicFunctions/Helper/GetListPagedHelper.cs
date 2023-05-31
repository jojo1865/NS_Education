using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NS_Education.Models.APIItems;
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

        private const string GetPagedListInputIncorrect = "輸入格式錯誤或缺少欄位，請檢查資料內容！";

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
            , BaseResponseForPagedList<TGetListRow> response)
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
            // 如果是倒序時，
            // |- a. 起始 index: 由最後減回來，並多減一頁的筆數
            // +- b. 取的筆數: index 大於等於 0 時，照一般處理；否則，加上 index（如 index 為 -2，表示向左溢出 2 筆，即最後一頁只有 n-2 筆）
            int startIndex = input.ReverseOrder
                ? GetEndIndex(input, totalRows) - input.GetTakeRowCount() + 1
                : GetStartIndex(input); // 0-index - 1-index, 作補正

            // 最多只拿到 TakeRowCount 筆；當 startIndex 小於 0 時（最後一頁資料不滿 TakeRowCount 筆時），加上 startIndex 作為補正
            int takeRow = input.ReverseOrder
                ? input.GetTakeRowCount() + Math.Min(0, startIndex)
                : input.GetTakeRowCount();

            // 如果 takeRow 小於等於 0，表示查不到資料，直接返回

            if (takeRow <= 0)
                return (0, new List<TEntity>());

            // 把小於 0 的 startIndex 轉成 0
            startIndex = Math.Max(0, startIndex);

            var resultList = await query
                .Skip(startIndex)
                .Take(takeRow)
                .ToListAsync();

            return (startIndex, resultList);
        }

        private static int GetStartIndex(TGetListRequest input)
        {
            return input.GetStartIndex();
        }

        private static int GetEndIndex(TGetListRequest input, int totalRows)
        {
            return totalRows - input.GetStartIndex() - 1;
        }

        #endregion
    }
}