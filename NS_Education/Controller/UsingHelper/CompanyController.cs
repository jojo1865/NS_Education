using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Company.GetInfoById;
using NS_Education.Models.APIItems.Company.GetList;
using NS_Education.Models.APIItems.Company.Submit;
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
            _getInfoByIdHelper = new GetInfoByIdHelper<CompanyController, D_Company, Company_GetInfoById_Output_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Company_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Company_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BCID.IsZeroOrAbove(), () => AddError(EmptyNotAllowed("資料所屬分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Company> GetListPagedOrderedQuery(Company_GetList_Input_APIItem input)
        {
            var query = DC.D_Company
                .Include(c => c.BC)
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
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                DepartmentItems = entity.D_Department
                    .OrderBy(dd => dd.DDID)
                    .Select(dd => new BaseResponseRowIdTitle
                {
                    ID = dd.DDID,
                    Title = dd.TitleC ?? dd.TitleE ?? ""
                })
                    .ToList(),
                DepartmentCt = entity.D_Department.Count
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
                .Include(dc => dc.BC)
                .Include(dc => dc.D_Department)
                .Where(dc => dc.DCID == id);
        }

        public async Task<Company_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_Company entity)
        {
            return await Task.FromResult(new Company_GetInfoById_Output_APIItem
            {
                DCID = entity.DCID,
                BCID = entity.BCID,
                BC_TitleC = entity.BC?.TitleC ?? "",
                BC_TitleE = entity.BC?.TitleE ?? "",
                BC_List = await DC.B_Category.GetCategorySelectable(entity.BC?.CategoryType, entity.BCID),
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
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_Company> DeleteItemQuery(int id)
        {
            return DC.D_Company.Where(c => c.DCID == id);
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
            bool isValid = input.StartValidate()
                .Validate(i => i.DCID == 0, () => AddError(WrongFormat("公司 ID")))
                .Validate(i => i.BCID.IsAboveZero(), () => AddError(EmptyNotAllowed("分類 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_Company> SubmitCreateData(Company_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_Company
            {
                BCID = input.BCID,
                Code = input.Code,
                TitleC = input.TitleC,
                TitleE = input.TitleE
            });
        }
        #endregion

        #region Submit - Edit
        public async Task<bool> SubmitEditValidateInput(Company_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DCID.IsAboveZero(), () => AddError(EmptyNotAllowed("公司 ID")))
                .Validate(i => i.BCID.IsAboveZero(), () => AddError(EmptyNotAllowed("分類 ID")))
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
            data.Code = input.Code;
            data.TitleC = input.TitleC;
            data.TitleE = input.TitleE;
        }
        #endregion

        #endregion
    }
}