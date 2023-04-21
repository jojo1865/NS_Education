using System.Linq;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.SiteData.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

namespace NS_Education.Controllers.SiteDataController
{
    public class SiteDataController : PublicClass,
        IGetListPaged<B_SiteData, SiteData_GetList_Input_APIItem, SiteData_GetList_Output_Row_APIItem>
    {
        private IGetListPagedHelper<SiteData_GetList_Input_APIItem> _getListHelper;

        public SiteDataController()
        {
            _getListHelper = new GetListPagedHelper<SiteDataController, B_SiteData, SiteData_GetList_Input_APIItem,
                SiteData_GetList_Output_Row_APIItem>(this);
        }

        #region GetList
        public async Task<string> GetList(SiteData_GetList_Input_APIItem input)
        {
            return await _getListHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(SiteData_GetList_Input_APIItem input)
        {
            bool isValid = input
                .StartValidate()
                .Validate(i => i.BCID.IsValidIdOrZero(),
                    () =>AddError(EmptyNotAllowed("分類 ID")))
                .IsValid();
            
            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(SiteData_GetList_Input_APIItem input)
        {
            var query = DC.B_SiteData.Include(sd => sd.BC).AsQueryable();

            if (input.ActiveFlag != null)
                query = query.Where(sd => sd.ActiveFlag == input.ActiveFlag);

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(sd => sd.Title.Contains(input.Keyword) || sd.Code.Contains(input.Keyword));

            if (input.BCID > 0)
                query = query.Where(sd => sd.BCID == input.BCID);

            return query.OrderBy(sd => sd.BSID);
        }

        public async Task<SiteData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_SiteData entity)
        {
            return await Task.FromResult(new SiteData_GetList_Output_Row_APIItem
            {
                BSID = entity.BSID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC.TitleC ?? "",
                BC_TitleE = entity.BC.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                BasicSize = entity.BasicSize,
                MaxSize = entity.MaxSize,
                UnitPrice = entity.UnitPrice,
                InPrice = entity.InPrice,
                OutPrice = entity.OutPrice,
                CubicleFlag = entity.CubicleFlag,
                PhoneExt1 = entity.PhoneExt1 ?? "",
                PhoneExt2 = entity.PhoneExt2 ?? "",
                PhoneExt3 = entity.PhoneExt3 ?? "",
                Note = entity.Note
            });
        }
        #endregion
    }
}