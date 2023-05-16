using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseGetResponseRowWithCreUpd : IGetResponseRow
    {
        public async Task SetInfoFromEntity<T>(T entity, PublicClass controller)
            where T : class
        {
            await _SetInfoFromEntity(entity, controller);
        }

        private async Task _SetInfoFromEntity<TEntity>(TEntity entity, PublicClass controller)
        {
            ActiveFlag = entity.GetIfHasProperty<TEntity, bool?>(DbConstants.ActiveFlag);
            
            CreDate = entity.GetIfHasProperty<TEntity, DateTime?>(DbConstants.CreDate) is DateTime creDate ? creDate.ToFormattedStringDateTime() : null;
            CreUID = entity.GetIfHasProperty<TEntity, int?>(DbConstants.CreUid);
            CreUser = CreUID is int creUid ? await controller.GetUserNameByID(creUid) : null;
            
            UpdDate = entity.GetIfHasProperty<TEntity, DateTime?>(DbConstants.UpdDate) is DateTime updDate ? updDate.ToFormattedStringDateTime() : null;
            UpdUID = entity.GetIfHasProperty<TEntity, int?>(DbConstants.UpdUid);
            // 當更新者和新增者一樣時，不多做查詢
            if (UpdUID == CreUID)
                UpdUser = CreUser;
            else
                UpdUser = UpdUID is int updUid ? await controller.GetUserNameByID(updUid) : null;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ActiveFlag { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreDate { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreUser { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CreUID { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UpdDate { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UpdUser { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? UpdUID { get; private set; }
    }
}