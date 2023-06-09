using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Models.APIItems.Controller.MenuData.MenuApi.GetListByUid
{
    public class MenuData_GetListByUid_Output_Node_APIItem : IGetResponseRow
    {
        [JsonIgnore] public MenuData_GetListByUid_Output_Node_APIItem Parent { get; set; }

        public int MDID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int SortNo { get; set; }

        [JsonIgnore] public int Index { get; private set; }

        public IList<MenuData_GetListByUid_Output_Node_APIItem> Items { get; set; } =
            new List<MenuData_GetListByUid_Output_Node_APIItem>();

        public IList<MenuData_GetListByUid_Output_MenuApi_APIItem> Apis { get; set; } =
            new List<MenuData_GetListByUid_Output_MenuApi_APIItem>();

        public Task SetInfoFromEntity<T>(T entity, PublicClass controller) where T : class
        {
            // 不做任何事，這個輸出不包含任何普遍性欄位。
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void SetIndex(int index)
        {
            Index = index;
        }
    }
}