using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.ContactType.GetList;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper
{
    public class ContactTypeController :
        PublicClass,
        IGetListLocal<ContactType_GetList_Output_Row_APIItem>
    {
        private readonly IGetListLocalHelper _getListLocalHelper;

        public ContactTypeController()
        {
            _getListLocalHelper =
                new GetListLocalHelper<ContactTypeController, ContactType_GetList_Output_Row_APIItem>(this);
        }

        #region GetList

        private static readonly ICollection<ContactType_GetList_Output_Row_APIItem> ContactTypes =
            new List<ContactType_GetList_Output_Row_APIItem>
            {
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = (int)ContactType.Phone,
                    Title = "電話"
                },
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = (int)ContactType.Fax,
                    Title = "傳真"
                },
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = (int)ContactType.Mobile,
                    Title = "手機"
                },
                new ContactType_GetList_Output_Row_APIItem
                {
                    ID = (int)ContactType.Line,
                    Title = "LINE"
                },
            }.AsReadOnly();

        /// <summary>
        /// 提供給其他需要用到通訊方式的功能用。透過 Id 取得通訊方式名稱，但當 Id 超出範圍時回傳 null。
        /// </summary>
        /// <param name="contactTypeId">Id</param>
        /// <returns>
        /// 有此值的資料時：回傳該資料 Title。<br/>
        /// 無此值的資料時：回傳 null。
        /// </returns>
        public static string GetContactTypeTitle(int contactTypeId)
        {
            return contactTypeId >= 0 && ContactTypes.Count > contactTypeId
                ? ContactTypes.ElementAt(contactTypeId).Title
                : null;
        }

        /// <summary>
        /// 提供給其他需要用到通訊方式下拉選單的功能用。
        /// </summary>
        /// <returns>通訊方式的選單</returns>
        public static ICollection<ContactType_GetList_Output_Row_APIItem> GetContactTypeList()
        {
            return ContactTypes;
        }

        /// <summary>
        /// 提供給其他需要用到通訊方式下拉可選選單的功能用。
        /// </summary>
        /// <returns>通訊方式的可選選單</returns>
        public static ICollection<CommonResponseRowForSelectable> GetContactTypeSelectable(int selectId)
        {
            return ContactTypes.Select(ct => new CommonResponseRowForSelectable
            {
                ID = ct.ID,
                Title = ct.Title ?? "",
                SelectFlag = ct.ID == selectId
            }).ToList();
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            return await _getListLocalHelper.GetListLocal();
        }

        public async Task<ICollection<ContactType_GetList_Output_Row_APIItem>> GetListLocalResults()
        {
            return await Task.FromResult(ContactTypes);
        }

        #endregion
    }
}