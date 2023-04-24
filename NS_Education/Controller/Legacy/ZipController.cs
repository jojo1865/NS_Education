using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Zip;
using NS_Education.Models.APIItems.Zip.GetList;
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
        IDeleteItem<D_Zip>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Zip_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        public ZipController()
        {
            _getListPagedHelper = new GetListPagedHelper<ZipController, D_Zip, Zip_GetList_Input_APIItem,
                Zip_GetList_Output_Row_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<ZipController, D_Zip>(this);
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
        public async Task<string> Submit(D_Zip N)
        {
            Error = "";
            if (N.DZID == 0)
            {
                if (N.ParentID <= 0)
                    Error += "請選擇這個郵遞區號所屬;";
                if (N.GroupName == "")
                    Error += "請輸入這個郵遞區號的層級;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Zip.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Zip.FirstOrDefaultAsync(q => q.DZID == N.DZID && !q.DeleteFlag);
                if (N.ParentID <= 0)
                    Error += "請選擇這個郵遞區號所屬;";
                if (N.GroupName == "")
                    Error += "請輸入這個郵遞區號的層級;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.DZID = N.DZID;
                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.ParentID = N.ParentID;
                    N_.GroupName = N.GroupName;
                    N_.Note = N.Note;

                    N_.ActiveFlag = N.ActiveFlag;
                    N_.DeleteFlag = N.DeleteFlag;
                    N_.UpdUID = GetUid();
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion
        
    }
}