using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Resver.GetHeadList;
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
    public class ResverController : PublicClass, 
        IGetListPaged<Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Resver_GetHeadList_Input_APIItem> _getListPagedHelper;

        public ResverController()
        {
            _getListPagedHelper = new GetListPagedHelper<ResverController, Resver_Head, Resver_GetHeadList_Input_APIItem, Resver_GetHeadList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetHeadList
        // route 請參照 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Resver_GetHeadList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Resver_GetHeadList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.CID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.BSCID12.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之預約狀態 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head> GetListPagedOrderedQuery(Resver_GetHeadList_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.BSCID12Navigation)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(rh => rh.Code != null && rh.Code.Contains(input.Keyword));

            if (input.TargetDate.TryParseDateTime(out DateTime targetDate))
                query = query.Where(rh => rh.SDate.Date >= targetDate.Date);

            if (input.CID.IsAboveZero())
                query = query.Where(rh => rh.CID == input.CID);

            if (input.BSCID12.IsAboveZero())
                query = query.Where(rh => rh.BSCID12 == input.BSCID12);

            return query.OrderByDescending(rh => rh.SDate)
                .ThenByDescending(rh => rh.EDate)
                .ThenBy(rh => rh.RHID);
        }

        public async Task<Resver_GetHeadList_Output_Row_APIItem> GetListPagedEntityToRow(Resver_Head entity)
        {
            return await Task.FromResult(new Resver_GetHeadList_Output_Row_APIItem
            {
                RHID = entity.RHID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                CustomerTitle = entity.CustomerTitle ?? "",
                CustomerCode = entity.CustomerTitle ?? "",
                PeopleCt = entity.PeopleCt,
                BSCID12 = entity.BSCID12,
                BSCID12_Title = entity.BSCID12Navigation?.Title ?? ""
            });
        }
        #endregion
    }
}