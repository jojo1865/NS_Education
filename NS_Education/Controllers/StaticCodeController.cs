using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.StaticCode.GetInfoById;
using NS_Education.Models.APIItems.StaticCode.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers
{
    /// <summary>
    /// 靜態參數的 Controller。
    /// </summary>
    public class StaticCodeController : PublicClass
        , IGetTypeList<B_StaticCode>
        , IGetListPaged<B_StaticCode, StaticCode_GetList_Input_APIItem, StaticCode_GetList_Output_Row_APIItem>
    {
        #region 共用
        
        private readonly IGetTypeListHelper _getTypeListHelper;
        private readonly IGetListPagedHelper<StaticCode_GetList_Input_APIItem> _getListHelper;

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
                .ToDictionary(sc => sc.Code, sc => sc);

            _getTypeListHelper =
                new GetTypeListHelper<StaticCodeController, B_StaticCode>(this);

            _getListHelper =
                new GetListPagedHelper<StaticCodeController
                    , B_StaticCode
                    , StaticCode_GetList_Input_APIItem
                    , StaticCode_GetList_Output_Row_APIItem>(this);
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
                ID = entity.SortNo,
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
                .Validate(i => i.CodeType >= -1)
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
            return
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
                    ActiveFlag = entity.ActiveFlag,
                    CreDate = entity.CreDate.ToFormattedString(),
                    CreUser = await GetUserNameByID(entity.CreUID),
                    CreUID = entity.CreUID,
                    UpdDate = entity.UpdDate.ToFormattedString(),
                    UpdUser = await GetUserNameByID(entity.UpdUID),
                    UpdUID = entity.UpdUID
                };
        }

        #endregion

        #region GetInfoById
        
        private const string GetInfoByIdInputIncorrect = "未輸入欲查詢的 ID 或格式有誤！";
        private const string GetInfoByIdNotFound = "查無指定的資料！";
        
        private readonly StaticCode_GetInfoById_Output_APIItem _getInfoByIdDummyOutput = new StaticCode_GetInfoById_Output_APIItem
        {
            BSCID = 0,
            iCodeType = 0,
            sCodeType = null,
            CodeTypeList = null, // 在轉換方法中設值。所以這個物件不能是 static。
            Code = null,
            Title = null,
            SortNo = 0,
            Note = null,
            ActiveFlag = true,
            CreDate = null,
            CreUser = null,
            CreUID = 0,
            UpdDate = null,
            UpdUser = null,
            UpdUID = 0
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

        private async Task<StaticCode_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(B_StaticCode entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            return new StaticCode_GetInfoById_Output_APIItem
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
                Note = entity.Note ?? "",
                ActiveFlag = entity.ActiveFlag,
                CreDate = entity.CreDate.ToFormattedString(),
                CreUser = await GetUserNameByID(entity.CreUID),
                CreUID = entity.CreUID,
                UpdDate = entity.UpdDate.ToFormattedString(),
                UpdUser = await GetUserNameByID(entity.UpdUID),
                UpdUID = entity.UpdUID
            };
        }

        #endregion
    }
}