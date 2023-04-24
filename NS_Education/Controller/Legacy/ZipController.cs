using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Zip;
using NS_Education.Models.APIItems.Zip.GetList;
using NS_Education.Models.APIItems.Zip.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.Legacy
{
    public class ZipController : PublicClass,
        IGetListPaged<D_Zip, Zip_GetList_Input_APIItem, Zip_GetList_Output_Row_APIItem>,
        IDeleteItem<D_Zip>,
        ISubmit<D_Zip, Zip_Submit_Input_APIItem>
    {

        #region Initialization

        private readonly IGetListPagedHelper<Zip_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<Zip_Submit_Input_APIItem> _submitHelper;

        public ZipController()
        {
            _getListPagedHelper = new GetListPagedHelper<ZipController, D_Zip, Zip_GetList_Input_APIItem,
                Zip_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<ZipController, D_Zip>(this);
            _submitHelper = new SubmitHelper<ZipController, D_Zip, Zip_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(Zip_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Zip_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.ParentId.IsValidIdOrZero(), () => AddError(EmptyNotAllowed("上層 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<D_Zip> GetListPagedOrderedQuery(Zip_GetList_Input_APIItem input)
        {
            var query = DC.D_Zip.AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(z => z.Title.Contains(input.Keyword) || z.Code.Contains(input.Keyword));

            query = query.Where(z => z.ParentID == input.ParentId);

            return query.OrderBy(z => z.DZID);
        }

        public async Task<Zip_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_Zip entity)
        {
            return await Task.FromResult(new Zip_GetList_Output_Row_APIItem
            {
                DZID = entity.DZID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                GroupName = entity.GroupName ?? "",
                ParentID = entity.ParentID,
                Note = entity.Note ?? ""
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = DC.D_Zip.FirstOrDefault(q => q.DZID == ID && !q.DeleteFlag);
            D_Zip_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                Item = new D_Zip_APIItem
                {
                    DZID = N.DZID,
                    ParentID = N.ParentID,
                    Code = N.Code,
                    Title = N.Title,
                    GroupName = N.GroupName,
                    Note = N.Note,
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }

            return ChangeJson(Item);
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.D_Zip.FirstOrDefaultAsync(q => q.DZID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_Zip> DeleteItemQuery(int id)
        {
            return DC.D_Zip.Where(z => z.DZID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_Zip.DZID))]
        public async Task<string> Submit(Zip_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(Zip_Submit_Input_APIItem input)
        {
            return input.DZID == 0;
        }
        
        private async Task<string[]> SubmitGetGroupNames()
        {
            string[] groupNames = await DC.B_StaticCode
                .Where(sc => sc.CodeType == 13 && sc.ActiveFlag && !sc.DeleteFlag)
                .Select(sc => sc.Title)
                .ToArrayAsync();
            return groupNames;
        }

        #region Submit - Add

        private const string SubmitGroupNameNotFound = "層級名稱於靜態參數檔中找不到對應資料！";
        
        public async Task<bool> SubmitAddValidateInput(Zip_Submit_Input_APIItem input)
        {
            string[] groupNames = await SubmitGetGroupNames();

            bool isValid = input.StartValidate()
                .Validate(i => i.DZID == 0, () => AddError(WrongFormat("國籍 / 郵遞區號 ID")))
                .Validate(i => i.ParentID.IsValidId(), () => AddError(EmptyNotAllowed("上層 ID")))
                .Validate(i => groupNames.Contains(input.GroupName), () => AddError(SubmitGroupNameNotFound))
                .IsValid();

            return isValid;
        }

        public async Task<D_Zip> SubmitCreateData(Zip_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_Zip
            {
                ParentID = input.ParentID,
                Code = input.Code,
                Title = input.Title,
                GroupName = input.GroupName,
                Note = input.Note
            });
        }
        
        #endregion
        
        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(Zip_Submit_Input_APIItem input)
        {
            string[] groupNames = await SubmitGetGroupNames();
            
            bool isValid = input.StartValidate()
                .Validate(i => i.DZID.IsValidId(), () => AddError(EmptyNotAllowed("國籍 / 郵遞區號 ID")))
                .Validate(i => i.ParentID.IsValidId(), () => AddError(EmptyNotAllowed("上層 ID")))
                .Validate(i => groupNames.Contains(input.GroupName), () => AddError(SubmitGroupNameNotFound))
                .IsValid();

            return isValid;
        }

        public IQueryable<D_Zip> SubmitEditQuery(Zip_Submit_Input_APIItem input)
        {
            return DC.D_Zip.Where(z => z.DZID == input.DZID);
        }

        public void SubmitEditUpdateDataFields(D_Zip data, Zip_Submit_Input_APIItem input)
        {
            data.ParentID = input.ParentID;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.GroupName = input.GroupName ?? data.GroupName;
            data.Note = input.Note;
        }
        
        #endregion

        #endregion
    }
}