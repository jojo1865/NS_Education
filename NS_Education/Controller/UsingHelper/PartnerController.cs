using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Partner.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;

namespace NS_Education.Controller.UsingHelper
{
    public class PartnerController : PublicClass
    , IGetListPaged<B_Partner, Partner_GetList_Input_APIItem, Partner_GetList_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Partner_GetList_Input_APIItem> _getListPagedHelper;

        public PartnerController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<PartnerController, B_Partner, Partner_GetList_Input_APIItem,
                    Partner_GetList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetList
        public async Task<string> GetList(Partner_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Partner_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之合作廠商所屬分類 ID")))
                .Validate(i => i.BSCID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之合作廠商所在區域 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_Partner> GetListPagedOrderedQuery(Partner_GetList_Input_APIItem input)
        {
            var query = DC.B_Partner
                .Include(p => p.BC)
                .Include(p => p.BSC)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(p =>
                    p.Title.Contains(input.Keyword) || p.Code.Contains(input.Keyword) ||
                    p.Compilation.Contains(input.Keyword));

            if (input.BCID.IsValidId())
                query = query.Where(p => p.BCID == input.BCID);

            if (input.BSCID.IsValidId())
                query = query.Where(p => p.BSCID == input.BSCID);

            return query.OrderBy(p => p.BPID);
        }

        public async Task<Partner_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_Partner entity)
        {
            return await Task.FromResult(new Partner_GetList_Output_Row_APIItem
            {
                BPID = entity.BPID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Compilation = entity.Compilation ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                Email = entity.Email ?? "",
                CleanFlag = entity.CleanFlag,
                CleanPrice = entity.CleanPrice,
                CleanSDate = entity.CleanSDate.ToFormattedStringDate(),
                CleanEDate = entity.CleanEDate.ToFormattedStringDate()
            });
        }
        #endregion
    }
}