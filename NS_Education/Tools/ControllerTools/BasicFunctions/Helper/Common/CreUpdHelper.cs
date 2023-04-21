using System;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common
{
    /// <summary>
    /// 針對 CreUID、CreDate、UpdUID、UpdDate、DeleteFlag 等欄位，進行預設處理的工具。
    /// </summary>
    internal static class CreUpdHelper
    {
        #region 通用

        /// <summary>
        /// 針對更新資料時需要一併更新的普遍型欄位進行處理。
        /// </summary>
        /// <param name="controller">controller</param>
        /// <param name="t">對象資料</param>
        /// <typeparam name="TController">controller 的型態</typeparam>
        /// <typeparam name="TEntity">對象資料的型態</typeparam>
        internal static void SetInfosOnUpdate<TController, TEntity>(TController controller, TEntity t)
            where TController : PublicClass
            where TEntity : class
        {
            t.SetIfHasProperty(DbConstants.UpdUid, controller.GetUid());
            t.SetIfHasProperty(DbConstants.UpdDate, DateTime.Now);
        }

        /// <summary>
        /// 針對建立資料時需要一併建立的普遍型欄位進行處理。
        /// </summary>
        /// <param name="controller">controller</param>
        /// <param name="t">對象資料</param>
        /// <typeparam name="TController">controller 的型態</typeparam>
        /// <typeparam name="TEntity">對象資料的型態</typeparam>
        internal static void SetInfosOnCreate<TController, TEntity>(TController controller, TEntity t)
            where TController : PublicClass
            where TEntity : class
        {
            t.SetIfHasProperty(DbConstants.CreUid, controller.GetUid());
            t.SetIfHasProperty(DbConstants.CreDate, DateTime.Now);
            t.SetIfHasProperty(DbConstants.UpdUid, 0);
            t.SetIfHasProperty(DbConstants.UpdDate, DateTime.Now);
            t.SetIfHasProperty(DbConstants.DeleteFlag, false);
        }

        /// <summary>
        /// 協助 Controller 在將 DB Entity 轉換成 Resposne 所須物件時，設定部分普遍的值。
        /// </summary>
        /// <param name="entity">DB 物件</param>
        /// <param name="row">Response 物件</param>
        /// <param name="controller">Controller 物件</param>
        /// <typeparam name="TEntity">DB 物件的 Generic Type</typeparam>
        /// <typeparam name="TRow">Response 物件的 Generic Type</typeparam>
        /// <typeparam name="TController">Controller 物件的 Generic Type</typeparam>
        internal static TRow CopyInfosIntoRow<TEntity, TRow, TController>(TEntity entity, TRow row, TController controller)
            where TEntity : class
            where TRow : BaseResponseWithCreUpd<TEntity>
            where TController : PublicClass
        {
            row.ActiveFlag = entity.GetIfHasProperty<TEntity, bool>(DbConstants.ActiveFlag);
            
            row.CreDate = entity.GetIfHasProperty<TEntity, DateTime>(DbConstants.CreDate).ToFormattedString();
            row.CreUID = entity.GetIfHasProperty<TEntity, int>(DbConstants.CreUid);
            row.CreUser = Task.Run(() => controller.GetUserNameByID(row.CreUID)).Result;
            
            row.UpdDate = entity.GetIfHasProperty<TEntity, DateTime>(DbConstants.UpdDate).ToFormattedString();
            row.UpdUID = entity.GetIfHasProperty<TEntity, int>(DbConstants.UpdUid);
            row.UpdUser = Task.Run(() => controller.GetUserNameByID(row.UpdUID)).Result;

            return row;
        }
        
        #endregion
    }
}