using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.CustomerGift.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    public class CustomerGiftController : PublicClass,
        IGetListPaged<CustomerGift, CustomerGift_GetList_Input_APIItem, CustomerGift_GetList_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<CustomerGift_GetList_Input_APIItem> _getListPagedHelper;

        public CustomerGiftController()
        {
            _getListPagedHelper = new GetListPagedHelper<CustomerGiftController, CustomerGift, CustomerGift_GetList_Input_APIItem,
                CustomerGift_GetList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetList
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CustomerGift_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(CustomerGift_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate(true)
                .Validate(i => i.CID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.SendYear.IsInBetween(1911, 9999), () => AddError(WrongFormat("欲篩選之贈送年分")))
                .Validate(i =>
                        !i.SDate.TryParseDateTime(out DateTime startDate)
                        || !i.EDate.TryParseDateTime(out DateTime endDate)
                        || endDate >= startDate
                    , () => AddError("GetDateRangeIncorrect"))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<CustomerGift> GetListPagedOrderedQuery(CustomerGift_GetList_Input_APIItem input)
        {
            var query = DC.CustomerGift
                .Include(cg => cg.C)
                .Include(cg => cg.BSC)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(cg => cg.Title.Contains(input.Keyword));

            if (input.CID.IsValidId())
                query = query.Where(cg => cg.CID == input.CID);

            query = query.Where(cg => cg.Year == input.SendYear);

            if (input.SDate.TryParseDateTime(out DateTime startDate))
                query = query.Where(cg => cg.SendDate.Date >= startDate.Date);

            if (input.EDate.TryParseDateTime(out DateTime endDate))
                query = query.Where(cg => cg.SendDate.Date <= endDate.Date);

            return query.OrderByDescending(cg => cg.SendDate)
                .ThenBy(cg => cg.CID)
                .ThenBy(cg => cg.CGID);
        }

        public async Task<CustomerGift_GetList_Output_Row_APIItem> GetListPagedEntityToRow(CustomerGift entity)
        {
            return await Task.FromResult(new CustomerGift_GetList_Output_Row_APIItem
            {
                CGID = entity.CGID,
                CID = entity.CID,
                C_TitleC = entity.C?.TitleC ?? "",
                C_TitleE = entity.C?.TitleE ?? "",
                Year = entity.Year,
                SendDate = entity.SendDate.ToFormattedStringDateTime(),
                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                Title = entity.Title ?? "",
                Ct = entity.Ct,
                Note = entity.Note ?? ""
            });
        }
        #endregion
    }
}