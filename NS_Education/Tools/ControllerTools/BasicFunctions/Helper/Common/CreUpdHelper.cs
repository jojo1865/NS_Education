using System;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Variables;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Common
{
    /// <summary>
    /// 針對 CreUID、CreDate、UpdUID、UpdDate 等欄位，進行預設處理的工具。
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

        #endregion
    }
}