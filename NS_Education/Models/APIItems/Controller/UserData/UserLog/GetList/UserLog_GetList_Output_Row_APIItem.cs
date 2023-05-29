using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList
{
    public class UserLog_GetList_Output_Row_APIItem : IGetResponseRow
    {
        public string Time { get; set; }
        public string Actor { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }

        [JsonIgnore] internal DateTime CreDate { get; set; }

        /// <inheritdoc />
        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            // 這個物件沒有任何普遍性欄位需要設定。
            return Task.CompletedTask;
        }
    }
}