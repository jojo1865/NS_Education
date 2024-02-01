using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BeingValidated;
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
    public class
        GetListUniqueFieldHelper<TController, TEntity, TResult> : IGetListAllHelper<CommonRequestForUniqueField>
        where TController : PublicClass, IGetListUniqueField<TEntity, TResult>
        where TEntity : class
        where TResult : class
    {
        public GetListUniqueFieldHelper(TController controller)
        {
            Controller = controller;
        }

        private TController Controller { get; }

        /// <inheritdoc />
        public async Task<string> GetAllList(CommonRequestForUniqueField input)
        {
            // 1. 查詢資料
            ICollection<TResult> fields = await GetRows(input);

            // 2. 寫一筆 UserLog
            await Controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, Controller.GetUid(),
                HttpContext.Current.Request);

            // 3. 回傳資料
            CommonResponseForList<TResult> response = new CommonResponseForList<TResult>
            {
                Items = fields ?? new List<TResult>()
            };

            return Controller.GetResponseJson(response);
        }

        /// <inheritdoc />
        public async Task<ICollection<TRow>> GetRows<TRow>(CommonRequestForUniqueField input)
        {
            return (ICollection<TRow>)await GetRows(input);
        }

        private async Task<ICollection<TResult>> GetRows(CommonRequestForUniqueField input)
        {
            // 1. 驗證輸入
            if (!ValidateInput(input))
                return null;

            // 2. 查詢資料
            ICollection<TResult> fields = await QueryUniqueFields(input);

            return fields;
        }

        private async Task<ICollection<TResult>> QueryUniqueFields(CommonRequestForUniqueField input)
        {
            IQueryable<TEntity> query = Controller.DC.Set<TEntity>().AsQueryable();
            IOrderedQueryable<TEntity> orderedQuery = Controller.GetListUniqueFieldsOrderQuery(query);

            // Filter by DeleteFlag
            IQueryable<TEntity> applyDeleteFlagQuery =
                FlagHelper.FilterByInputDeleteFlag(orderedQuery, input.DeleteFlag == 1);

            IQueryable<TResult> filteredQuery =
                applyDeleteFlagQuery
                    .Select(Controller.GetListUniqueFieldsQueryExpression());

            if (input.Keyword.HasContent())
                filteredQuery = Controller.GetListUniqueFieldsApplyKeywordFilter(filteredQuery, input.Keyword);

            filteredQuery = filteredQuery
                .Distinct()
                .Take(input.MaxRow);

            return await filteredQuery.AsNoTracking().ToArrayAsync();
        }

        private bool ValidateInput(CommonRequestForUniqueField input)
        {
            return input.StartValidate()
                .Validate(i => i.DeleteFlag.IsInBetween(0, 1),
                    i => Controller.AddError(new NotSupportedValueError("過濾是否已刪除的資料", nameof(input.DeleteFlag))))
                .Validate(i => i.MaxRow.IsInBetween(1, 1000),
                    i => Controller.AddError(new ValueOutOfRangeError("最大筆數", nameof(input.MaxRow), 1, 1000)))
                .IsValid();
        }
    }
}