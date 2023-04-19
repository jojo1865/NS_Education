using System;
using NS_Education.Variables;

namespace NS_Education.Controllers.BaseClass.Helper
{
    internal static class CommonControllerHelper<TController, TEntity> 
        where TController : PublicClass
        where TEntity : class
    {
        #region 通用
        
        internal static void SetInfosOnUpdate(TController controller, TEntity t)
        {
            if (CommonEntityHelper<TEntity>.HasUpdUid)
                CommonEntityHelper<TEntity>.SetProperty(t, DbConstants.UpdUid, controller.GetUid());

            if (CommonEntityHelper<TEntity>.HasUpdDate)
                CommonEntityHelper<TEntity>.SetProperty(t, DbConstants.UpdDate, DateTime.Now);
        }

        internal static void SetInfosOnCreate(TController controller, TEntity t)
        {
            if (CommonEntityHelper<TEntity>.HasCreUid)
                CommonEntityHelper<TEntity>.SetProperty(t, DbConstants.CreUid, controller.GetUid());

            if (CommonEntityHelper<TEntity>.HasCreDate)
                CommonEntityHelper<TEntity>.SetProperty(t, DbConstants.CreDate, DateTime.Now);

            if (CommonEntityHelper<TEntity>.HasUpdUid)
                CommonEntityHelper<TEntity>.SetProperty(t, DbConstants.UpdUid, 0);

            if (CommonEntityHelper<TEntity>.HasCreDate)
                CommonEntityHelper<TEntity>.SetProperty(t, DbConstants.UpdDate, DateTime.Now);
        }

        #endregion
    }
}