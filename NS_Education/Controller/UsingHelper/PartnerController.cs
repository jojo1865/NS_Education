using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Partner.GetInfoById;
using NS_Education.Models.APIItems.Partner.GetList;
using NS_Education.Models.APIItems.Partner.Submit;
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
    public class PartnerController : PublicClass
        , IGetListPaged<B_Partner, Partner_GetList_Input_APIItem, Partner_GetList_Output_Row_APIItem>
        , IGetInfoById<B_Partner, Partner_GetInfoById_Output_APIItem>
        , IDeleteItem<B_Partner>
        , IChangeActive<B_Partner>
        , ISubmit<B_Partner, Partner_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Partner_GetList_Input_APIItem> _getListPagedHelper;

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly ISubmitHelper<Partner_Submit_Input_APIItem> _submitHelper;

        public PartnerController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<PartnerController, B_Partner, Partner_GetList_Input_APIItem,
                    Partner_GetList_Output_Row_APIItem>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<PartnerController, B_Partner, Partner_GetInfoById_Output_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<PartnerController, B_Partner>(this);
            _changeActiveHelper = new ChangeActiveHelper<PartnerController, B_Partner>(this);
            _submitHelper = new SubmitHelper<PartnerController, B_Partner, Partner_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
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
                Note = entity.Note,
                CleanFlag = entity.CleanFlag,
                CleanPrice = entity.CleanPrice,
                CleanSDate = entity.CleanSDate.ToFormattedStringDate(),
                CleanEDate = entity.CleanEDate.ToFormattedStringDate()
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public async Task<bool> GetInfoByIdValidateInput(int id)
        {
            bool isValid = id.StartValidate()
                .Validate(i => i.IsValidId(), () => AddError(EmptyNotAllowed("合作廠商 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_Partner> GetInfoByIdQuery(int id)
        {
            return DC.B_Partner
                .Include(p => p.BC)
                .Include(p => p.BSC)
                .Where(p => p.BPID == id);
        }

        public async Task<Partner_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_Partner entity)
        {
            return await Task.FromResult(new Partner_GetInfoById_Output_APIItem
            {
                BPID = entity.BPID,

                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                BC_List = entity.BC == null
                    ? new List<BaseResponseRowForSelectable>()
                    : await DC.B_Category
                        .Where(c => c.CategoryType == entity.BC.CategoryType && c.ActiveFlag && !c.DeleteFlag)
                        .Select(c => new BaseResponseRowForSelectable
                        {
                            ID = c.BCID,
                            Title = c.TitleC ?? c.TitleE ?? "",
                            SelectFlag = c.BCID == entity.BCID
                        }).ToListAsync(),

                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Compilation = entity.Compilation,

                BSCID = entity.BSCID,
                BSC_Title = entity.BSC?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.BSC?.CodeType, entity.BSCID),

                Email = entity.Email,
                Note = entity.Note,
                CleanFlag = entity.CleanFlag,
                CleanPrice = entity.CleanPrice,
                CleanSDate = entity.CleanSDate.ToFormattedStringDate(),
                CleanEDate = entity.CleanEDate.ToFormattedStringDate()
            });
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<B_Partner> DeleteItemQuery(int id)
        {
            return DC.B_Partner.Where(p => p.BPID == id);
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<B_Partner> ChangeActiveQuery(int id)
        {
            return DC.B_Partner.Where(p => p.BPID == id);
        }

        #endregion

        #region Submit

        private const string SubmitCleanDatesIncorrect = "清潔合約結束日應大於等於起始日！";

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Partner_Submit_Input_APIItem.BPID))]
        public async Task<string> Submit(Partner_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Partner_Submit_Input_APIItem input)
        {
            return input.BPID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Partner_Submit_Input_APIItem input)
        {
            DateTime startDate = default;
            DateTime endDate = default;

            bool isValid = input.StartValidate(true)
                .Validate(i => i.BPID == 0, () => AddError(WrongFormat("廠商 ID")))
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("分類 ID")))
                .Validate(i => i.CleanSDate.TryParseDateTime(out startDate), () => AddError(WrongFormat("清潔合約起始日")))
                .Validate(i => i.CleanEDate.TryParseDateTime(out endDate), () => AddError(WrongFormat("清潔合約結束日")))
                .Validate(i => endDate >= startDate, () => AddError(SubmitCleanDatesIncorrect))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<B_Partner> SubmitCreateData(Partner_Submit_Input_APIItem input)
        {
            if (!input.CleanSDate.TryParseDateTime(out DateTime startDate)
                || !input.CleanEDate.TryParseDateTime(out DateTime endDate))
                throw new ArgumentException(SubmitCleanDatesIncorrect);

            return await Task.FromResult(new B_Partner
            {
                BPID = input.BPID,
                BCID = input.BCID,
                Code = input.Code,
                Title = input.Title,
                Compilation = input.Compilation,
                BSCID = input.BSCID,
                Email = input.Email,
                CleanFlag = input.CleanFlag,
                CleanPrice = input.CleanPrice,
                CleanSDate = startDate,
                CleanEDate = endDate
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Partner_Submit_Input_APIItem input)
        {
            DateTime startDate = default;
            DateTime endDate = default;

            bool isValid = input.StartValidate(true)
                .Validate(i => i.BPID.IsValidId(), () => AddError(EmptyNotAllowed("廠商 ID")))
                .Validate(i => i.BCID.IsValidId(), () => AddError(EmptyNotAllowed("分類 ID")))
                .Validate(i => i.CleanSDate.TryParseDateTime(out startDate), () => AddError(WrongFormat("清潔合約起始日")))
                .Validate(i => i.CleanEDate.TryParseDateTime(out endDate), () => AddError(WrongFormat("清潔合約結束日")))
                .Validate(i => endDate >= startDate, () => AddError(SubmitCleanDatesIncorrect))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_Partner> SubmitEditQuery(Partner_Submit_Input_APIItem input)
        {
            return DC.B_Partner.Where(p => p.BPID == input.BPID);
        }

        public void SubmitEditUpdateDataFields(B_Partner data, Partner_Submit_Input_APIItem input)
        {
            if (!input.CleanSDate.TryParseDateTime(out DateTime startDate)
                || !input.CleanEDate.TryParseDateTime(out DateTime endDate))
                throw new ArgumentException(SubmitCleanDatesIncorrect);

            data.BPID = input.BPID;
            data.BCID = input.BCID;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.Compilation = input.Compilation ?? data.Compilation;
            data.BSCID = input.BSCID;
            data.Email = input.Email ?? data.Email;
            data.CleanFlag = input.CleanFlag;
            data.CleanPrice = input.CleanPrice;
            data.CleanSDate = startDate;
            data.CleanEDate = endDate;
        }

        #endregion

        #endregion
    }
}