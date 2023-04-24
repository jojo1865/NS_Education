using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.StaticCode.GetInfoById;
using NS_Education.Models.APIItems.StaticCode.GetList;
using NS_Education.Models.APIItems.StaticCode.Submit;
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
    /// <summary>
    /// 靜態參數的 Controller。
    /// </summary>
    public class StaticCodeController : PublicClass
        , IGetTypeList<B_StaticCode>
        , IGetListPaged<B_StaticCode, StaticCode_GetList_Input_APIItem, StaticCode_GetList_Output_Row_APIItem>
        , IChangeActive<B_StaticCode>
        , IDeleteItem<B_StaticCode>
        , ISubmit<B_StaticCode, StaticCode_Submit_Input_APIItem>
    {
        #region 共用

        private readonly IGetTypeListHelper _getTypeListHelper;
        private readonly IGetListPagedHelper<StaticCode_GetList_Input_APIItem> _getListHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<StaticCode_Submit_Input_APIItem> _submitHelper;

        /// <summary>
        /// 靜態參數類別名稱對照表。<br/>
        /// 內容在建構式 populate。<br/>
        /// 在 ASP.NET 中，端點每次被呼叫都會是新的 Controller，所以沒有需要 refresh 的問題。
        /// </summary>
        private readonly Dictionary<string, B_StaticCode> StaticCodeTypes;

        public StaticCodeController()
        {
            StaticCodeTypes = DC.B_StaticCode
                .Where(sc => sc.ActiveFlag && !sc.DeleteFlag)
                .Where(sc => sc.CodeType == 0)
                .OrderBy(sc => sc.Code)
                .ThenBy(sc => sc.SortNo)
                // EF 不支援 GroupBy，所以回到本地在記憶體做
                .AsEnumerable()
                // CodeType 和 Code 並不是 PK，有可能有多筆同樣 CodeType Code 的資料，所以這裡各種 Code 只取一筆，以免重複 Key
                .GroupBy(sc => sc.Code)
                .ToDictionary(group => group.Key, group => group.First());

            _getTypeListHelper =
                new GetTypeListHelper<StaticCodeController, B_StaticCode>(this);

            _getListHelper =
                new GetListPagedHelper<StaticCodeController
                    , B_StaticCode
                    , StaticCode_GetList_Input_APIItem
                    , StaticCode_GetList_Output_Row_APIItem>(this);

            _changeActiveHelper =
                new ChangeActiveHelper<StaticCodeController, B_StaticCode>(this);

            _deleteItemHelper =
                new DeleteItemHelper<StaticCodeController, B_StaticCode>(this);

            _submitHelper =
                new SubmitHelper<StaticCodeController, B_StaticCode, StaticCode_Submit_Input_APIItem>(this);
        }

        #endregion

        #region GetTypeList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetTypeList()
        {
            return await _getTypeListHelper.GetTypeList();
        }

        public IOrderedQueryable<B_StaticCode> GetTypeListQuery()
        {
            return DC.B_StaticCode
                .Where(sc => sc.CodeType == 0 && sc.ActiveFlag)
                .OrderBy(sc => sc.SortNo);
        }

        public async Task<BaseResponseRowForType> GetTypeListEntityToRow(B_StaticCode entity)
        {
            return await Task.FromResult(new BaseResponseRowForType
            {
                ID = int.Parse(entity.Code),
                Title = entity.Title
            });
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(StaticCode_GetList_Input_APIItem input)
        {
            return await _getListHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(StaticCode_GetList_Input_APIItem input)
        {
            return await Task.Run(() => input
                .StartValidate()
                .Validate(i => i.CodeType >= -1,
                    () => AddError(EmptyNotAllowed("靜態參數類別")))
                .IsValid()
            );
        }

        public IOrderedQueryable<B_StaticCode> GetListPagedOrderedQuery(StaticCode_GetList_Input_APIItem input)
        {
            var query = DC.B_StaticCode.Where(sc => sc.ActiveFlag);

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(sc => sc.Title.Contains(input.Keyword) || sc.Code.Contains(input.Keyword));

            if (input.CodeType > -1)
                query = query.Where(sc => sc.CodeType == input.CodeType);

            return query
                .OrderBy(q => q.CodeType)
                .ThenBy(q => q.SortNo)
                .ThenBy(q => q.Code);
        }

        public async Task<StaticCode_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_StaticCode entity)
        {
            return await Task.FromResult(
                new StaticCode_GetList_Output_Row_APIItem
                {
                    BSCID = entity.BSCID,
                    iCodeType = entity.CodeType,
                    sCodeType = StaticCodeTypes.ContainsKey(entity.CodeType.ToString())
                        ? StaticCodeTypes[entity.CodeType.ToString()]?.Title ?? ""
                        : "",
                    Code = entity.Code,
                    Title = entity.Title,
                    SortNo = entity.SortNo,
                    Note = entity.Note ?? "",
                });
        }

        #endregion

        #region GetInfoById

        private const string GetInfoByIdInputIncorrect = "未輸入欲查詢的 ID 或格式有誤！";
        private const string GetInfoByIdNotFound = "查無指定的資料！";

        private readonly StaticCode_GetInfoById_Output_APIItem _getInfoByIdDummyOutput =
            new StaticCode_GetInfoById_Output_APIItem
            {
                BSCID = 0,
                iCodeType = 0,
                sCodeType = null,
                CodeTypeList = null, // 在轉換方法中設值。所以這個物件不能是 static。
                Code = null,
                Title = null,
                SortNo = 0,
                Note = null
            };

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoById(int id)
        {
            // 因為這個端點有特殊邏輯（id 輸入 0 時不查資料而是回傳僅部分欄位的空資料），不使用 Helper 會比較清晰

            // 1. 驗證輸入
            if (id < -1)
            {
                AddError(GetInfoByIdInputIncorrect);
                return GetResponseJson();
            }

            // 2. 依據輸入分支
            // |- a. 如果是 0，拿空資料
            // +- b. 如果不是 0，查詢資料，無資料時跳錯
            StaticCode_GetInfoById_Output_APIItem response;
            if (id == 0)
                response = _getInfoByIdDummyOutput;
            else
            {
                var entity = await DC.B_StaticCode
                    .Where(sc => sc.ActiveFlag && !sc.DeleteFlag)
                    .Where(sc => sc.BSCID == id)
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    AddError(GetInfoByIdNotFound);
                    return GetResponseJson();
                }

                response = await GetInfoByIdConvertEntityToResponse(entity);
            }

            // 3. 幫資料塞 CodeTypeList
            // 借用 GetTypeList 的邏輯
            response.CodeTypeList = GetTypeListQuery()
                .Where(sc => !sc.DeleteFlag)
                .AsEnumerable() // 在這裡就轉換成 Enumerable，避免 LINQ 以為是 Query 中要做的處理，導致多重 DataConnection 問題
                .Select(sc => Task.Run(() => GetTypeListEntityToRow(sc)).Result)
                .ToList();

            // 4. 回傳
            return GetResponseJson(response);
        }

        private async Task<StaticCode_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            B_StaticCode entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await Task.FromResult(new StaticCode_GetInfoById_Output_APIItem
            {
                BSCID = entity.BSCID,
                iCodeType = entity.CodeType,
                sCodeType = StaticCodeTypes.ContainsKey(entity.CodeType.ToString())
                    ? StaticCodeTypes[entity.CodeType.ToString()]?.Title ?? ""
                    : "",
                // CodeTypeList 在此不塞值 
                Code = entity.Code,
                Title = entity.Title,
                SortNo = entity.SortNo,
                Note = entity.Note ?? ""
            });
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<B_StaticCode> ChangeActiveQuery(int id)
        {
            return DC.B_StaticCode.Where(sc => sc.BSCID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int id)
        {
            return await _deleteItemHelper.DeleteItem(id);
        }

        public IQueryable<B_StaticCode> DeleteItemQuery(int id)
        {
            return DC.B_StaticCode.Where(sc => sc.BSCID == id);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any,
            RequirePrivilege.AddOrEdit,
            null,
            nameof(StaticCode_Submit_Input_APIItem.BSCID))]
        public async Task<string> Submit(StaticCode_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(StaticCode_Submit_Input_APIItem input)
        {
            return input.BSCID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(StaticCode_Submit_Input_APIItem input)
        {
            return await Task.Run(() => input.StartValidate()
                .Validate(i => i.BSCID.IsValidIdOrZero(),
                    () => AddError(EmptyNotAllowed("靜態參數 ID")))
                .Validate(i => i.CodeType.IsValidIdOrZero(),
                    () => AddError(EmptyNotAllowed("靜態參數所屬類別")))
                .Validate(i => !i.Code.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("靜態參數編碼")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("靜態參數名稱")))
                .Validate(i => i.ActiveFlag != null,
                    () => AddError(EmptyNotAllowed("是否啟用")))
                .IsValid());
        }

        public async Task<B_StaticCode> SubmitCreateData(StaticCode_Submit_Input_APIItem input)
        {
            return new B_StaticCode
            {
                CodeType = input.CodeType ?? throw new ArgumentNullException(nameof(input.CodeType)),
                Code = input.Code ?? throw new ArgumentNullException(nameof(input.Code)),
                Title = input.Title ?? throw new ArgumentNullException(nameof(input.Title)),
                // SortNo 邏輯：每種 CodeType 中目前最大的 SortNo+1，如果是第一筆新 Code 就是 1
                SortNo = await DC.B_StaticCode
                    .Where(sc => sc.CodeType == input.CodeType)
                    .OrderBy(sc => sc.SortNo)
                    .Select(sc => sc.SortNo)
                    .FirstOrDefaultAsync() + 1,
                Note = input.Note ?? "",
                ActiveFlag = input.ActiveFlag ?? throw new ArgumentNullException(nameof(input.ActiveFlag)),
            };
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(StaticCode_Submit_Input_APIItem input)
        {
            return await Task.Run(() => input.StartValidate()
                .Validate(i => i.BSCID.IsValidId(),
                    () => AddError(EmptyNotAllowed("靜態參數 ID")))
                .Validate(i => i.CodeType.IsValidIdOrZero(),
                    () => AddError(EmptyNotAllowed("靜態參數所屬類別")))
                .Validate(i => !i.Code.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("靜態參數編碼")))
                .Validate(i => !i.Title.IsNullOrWhiteSpace(),
                    () => AddError(EmptyNotAllowed("靜態參數名稱")))
                .Validate(i => i.ActiveFlag != null,
                    () => AddError(EmptyNotAllowed("是否啟用")))
                .Validate(i => i.DeleteFlag != null,
                    () => AddError(EmptyNotAllowed("刪除狀態")))
                .IsValid());
        }

        public IQueryable<B_StaticCode> SubmitEditQuery(StaticCode_Submit_Input_APIItem input)
        {
            return DC.B_StaticCode.Where(sc => sc.BSCID == input.BSCID);
        }

        public void SubmitEditUpdateDataFields(B_StaticCode data, StaticCode_Submit_Input_APIItem input)
        {
            data.CodeType = input.CodeType ?? data.CodeType;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.Note = input.Note ?? data.Note;
            data.ActiveFlag = input.ActiveFlag ?? data.ActiveFlag;
            data.DeleteFlag = input.DeleteFlag ?? data.DeleteFlag;
        }

        #endregion

        #endregion
    }
}