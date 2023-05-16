using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseGetResponseRowWithCreDateOnly : IGetResponseRow
    {
        public async Task SetInfoFromEntity<T>(T entity, PublicClass controller)
            where T : class
        {
            await _SetInfoFromEntity(entity, controller);
        }

        private Task _SetInfoFromEntity<TEntity>(TEntity entity, PublicClass controller)
        {
            CreDate = entity.GetIfHasProperty<TEntity, DateTime?>(DbConstants.CreDate) is DateTime dt ? dt.ToFormattedStringDateTime() : null;
            return Task.CompletedTask;
        }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreDate { get; private set; }
    }
}