using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.Department.GetInfoById;
using NS_Education.Models.APIItems.Controller.Department.GetList;
using NS_Education.Models.APIItems.Controller.Department.Submit;
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
    public class DepartmentController : PublicClass
        , IGetListPaged<D_Department, Department_GetList_Input_APIItem, Department_GetList_Output_Row_APIItem>
        , IGetInfoById<D_Department, Department_GetInfoById_Output_APIItem>
        , IDeleteItem<D_Department>
        , ISubmit<D_Department, Department_Submit_Input_APIItem>
        , IChangeActive<D_Department>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Department_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<Department_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public DepartmentController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<DepartmentController, D_Department, Department_GetList_Input_APIItem,
                    Department_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new DeleteItemHelper<DepartmentController, D_Department>(this);
            _submitHelper = new SubmitHelper<DepartmentController, D_Department, Department_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<DepartmentController, D_Department>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<DepartmentController, D_Department, Department_GetInfoById_Output_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Department_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Department_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DCID.IsZeroOrAbove(), () => AddError(WrongFormat("所屬公司 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Department> GetListPagedOrderedQuery(Department_GetList_Input_APIItem input)
        {
            var query = DC.D_Department
                .Include(d => d.D_Company)
                .Include(d => d.D_Hall)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(d =>
                    d.TitleC.Contains(input.Keyword) || d.TitleE.Contains(input.Keyword) ||
                    d.Code.Contains(input.Keyword));

            if (input.DCID.IsAboveZero())
                query = query.Where(d => d.DCID == input.DCID);

            return query.OrderBy(d => d.DDID);
        }

        public async Task<Department_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Department entity)
        {
            return await Task.FromResult(new Department_GetList_Output_Row_APIItem
            {
                DDID = entity.DDID,
                DCID = entity.DCID,
                DC_TitleC = entity.D_Company?.TitleC ?? "",
                DC_TitleE = entity.D_Company?.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                PeopleCt = entity.PeopleCt,
                HallCt = entity.D_Hall?.Count ?? 0
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

        public IQueryable<D_Department> GetInfoByIdQuery(int id)
        {
            return DC.D_Department
                .Include(dd => dd.D_Company)
                .Include(dd => dd.D_Hall)
                .Where(dd => dd.DDID == id);
        }

        public async Task<Department_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_Department entity)
        {
            return await Task.FromResult(new Department_GetInfoById_Output_APIItem
            {
                DDID = entity.DDID,
                DCID = entity.DCID,
                DC_TitleC = entity.D_Company?.TitleC ?? "",
                DC_TitleE = entity.D_Company?.TitleE ?? "",
                Code = entity.Code ?? "",
                TitleC = entity.TitleC ?? "",
                TitleE = entity.TitleE ?? "",
                HallCt = entity.D_Hall.Count,
                PeopleCt = entity.PeopleCt
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

        public IQueryable<D_Department> ChangeActiveQuery(int id)
        {
            return DC.D_Department.Where(dd => dd.DDID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<D_Department> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.D_Department.Where(d => ids.Contains(d.DDID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(Department_Submit_Input_APIItem.DDID))]
        public async Task<string> Submit(Department_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Department_Submit_Input_APIItem input)
        {
            return input.DDID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(Department_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DDID == 0, () => AddError(WrongFormat("部門 ID")))
                .ValidateAsync(async i => await DC.D_Company.ValidateIdExists(i.DCID, nameof(D_Company.DCID)), () => AddError(EmptyNotAllowed("公司 ID")))
                .Validate(i => i.TitleC.HasContent() || i.TitleE.HasContent(), () => AddError(EmptyNotAllowed("名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_Department> SubmitCreateData(Department_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_Department
            {
                DCID = input.DCID,
                TitleC = input.TitleC,
                TitleE = input.TitleE,
                PeopleCt = input.PeopleCt
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Department_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.DDID.IsAboveZero(), () => AddError(EmptyNotAllowed("部門 ID")))
                .ValidateAsync(async i => await DC.D_Company.ValidateIdExists(i.DCID, nameof(D_Company.DCID)), () => AddError(EmptyNotAllowed("公司 ID")))
                .Validate(i => i.TitleC.HasContent() || i.TitleE.HasContent(), () => AddError(EmptyNotAllowed("名稱")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_Department> SubmitEditQuery(Department_Submit_Input_APIItem input)
        {
            return DC.D_Department.Where(d => d.DDID == input.DDID);
        }

        public void SubmitEditUpdateDataFields(D_Department data, Department_Submit_Input_APIItem input)
        {
            data.DCID = input.DCID;
            data.TitleC = input.TitleC;
            data.TitleE = input.TitleE;
            data.PeopleCt = input.PeopleCt;
        }

        #endregion

        #endregion
    }
}