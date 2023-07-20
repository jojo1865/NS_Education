using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Partner.GetInfoById;
using NS_Education.Models.APIItems.Controller.Partner.GetList;
using NS_Education.Models.APIItems.Controller.Partner.Submit;
using NS_Education.Models.Entities;
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Partner_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Partner_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選之合作廠商所屬分類 ID", nameof(input.BCID))))
                .Validate(i => i.BSCID.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選之合作廠商所在區域 ID", nameof(input.BSCID))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_Partner> GetListPagedOrderedQuery(Partner_GetList_Input_APIItem input)
        {
            var query = DC.B_Partner
                .Include(p => p.B_Category)
                .Include(p => p.B_StaticCode)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(p =>
                    p.Title.Contains(input.Keyword) || p.Code.Contains(input.Keyword));

            if (input.BCID.IsAboveZero())
                query = query.Where(p => p.BCID == input.BCID);

            if (input.BSCID.IsAboveZero())
                query = query.Where(p => p.BSCID == input.BSCID);

            return query.OrderBy(p => p.BPID);
        }

        public async Task<Partner_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_Partner entity)
        {
            return await Task.FromResult(new Partner_GetList_Output_Row_APIItem
            {
                BPID = entity.BPID,
                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Compilation = entity.Compilation ?? "",
                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode?.Title ?? "",
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<B_Partner> GetInfoByIdQuery(int id)
        {
            return DC.B_Partner
                .Include(p => p.B_Category)
                .Include(p => p.B_StaticCode)
                .Where(p => p.BPID == id);
        }

        public async Task<Partner_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_Partner entity)
        {
            return await Task.FromResult(new Partner_GetInfoById_Output_APIItem
            {
                BPID = entity.BPID,

                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                BC_List = await DC.B_Category.GetCategorySelectable(entity.B_Category?.CategoryType, entity.BCID),

                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                Compilation = entity.Compilation,

                BSCID = entity.BSCID,
                BSC_Title = entity.B_StaticCode?.Title ?? "",
                BSC_List = await DC.B_StaticCode.GetStaticCodeSelectable(entity.B_StaticCode?.CodeType, entity.BSCID),

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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<B_Partner> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.B_Partner.Where(p => ids.Contains(p.BPID));
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
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

            bool isValid = await input.StartValidate()
                .Validate(i => i.BPID == 0, () => AddError(WrongFormat("廠商 ID", nameof(input.BPID))))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Partner),
                    () => AddError(NotFound("分類 ID", nameof(input.BCID))))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Region),
                    () => AddError(NotFound("區域 ID", nameof(input.BSCID))))
                .Validate(i => i.BCID.IsAboveZero(), () => AddError(EmptyNotAllowed("分類 ID", nameof(input.BCID))))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("名稱", nameof(input.Title))))
                .Validate(i => !i.Compilation.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("統一編號", nameof(input.Compilation))))
                .Validate(i => i.Email.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("E-Mail", nameof(input.Email), 0, 100)))
                .Validate(i => !i.CleanFlag || i.CleanSDate.TryParseDateTime(out startDate),
                    () => AddError(WrongFormat("清潔合約起始日", nameof(input.CleanSDate))))
                .Validate(i => !i.CleanFlag || i.CleanEDate.TryParseDateTime(out endDate),
                    () => AddError(WrongFormat("清潔合約結束日", nameof(input.CleanEDate))))
                .Validate(i => !i.CleanFlag || endDate >= startDate,
                    () => AddError(MinLargerThanMax("清潔合約起始日", nameof(input.CleanSDate), "清潔合約結束日",
                        nameof(input.CleanEDate))))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("代碼", nameof(input.Code), 0, 10)))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    () => AddError(LengthOutOfRange("名稱", nameof(input.Title), 1, 60)))
                .Validate(i => i.Compilation.Length == 8,
                    () => AddError(WrongFormat("統一編號", nameof(input.Compilation))))
                .IsValid();

            return isValid;
        }

        public async Task<B_Partner> SubmitCreateData(Partner_Submit_Input_APIItem input)
        {
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
                CleanSDate = input.CleanSDate.ParseDateTime()
                    .Date,
                CleanEDate = input.CleanEDate.ParseDateTime()
                    .Date,
                ActiveFlag = input.ActiveFlag,
                Note = input.Note
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Partner_Submit_Input_APIItem input)
        {
            DateTime startDate = default;
            DateTime endDate = default;

            bool isValid = await input.StartValidate()
                .Validate(i => i.BPID.IsAboveZero(), () => AddError(EmptyNotAllowed("廠商 ID", nameof(input.BPID))))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Partner),
                    () => AddError(NotFound("分類 ID", nameof(input.BCID))))
                .ValidateAsync(
                    async i => await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID, StaticCodeType.Region),
                    () => AddError(NotFound("區域 ID", nameof(input.BSCID))))
                .Validate(i => i.BCID.IsAboveZero(), () => AddError(EmptyNotAllowed("分類 ID", nameof(input.BCID))))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("名稱", nameof(input.Title))))
                .Validate(i => !i.Compilation.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("統一編號", nameof(input.Compilation))))
                .Validate(i => i.Email.HasLengthBetween(0, 100),
                    () => AddError(LengthOutOfRange("E-Mail", nameof(input.Email), 0, 100)))
                .Validate(i => !i.CleanFlag || i.CleanSDate.TryParseDateTime(out startDate),
                    () => AddError(WrongFormat("清潔合約起始日", nameof(input.CleanSDate))))
                .Validate(i => !i.CleanFlag || i.CleanEDate.TryParseDateTime(out endDate),
                    () => AddError(WrongFormat("清潔合約結束日", nameof(input.CleanEDate))))
                .Validate(i => !i.CleanFlag || endDate >= startDate,
                    () => AddError(MinLargerThanMax("清潔合約起始日", nameof(input.CleanSDate), "清潔合約結束日",
                        nameof(input.CleanEDate))))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("代碼", nameof(input.Code), 0, 10)))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    () => AddError(LengthOutOfRange("名稱", nameof(input.Title), 1, 60)))
                .Validate(i => i.Compilation.Length == 8,
                    () => AddError(WrongFormat("統一編號", nameof(input.Compilation))))
                .IsValid();

            return isValid;
        }

        public IQueryable<B_Partner> SubmitEditQuery(Partner_Submit_Input_APIItem input)
        {
            return DC.B_Partner.Where(p => p.BPID == input.BPID);
        }

        public void SubmitEditUpdateDataFields(B_Partner data, Partner_Submit_Input_APIItem input)
        {
            data.BPID = input.BPID;
            data.BCID = input.BCID;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.Compilation = input.Compilation ?? data.Compilation;
            data.BSCID = input.BSCID;
            data.Email = input.Email ?? data.Email;
            data.CleanFlag = input.CleanFlag;
            data.CleanPrice = input.CleanPrice;
            data.CleanSDate = input.CleanSDate.ParseDateTime().Date;
            data.CleanEDate = input.CleanEDate.ParseDateTime().Date;
            data.ActiveFlag = input.ActiveFlag;
            data.Note = input.Note ?? data.Note;
        }

        #endregion

        #endregion
    }
}