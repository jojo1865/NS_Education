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
    public class GetListUniqueFieldHelper<TController, TEntity> : IGetListAllHelper<CommonRequestForUniqueField>
        where TController : PublicClass, IGetListUniqueField<TEntity>
        where TEntity : class
    {
        public GetListUniqueFieldHelper(TController controller)
        {
            Controller = controller;
        }

        private TController Controller { get; }

        /// <inheritdoc />
        public async Task<string> GetAllList(CommonRequestForUniqueField input)
        {
            // 1. 驗證輸入
            if (!ValidateInput(input))
                return Controller.GetResponseJson();

            // 2. 查詢資料
            ICollection<string> names = await QueryUniqueNames(input);

            // 3. 寫一筆 UserLog
            await Controller.DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, Controller.GetUid(),
                HttpContext.Current.Request);

            // 4. 回傳資料
            CommonResponseForList<string> response = new CommonResponseForList<string>
            {
                Items = names
            };

            return Controller.GetResponseJson(response);
        }

        private async Task<ICollection<string>> QueryUniqueNames(CommonRequestForUniqueField input)
        {
            IQueryable<TEntity> query = Controller.DC.Set<TEntity>().AsQueryable();
            IOrderedQueryable<TEntity> orderedQuery = Controller.GetListUniqueNamesOrderQuery(query);

            // Filter by DeleteFlag
            IQueryable<TEntity> applyDeleteFlagQuery =
                FlagHelper.FilterByInputDeleteFlag(orderedQuery, input.DeleteFlag == 1);

            IQueryable<string> filteredQuery =
                applyDeleteFlagQuery
                    .Select(Controller.GetListUniqueNamesQueryExpression());

            if (input.Keyword.HasContent())
                filteredQuery = filteredQuery.Where(s => s.Contains(input.Keyword));

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