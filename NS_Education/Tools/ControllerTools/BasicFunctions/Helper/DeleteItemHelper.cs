using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper
{
    /// <summary>
    /// DeleteItem 功能的預設處理工具。
    /// </summary>
    /// <typeparam name="TController">Controller 類型</typeparam>
    /// <typeparam name="TEntity">掌管資料類型</typeparam>
    public class DeleteItemHelper<TController, TEntity> : IDeleteItemHelper
        where TController : PublicClass, IDeleteItem<TEntity>
        where TEntity : class
    {
        private readonly TController _controller;

        public DeleteItemHelper(TController controller)
        {
            _controller = controller;
        }

        #region DeleteItem

        private const string DeleteItemNotSupported = "此 Controller 的資料型態不支援設定刪除狀態功能！";

        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="input">輸入資料，<see cref="DeleteItem_Input_APIItem"/></param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            if (!FlagHelper<TEntity>.HasDeleteFlag)
                throw new NotSupportedException(DeleteItemNotSupported);

            // 1. 驗證輸入。
            // 驗證集合是否皆為獨特 Id
            bool isCollectionValid = input.Items.StartValidate()
                .Validate(items => items.GroupBy(i => i.Id).Count() == input.Items.Count(),
                    () => _controller.AddError(_controller.CopyNotAllowed("欲刪除的資料 ID",
                        nameof(DeleteItem_Input_Row_APIItem.Id))))
                .IsValid();

            // 驗證每個元素是否輸入正確

            bool isEveryElementValid = input.Items.StartValidateElements()
                .Validate(i => i.Id != null && i.Id.IsAboveZero(),
                    () => _controller.AddError(_controller.EmptyNotAllowed("欲刪除的資料 ID",
                        nameof(DeleteItem_Input_Row_APIItem.Id))))
                .Validate(i => i.DeleteFlag != null,
                    i => _controller.AddError(_controller.EmptyNotAllowed($"欲刪除或復活的輸入值",
                        nameof(DeleteItem_Input_Row_APIItem.DeleteFlag))))
                .IsValid();

            if (!isCollectionValid || !isEveryElementValid)
                return _controller.GetResponseJson();

            // 驗證每個元素是否都存在於 Db
            // ReSharper disable once PossibleInvalidOperationException
            // 建立 ID : entity 的字典，方便之後對照輸入資料
            var data = (await _controller.DeleteItemsQuery(input.Items.Select(i => i.Id.Value))
                    .ToArrayAsync())
                .ToDictionary(t => _controller.DC.GetPrimaryKeyFromEntity(t), t => t);

            // 前面已經確認過每個元素都是獨特的，所以當這裡的數量不同時，表示有輸入對應不到資料（查無資料）。
            if (data.Keys.Count != input.Items.Count())
            {
                _controller.AddError(_controller.NotFound());
                return _controller.GetResponseJson();
            }

            // 2. 更新刪除狀態，並存入 DB。
            try
            {
                foreach (DeleteItem_Input_Row_APIItem i in input.Items)
                {
                    // 前面驗證已經確認僱所有輸入值都不是 null
                    // ReSharper disable PossibleInvalidOperationException
                    FlagHelper.SetDeleteFlag(data[i.Id.Value], i.DeleteFlag.Value);
                }

                await _controller.DC.SaveChangesStandardProcedureAsync(_controller.GetUid(),
                    HttpContext.Current.Request);
            }
            catch (Exception e)
            {
                _controller.AddError(_controller.UpdateDbFailed(e));
            }

            // 3. 回傳通用回傳訊息格式。
            return _controller.GetResponseJson();
        }

        public async Task<bool> DeleteItemValidateReservation<TReservationEntity>(DeleteItem_Input_APIItem input,
            IDeleteItemValidateReservation<TReservationEntity> validation)
            where TReservationEntity : class
        {
            HashSet<int> uniqueDeleteId = input.GetUniqueDeleteId();

            IQueryable<Resver_Head> basicQuery = _controller.DC.Resver_Head
                .Where(ResverHeadExpression.IsOngoingExpression);

            TReservationEntity[] cantDeleteData = await validation
                .SupplyQueryWithInputIdCondition(basicQuery, uniqueDeleteId)
                .AsNoTracking()
                .ToArrayAsync();

            // 整合成單個錯誤訊息
            IDictionary<object, string> errors = cantDeleteData
                .GroupBy(validation.GetInputId)
                .OrderBy(grouping => grouping.Key)
                .ToDictionary(grouping => grouping.Key, grouping =>
                    DeleteItemValidateReservationCombineHeadId(validation, grouping));

            foreach (KeyValuePair<object, string> kvp in errors)
            {
                _controller.AddError(_controller.NotSupportedValue($"欲刪除的資料 ID（{kvp.Key}）",
                    nameof(DeleteItem_Input_Row_APIItem.Id),
                    $"已存在進行中的預約單（單號 {kvp.Value}）"));
            }

            return !cantDeleteData.Any();
        }

        private static string DeleteItemValidateReservationCombineHeadId<TReservationEntity>(
            IDeleteItemValidateReservation<TReservationEntity> validation,
            IGrouping<object, TReservationEntity> grouping) where TReservationEntity : class
        {
            // 把預約單 ID 組成最大不超過一定數量的一串字串，並視必要加上 ... 符號
            return String.Join(", ",
                       grouping.Select(validation.GetHeadId).Take(IoConstants.DeleteItemHeadIdMaxCount))
                   + (grouping.Count() > IoConstants.DeleteItemHeadIdMaxCount ? "..." : "");
        }

        #endregion
    }
}