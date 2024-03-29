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
    /// GetList 功能的預設處理工具。處理無分頁的整批資料回傳。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    /// <typeparam name="TGetListRequest">傳入物件類型</typeparam>
    /// <typeparam name="TGetListRow">回傳時，List 中子物件的類型</typeparam>
    public class
        GetListAllHelper<TController, TEntity, TGetListRequest, TGetListRow> : IGetListAllHelper<TGetListRequest>
        where TController : PublicClass, IGetListAll<TEntity, TGetListRequest, TGetListRow>
        where TEntity : class
        where TGetListRequest : BaseRequestForList
        where TGetListRow : class
    {
        private readonly TController _controller;

        public GetListAllHelper(TController controller)
        {
            _controller = controller;
        }

        #region GetAllList

        public async Task<string> GetAllList(TGetListRequest input)
        {
            ICollection<TGetListRow> rows = await GetRows(input);

            CommonResponseForList<TGetListRow> response = new CommonResponseForList<TGetListRow>
            {
                Items = rows ?? new List<TGetListRow>()
            };

            return _controller.GetResponseJson(response);
        }

        /// <inheritdoc />
        public async Task<ICollection<TRow>> GetRows<TRow>(TGetListRequest input)
        {
            return (ICollection<TRow>)await GetRows(input);
        }

        private async Task<ICollection<TGetListRow>> GetRows(TGetListRequest input)
        {
            // 1. 驗證輸入
            bool inputValidated = await _controller.GetListAllValidateInput(input);

            if (!inputValidated && !_controller.HasError())
                _controller.AddError(new WrongFormatError());

            if (_controller.HasError())
                return null;

            // 2. 執行查詢
            var queryResult = await _GetListQueryResult(input);

            // 3. 有錯誤時提早返回
            if (_controller.HasError())
                return null;

            // 4. 寫一筆 UserLog
            await _controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, _controller.GetUid(),
                HttpContext.Current.Request);

            // 5. 按指定格式回傳結果
            // 如果實作者有再用 DB 查值，會造成多重 Connection 異常，所以這邊不能使用 Task.WhenAll。（如：取得 Username）
            ICollection<TGetListRow> rows = new List<TGetListRow>();
            int index = 0;
            foreach (var entity in queryResult)
            {
                var row = Task.Run(() => _controller.GetListAllEntityToRow(entity)).Result;

                if (row is IGetResponseRow getResponseRow)
                {
                    await getResponseRow.SetInfoFromEntity(entity, _controller);
                    getResponseRow.SetIndex(index++);
                }

                rows.Add(row);
            }

            rows = rows.SortWithInput(input.Sorting).ToList();

            return rows;
        }

        private async Task<IList<TEntity>> _GetListQueryResult(TGetListRequest input)
        {
            IQueryable<TEntity> query = _controller.GetListAllOrderedQuery(input);

            // Filter by ActiveFlag
            if (input.ActiveFlag.IsInBetween(0, 1))
                query = FlagHelper.FilterByInputActiveFlag(query, input.ActiveFlag == 1);

            // Filter by DeleteFlag
            query = FlagHelper.FilterByInputDeleteFlag(query, input.DeleteFlag == 1);

            // 回傳實際資料

            return await query.ToListAsync();
        }

        #endregion
    }
}