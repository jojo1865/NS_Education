using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.Customer.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper
{
    public class CustomerController : PublicClass,
        IGetListPaged<Customer, Customer_GetList_Input_APIItem, Customer_GetList_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Customer_GetList_Input_APIItem> _getListPagedHelper;

        public CustomerController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CustomerController, Customer, Customer_GetList_Input_APIItem,
                    Customer_GetList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetList
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(Customer_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Customer_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BSCID6.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選的行業別")))
                .Validate(i => i.BSCID4.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選的區域別")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Customer> GetListPagedOrderedQuery(Customer_GetList_Input_APIItem input)
        {
            var query = DC.Customer
                .Include(c => c.Resver_Head)
                .Include(c => c.BSCID6Navigation)
                .Include(c => c.BSCID4Navigation)
                .Include(c => c.CustomerVisit)
                .Include(c => c.CustomerQuestion)
                .Include(c => c.CustomerGift)
                .Include(c => c.M_Customer_BusinessUser)
                .ThenInclude(cbu => cbu.BU)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(c =>
                    c.TitleC.Contains(input.Keyword) 
                    || c.TitleE.Contains(input.Keyword) 
                    || c.Compilation.Contains(input.Keyword) 
                    || c.Code.Contains(input.Keyword));

            if (input.BSCID6.IsValidId())
                query = query.Where(c => c.BSCID6 == input.BSCID6);

            if (input.BSCID4.IsValidId())
                query = query.Where(c => c.BSCID4 == input.BSCID4);

            if (input.BUID.IsValidId())
                query = query.Where(c => c.M_Customer_BusinessUser.Any(cbu => cbu.BUID == input.BUID));

            // ResverType 為 0 時，只找沒有任何預約紀錄的客戶
            // ResverType 為 1 時，只找有預約過的客戶
            if (input.ResverType.IsInBetween(0, 1))
                query = query.Where(c =>
                    c.Resver_Head.Any(ResverHeadIsViable) == (input.ResverType == 1));

            return query.OrderBy(c => c.CID);
        }

        private static bool ResverHeadIsViable(Resver_Head rh)
        {
            return !rh.DeleteFlag && rh.BSCID12 == DbConstants.ReserveHeadDraftStateCode;
        }

        public async Task<Customer_GetList_Output_Row_APIItem> GetListPagedEntityToRow(Customer entity)
        {
            return await Task.FromResult(new Customer_GetList_Output_Row_APIItem
            {
                CID = entity.CID,
                BSCID6 = entity.BSCID6,
                BSC6_Title = entity.BSCID6Navigation?.Title ?? "",
                BSCID4 = entity.BSCID4,
                BSC4_Title = entity.BSCID4Navigation?.Title ?? "",
                Code = entity.Code ?? "",
                Compilation = entity.Compilation ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                Email = entity.Email ?? "",
                InvoiceTitle = entity.InvoiceTitle ?? "",
                ContactName = entity.ContectName ?? "",
                ContactPhone = entity.ContectPhone ?? "",
                Website = entity.Website ?? "",
                Note = entity.Note ?? "",
                BillFlag = entity.BillFlag,
                InFlag = entity.InFlag,
                PotentialFlag = entity.PotentialFlag,
                ResverCt = entity.Resver_Head.Count(ResverHeadIsViable),
                VisitCt = entity.CustomerVisit.Count(cv => !cv.DeleteFlag),
                QuestionCt = entity.CustomerQuestion.Count(cq => !cq.DeleteFlag),
                GiftCt = entity.CustomerGift.Count(cg => !cg.DeleteFlag),
                Items = entity.M_Customer_BusinessUser
                    .Where(cbu => cbu.ActiveFlag && !cbu.DeleteFlag && cbu.BU.ActiveFlag && !cbu.BU.DeleteFlag)
                    .Select(cbu => new Customer_GetList_BusinessUser_APIItem
                    {
                        BUID = cbu.BUID,
                        Name = cbu.BU.Name ?? ""
                    }).ToList()
            });
        }
        #endregion
    }
}