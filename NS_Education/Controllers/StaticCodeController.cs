using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NS_Education.Models.APIItems;
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
        private readonly IGetTypeListHelper _getTypeListHelper;
        private readonly IGetListPagedHelper<StaticCode_GetList_Input_APIItem> _getListHelper;

        private Dictionary<string, B_StaticCode> StaticCodeTypesInner { get; set; }

        /// <summary>
        /// 靜態參數類別名稱對照表。<br/>
        /// 每次實際被取用時，若 Inner 沒有值，才作查詢並 Populate。<br/>
        /// 在 ASP.NET 中，端點每次被呼叫都會是新的 Controller，所以沒有需要 refresh 的問題。
        /// </summary>
        private Dictionary<string, B_StaticCode> StaticCodeTypes
        {
            get
            {
                return StaticCodeTypesInner ?? (StaticCodeTypesInner = DC.B_StaticCode
                    .Where(sc => sc.ActiveFlag && !sc.DeleteFlag)
                    .Where(sc => sc.CodeType == 0)
                    .ToDictionary(sc => sc.Code, sc => sc));
            }
        }
            
        public StaticCodeController()
        {
            _getTypeListHelper = new GetTypeListHelper<StaticCodeController, B_StaticCode>(this);
            _getListHelper =
                new GetListPagedHelper<StaticCodeController
                    , B_StaticCode
                    , StaticCode_GetList_Input_APIItem
                    , StaticCode_GetList_Output_Row_APIItem>(this);
        }

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
                    sCodeType = StaticCodeTypes.ContainsKey(entity.CodeType.ToString()) ? StaticCodeTypes[entity.CodeType.ToString()]?.Title ?? "" : "",
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