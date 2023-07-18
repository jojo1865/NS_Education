using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.OrderCode.GetInfoById;
using NS_Education.Models.APIItems.Controller.OrderCode.GetList;
using NS_Education.Models.APIItems.Controller.OrderCode.GetTypeList;
using NS_Education.Models.APIItems.Controller.OrderCode.Submit;
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
    /// <summary>
    /// 入帳代號 B_OrderCode 的 Controller。
    /// </summary>
    public class OrderCodeController : PublicClass,
        IGetTypeList<B_OrderCode, OrderCode_GetTypeList_Output_APIItem>,
        IGetListPaged<B_OrderCode, OrderCode_GetList_Input_APIItem, OrderCode_GetList_Output_Row_APIItem>,
        IChangeActive<B_OrderCode>,
        IDeleteItem<B_OrderCode>,
        ISubmit<B_OrderCode, OrderCode_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetTypeListHelper _getTypeListHelper;
        private readonly IGetListPagedHelper<OrderCode_GetList_Input_APIItem> _getListHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<OrderCode_Submit_Input_APIItem> _submitHelper;

        /// <summary>
        /// 靜態參數類別名稱對照表。<br/>
        /// 內容在建構式 populate。<br/>
        /// 在 ASP.NET 中，端點每次被呼叫都會是新的 Controller，所以沒有需要 refresh 的問題。
        /// </summary>
        private readonly Dictionary<string, B_OrderCode> OrderCodeTypes;

        public OrderCodeController()
        {
            OrderCodeTypes = DC.B_OrderCode
                .Where(sc => sc.ActiveFlag && !sc.DeleteFlag)
                .Where(sc => sc.CodeType == "0")
                .OrderBy(sc => sc.Code.Length)
                .ThenBy(sc => sc.Code)
                .ThenBy(sc => sc.SortNo)
                // EF 不支援 GroupBy，所以回到本地在記憶體做
                .AsEnumerable()
                // CodeType 和 Code 並不是 PK，有可能有多筆同樣 CodeType Code 的資料，所以這裡各種 Code 只取一筆，以免重複 Key
                .GroupBy(sc => sc.Code)
                .ToDictionary(group => group.Key, group => group.First());
            _submitHelper = new SubmitHelper<OrderCodeController, B_OrderCode, OrderCode_Submit_Input_APIItem>(this);
            _deleteItemHelper = new DeleteItemHelper<OrderCodeController, B_OrderCode>(this);
            _changeActiveHelper = new ChangeActiveHelper<OrderCodeController, B_OrderCode>(this);

            _getTypeListHelper =
                new GetTypeListHelper<OrderCodeController, B_OrderCode, OrderCode_GetTypeList_Output_APIItem>(this);

            _getListHelper = new GetListPagedHelper<OrderCodeController
                , B_OrderCode
                , OrderCode_GetList_Input_APIItem
                , OrderCode_GetList_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetTypeList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetTypeList()
        {
            return await _getTypeListHelper.GetTypeList();
        }

        public IOrderedQueryable<B_OrderCode> GetTypeListQuery()
        {
            return DC.B_OrderCode
                .Where(oc => oc.ActiveFlag && oc.CodeType == "0")
                .OrderBy(oc => oc.SortNo);
        }

        public async Task<OrderCode_GetTypeList_Output_APIItem> GetTypeListEntityToRow(B_OrderCode entity)
        {
            return await Task.FromResult(new OrderCode_GetTypeList_Output_APIItem
            {
                BOCID = entity.BOCID,
                iCodeType = entity.Code,
                sCodeType = entity.Title ?? ""
            });
        }

        #endregion

        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(OrderCode_GetList_Input_APIItem input)
        {
            return await _getListHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(OrderCode_GetList_Input_APIItem input)
        {
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<B_OrderCode> GetListPagedOrderedQuery(OrderCode_GetList_Input_APIItem input)
        {
            var query = DC.B_OrderCode.AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(oc => oc.Title.Contains(input.Keyword) || oc.Code.Contains(input.Keyword));

            if (input.CodeType.HasContent())
                query = query.Where(oc => oc.CodeType == input.CodeType);

            return query.OrderBy(oc => oc.CodeType)
                .ThenBy(oc => oc.SortNo)
                .ThenBy(oc => oc.Code.Length)
                .ThenBy(oc => oc.Code)
                .ThenBy(oc => oc.BOCID);
        }

        public async Task<OrderCode_GetList_Output_Row_APIItem> GetListPagedEntityToRow(B_OrderCode entity)
        {
            return await Task.FromResult(new OrderCode_GetList_Output_Row_APIItem
            {
                BOCID = entity.BOCID,
                iCodeType = entity.CodeType,
                sCodeType = OrderCodeTypes.ContainsKey(entity.CodeType.ToString())
                    ? OrderCodeTypes[entity.CodeType.ToString()].Title
                    : "",
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                PrintTitle = entity.PrintTitle ?? "",
                PrintNote = entity.PrintNote ?? "",
                SortNo = entity.SortNo,
            });
        }

        #endregion

        #region GetInfoById

        private readonly OrderCode_GetInfoById_Output_APIItem _getInfoByIdDummyOutput =
            new OrderCode_GetInfoById_Output_APIItem
            {
                BOCID = 0,
                iCodeType = null,
                sCodeType = null,
                CodeTypeList = null, // 在轉換方法中才設值，所以這個物件不能是 static。
                Code = null,
                Title = null,
                PrintTitle = null,
                PrintNote = null,
                SortNo = 0
            };

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            // 因為這個端點有特殊邏輯（id 輸入 0 時不查資料而是回傳僅部分欄位的空資料），不使用 Helper 會比較清晰

            // 1. 驗證輸入
            if (!id.IsZeroOrAbove())
            {
                AddError(WrongFormat("欲查詢的 ID", nameof(id)));
                return GetResponseJson();
            }

            // 2. 依據輸入分支
            // |- a. 如果是 0，拿空資料
            // +- b. 如果不是 0，查詢資料，無資料時跳錯
            OrderCode_GetInfoById_Output_APIItem response;
            if (id == 0)
                response = _getInfoByIdDummyOutput;
            else
            {
                var entity = await DC.B_OrderCode
                    .Where(sc => sc.ActiveFlag && !sc.DeleteFlag)
                    .Where(sc => sc.BOCID == id)
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    AddError(NotFound());
                    return GetResponseJson();
                }

                // 寫 UserLog
                await DC.WriteUserLogAndSaveAsync(UserLogControlType.Show, GetUid(), Request);

                response = await GetInfoByIdConvertEntityToResponse(entity);
                await response.SetInfoFromEntity(entity, this);
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

        private async Task<OrderCode_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            B_OrderCode entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await Task.FromResult(new OrderCode_GetInfoById_Output_APIItem
            {
                BOCID = entity.BOCID,
                iCodeType = entity.CodeType,
                sCodeType = OrderCodeTypes.ContainsKey(entity.CodeType.ToString())
                    ? OrderCodeTypes[entity.CodeType.ToString()]?.Title ?? ""
                    : "",
                // CodeTypeList 在此不塞值 
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                PrintTitle = entity.PrintTitle ?? "",
                PrintNote = entity.PrintNote ?? "",
                SortNo = entity.SortNo
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

        public IQueryable<B_OrderCode> ChangeActiveQuery(int id)
        {
            return DC.B_OrderCode.Where(oc => oc.BOCID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<B_OrderCode> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.B_OrderCode.Where(oc => ids.Contains(oc.BOCID));
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(OrderCode_Submit_Input_APIItem.BOCID))]
        public async Task<string> Submit(OrderCode_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(OrderCode_Submit_Input_APIItem input)
        {
            return input.BOCID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(OrderCode_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BOCID == 0,
                    () => AddError(WrongFormat("入帳代號 ID", nameof(input.BOCID))))
                .Validate(i => i.Title.HasContent(),
                    () => AddError(EmptyNotAllowed("入帳代號名稱", nameof(input.Title))))
                .Validate(i => i.PrintTitle.HasContent(),
                    () => AddError(EmptyNotAllowed("帳單列印名稱", nameof(input.PrintTitle))))
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(OutOfRange("編碼", nameof(input.Code), 0, 10)))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    () => AddError(OutOfRange("名稱", nameof(input.Title), 1, 60)))
                .Validate(i => i.PrintTitle.HasLengthBetween(1, 100),
                    () => AddError(OutOfRange("帳單列印名稱", nameof(input.PrintTitle), 1, 100)))
                .Validate(i => i.PrintNote.HasLengthBetween(0, 100),
                    () => AddError(OutOfRange("帳單列印說明", nameof(input.PrintNote), 0, 100)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<B_OrderCode> SubmitCreateData(OrderCode_Submit_Input_APIItem input)
        {
            return new B_OrderCode
            {
                CodeType = input.CodeType,
                Code = input.Code,
                Title = input.Title ?? throw new ArgumentNullException(nameof(input.Title)),
                PrintTitle = input.PrintTitle ?? throw new ArgumentNullException(nameof(input.PrintTitle)),
                PrintNote = input.PrintNote ?? "",
                SortNo = await DC.B_OrderCode
                    .Where(sc => sc.CodeType == input.CodeType)
                    .OrderBy(sc => sc.SortNo)
                    .Select(sc => sc.SortNo)
                    .FirstOrDefaultAsync() + 1
            };
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(OrderCode_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BOCID.IsAboveZero(),
                    () => AddError(WrongFormat("入帳代號 ID", nameof(input.BOCID))))
                .Validate(i => i.Title.HasContent(),
                    () => AddError(EmptyNotAllowed("入帳代號名稱", nameof(input.Title))))
                .Validate(i => i.PrintTitle.HasContent(),
                    () => AddError(EmptyNotAllowed("帳單列印名稱", nameof(input.PrintTitle))))
                .Validate(i => i.Code.HasLengthBetween(0, 10),
                    () => AddError(OutOfRange("編碼", nameof(input.Code), 0, 10)))
                .Validate(i => i.Title.HasLengthBetween(1, 60),
                    () => AddError(OutOfRange("名稱", nameof(input.Title), 1, 60)))
                .Validate(i => i.PrintTitle.HasLengthBetween(1, 100),
                    () => AddError(OutOfRange("帳單列印名稱", nameof(input.PrintTitle), 1, 100)))
                .Validate(i => i.PrintNote.HasLengthBetween(0, 100),
                    () => AddError(OutOfRange("帳單列印說明", nameof(input.PrintNote), 0, 100)))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<B_OrderCode> SubmitEditQuery(OrderCode_Submit_Input_APIItem input)
        {
            return DC.B_OrderCode.Where(oc => oc.ActiveFlag && oc.BOCID == input.BOCID);
        }

        public void SubmitEditUpdateDataFields(B_OrderCode data, OrderCode_Submit_Input_APIItem input)
        {
            data.BOCID = input.BOCID;
            data.CodeType = input.CodeType;
            data.Code = input.Code ?? data.Code;
            data.Title = input.Title ?? data.Title;
            data.PrintTitle = input.PrintTitle ?? data.PrintTitle;
            data.PrintNote = input.PrintNote ?? "";
        }

        #endregion

        #endregion
    }
}