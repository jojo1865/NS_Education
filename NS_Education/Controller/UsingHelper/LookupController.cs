using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.Lookup.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    public class LookupController : PublicClass
    {
        /// <summary>
        /// 取得可編輯之對照檔列表的端點。
        /// </summary>
        /// <returns>通用訊息回傳格式</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            // 特例：因為結果需要進行 aggregate，無法使用 helper。

            IGrouping<string, Lookup>[] groups = await DC.Lookup
                .GroupBy(lu => lu.Category)
                .ToArrayAsync();

            ILookup<string, IGrouping<string, Lookup>> data = groups.ToLookup(lu => lu.Key, lu => lu);

            CommonResponseForList<Lookup_GetList_Category_APIItem> response =
                new CommonResponseForList<Lookup_GetList_Category_APIItem>();

            response.Items = data
                .Select(grouping => new Lookup_GetList_Category_APIItem
                {
                    Title = grouping.Key,
                    Lookups = grouping.SelectMany(g => g)
                        .Select(lu => new Lookup_GetList_Lookup_APIItem
                        {
                            Title = lu.Title ?? "",
                            IdPropertyName = lu.IdPropertyName ?? "",
                            EditPageRoute = lu.EditPageRoute ?? "/",
                            GetListEndpoint = lu.GetListEndpoint ?? "/",
                            SubmitEndpoint = lu.SubmitEndpoint ?? "/",
                            DeleteEndpoint = lu.DeleteEndpoint ?? "/",
                            GetEndpoint = lu.GetEndpoint ?? "/",
                            Field1Title = lu.Field1Title ?? "",
                            Field2Title = lu.Field2Title ?? "",
                            Field3Title = lu.Field3Title ?? "",
                            Field4Title = lu.Field4Title ?? "",
                            Field5Title = lu.Field5Title ?? "",
                            Field1PropertyName = lu.Field1PropertyName ?? "",
                            Field2PropertyName = lu.Field2PropertyName ?? "",
                            Field3PropertyName = lu.Field3PropertyName ?? "",
                            Field4PropertyName = lu.Field4PropertyName ?? "",
                            Field5PropertyName = lu.Field5PropertyName ?? ""
                        })
                })
                .ToList();
            return GetResponseJson(response);
        }
    }
}