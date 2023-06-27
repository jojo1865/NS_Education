using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

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

        private static string UpdateFailed(Exception e)
            => e is null ? "更新 DB 時失敗！" : $"更新 DB 時失敗：{e.Message}；Inner:{e.InnerException?.Message}！";

        private const string DeleteItemRepeating = "欲刪除的 ID 存在重複資料，請確認每個 ID 只輸入一次！";
        private const string DeleteItemNotSupported = "此 Controller 的資料型態不支援設定刪除狀態功能！";
        private const string DeleteItemInputIdIncorrect = "未輸入欲刪除或復活的資料 ID 或是格式不正確！";

        private static string DeleteItemInputDeleteFlagIncorrect(int? id)
            => id != null ? $"ID {id} 未輸入欲刪除或復活，或是格式不正確！" : "其中一筆輸入資料未輸入欲刪除或復活，或是格式不正確！";

        private const string DeleteItemNotFound = "其中一筆或多筆欲刪除或復活的 ID 查無資料！";

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
                    () => _controller.AddError(DeleteItemRepeating))
                .IsValid();

            // 驗證每個元素是否輸入正確

            bool isEveryElementValid = input.Items.StartValidateElements()
                .Validate(i => i.Id != null && i.Id.IsAboveZero(),
                    () => _controller.AddError(DeleteItemInputIdIncorrect))
                .Validate(i => i.DeleteFlag != null,
                    i => _controller.AddError(DeleteItemInputDeleteFlagIncorrect(i.Id)))
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
                _controller.AddError(DeleteItemNotFound);
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
                _controller.AddError(UpdateFailed(e));
            }

            // 3. 回傳通用回傳訊息格式。
            return _controller.GetResponseJson();
        }

        #endregion
    }
}