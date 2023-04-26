using System;
using System.Threading.Tasks;
using NS_Education.Tools;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseResponseWithCreUpd<TEntity>
      where TEntity : class
    {
        /// <summary>
        /// 依據實際的 DB 物件，設置此欄位的基本欄位。<br/>
        /// 若使用 Helper 時，此方法會被自動呼叫，實作者可以省略。
        /// </summary>
        /// <param name="entity">DB 物件</param>
        /// <param name="controller">Controller 物件</param>
        public async Task SetInfoFromEntity(TEntity entity, PublicClass controller)
        {
            ActiveFlag = entity.GetIfHasProperty<TEntity, bool>(DbConstants.ActiveFlag);
            
            CreDate = entity.GetIfHasProperty<TEntity, DateTime>(DbConstants.CreDate).ToFormattedStringDateTime();
            CreUID = entity.GetIfHasProperty<TEntity, int>(DbConstants.CreUid);
            CreUser = CreUID == default ? "" : await controller.GetUserNameByID(CreUID);
            
            UpdDate = entity.GetIfHasProperty<TEntity, DateTime>(DbConstants.UpdDate).ToFormattedStringDateTime();
            UpdUID = entity.GetIfHasProperty<TEntity, int>(DbConstants.UpdUid);
            // 當更新者和新增者一樣時，不多做查詢
            if (UpdUID == CreUID)
                UpdUser = CreUser;
            else
                UpdUser = UpdUID == default ? "" : await controller.GetUserNameByID(UpdUID);
        }

        public bool ActiveFlag { get; private set; }
        
        public string CreDate { get; private set; }
        public string CreUser { get; private set; }
        public int CreUID { get; private set; }
        public string UpdDate { get; private set; }
        public string UpdUser { get; private set; }
        public int UpdUID { get; private set; }
    }
}