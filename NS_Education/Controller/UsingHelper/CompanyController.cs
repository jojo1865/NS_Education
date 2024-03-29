﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Company.GetInfoById;
using NS_Education.Models.APIItems.Controller.Company.GetList;
using NS_Education.Models.APIItems.Controller.Company.Submit;
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
    public class CompanyController : PublicClass,
        IGetListPaged<D_Company, Company_GetList_Input_APIItem, Company_GetList_Output_Row_APIItem>,
        IGetInfoById<D_Company, Company_GetInfoById_Output_APIItem>,
        IDeleteItem<D_Company>,
        ISubmit<D_Company, Company_Submit_Input_APIItem>,
        IChangeActive<D_Company>
    {
        #region Intialization

        private readonly IGetListPagedHelper<Company_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<Company_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public CompanyController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CompanyController, D_Company, Company_GetList_Input_APIItem,
                    Company_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<CompanyController, D_Company>(this);
            _submitHelper = new SubmitHelper<CompanyController, D_Company, Company_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<CompanyController, D_Company>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<CompanyController, D_Company, Company_GetInfoById_Output_APIItem>(this);
        }

        #endregion

        #region GetList

        // 用於套用 DeleteFlag 設定到子項目
        private int departmentDeleteFlag;

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Company_GetList_Input_APIItem input)
        {
            departmentDeleteFlag = input.DepartmentDeleteFlag;
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Company_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(), () => AddError(EmptyNotAllowed("資料所屬分類 ID", nameof(input.BCID))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Company> GetListPagedOrderedQuery(Company_GetList_Input_APIItem input)
        {
            var query = DC.D_Company
                .Include(c => c.B_Category)
                .Include(c => c.D_Department)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(c =>
                    c.TitleC.Contains(input.Keyword) || c.TitleE.Contains(input.Keyword) ||
                    c.Code.Contains(input.Keyword));

            if (input.BCID.IsAboveZero())
                query = query.Where(c => c.BCID == input.BCID);

            return query.OrderBy(c => c.DCID);
        }

        public async Task<Company_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Company entity)
        {
            return await Task.FromResult(new Company_GetList_Output_Row_APIItem
            {
                DCID = entity.DCID,
                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                DepartmentItems = entity.D_Department
                    .OrderBy(dd => dd.DDID)
                    .Select(dd => new Company_GetList_DepartmentItem_APIItem
                    {
                        DDID = dd.DDID,
                        Title_C = dd.TitleC ?? "",
                        Title_E = dd.TitleE ?? "",
                        PeopleCt = dd.PeopleCt,
                        DCID = dd.DCID,
                        DeleteFlag = dd.DeleteFlag
                    })
                    .Where(dd => departmentDeleteFlag == -1 || dd.DeleteFlag == (departmentDeleteFlag == 1))
                    .ToList(),
                DepartmentCt = entity.D_Department
                    .Count(dd => dd.ActiveFlag && !dd.DeleteFlag),
                DeleteFlag = entity.DeleteFlag
            });
        }

        #endregion

        #region GetInfoByID

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<D_Company> GetInfoByIdQuery(int id)
        {
            return DC.D_Company
                .Include(dc => dc.B_Category)
                .Include(dc => dc.D_Department)
                .Where(dc => dc.DCID == id);
        }

        public async Task<Company_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_Company entity)
        {
            return await Task.FromResult(new Company_GetInfoById_Output_APIItem
            {
                DCID = entity.DCID,
                BCID = entity.BCID,
                BC_TitleC = entity.B_Category?.TitleC ?? "",
                BC_TitleE = entity.B_Category?.TitleE ?? "",
                BC_List = await DC.B_Category.GetCategorySelectable(CategoryType.Company, entity.BCID),
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                DepartmentCt = entity.D_Department.Count
            });
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<D_Company> ChangeActiveQuery(int id)
        {
            return DC.D_Company.Where(c => c.DCID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<D_Company> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.D_Company.Where(c => ids.Contains(c.DCID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Company_Submit_Input_APIItem.DCID))]
        public async Task<string> Submit(Company_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Company_Submit_Input_APIItem input)
        {
            return input.DCID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Company_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DCID == 0, () => AddError(WrongFormat("公司 ID", nameof(input.DCID))))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Company),
                    () => AddError(NotFound("分類 ID", nameof(input.BCID))))
                .Validate(i => i.TitleC.HasContent() || i.TitleE.HasContent(),
                    () => AddError(EmptyNotAllowed("名稱", nameof(input.TitleC))))
                .Validate(i => i.TitleC.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("中文名稱", nameof(input.TitleC), 0, 50)))
                .Validate(i => i.TitleE.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("英文名稱", nameof(input.TitleE), 0, 50)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_Company> SubmitCreateData(Company_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_Company
            {
                BCID = input.BCID,
                TitleC = input.TitleC,
                TitleE = input.TitleE
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Company_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DCID.IsAboveZero(), () => AddError(EmptyNotAllowed("公司 ID", nameof(input.DCID))))
                .ValidateAsync(async i => await DC.B_Category.ValidateCategoryExists(i.BCID, CategoryType.Company),
                    () => AddError(NotFound("分類 ID", nameof(input.BCID))))
                .Validate(i => i.TitleC.HasContent() || i.TitleE.HasContent(),
                    () => AddError(EmptyNotAllowed("名稱", nameof(input.TitleC))))
                .Validate(i => i.TitleC.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("中文名稱", nameof(input.TitleC), 0, 50)))
                .Validate(i => i.TitleE.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("英文名稱", nameof(input.TitleE), 0, 50)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_Company> SubmitEditQuery(Company_Submit_Input_APIItem input)
        {
            return DC.D_Company.Where(c => c.DCID == input.DCID);
        }

        public void SubmitEditUpdateDataFields(D_Company data, Company_Submit_Input_APIItem input)
        {
            data.DCID = input.DCID;
            data.BCID = input.BCID;
            data.TitleC = input.TitleC;
            data.TitleE = input.TitleE;
        }

        #endregion

        #endregion
    }
}