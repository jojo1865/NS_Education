using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.UserDataController
{
    /// <summary>
    /// 處理操作紀錄 Log 的 API。<br/>
    /// 處理的是 UserLog，但因為目前開的 Route 為 UserData，因此還是歸類在 UserDataController，
    /// </summary>
    public class UserDataLogController : PublicClass,
        IGetListPaged<UserLog, UserLog_GetList_Input_APIItem, UserLog_GetList_Output_Row_APIItem>
    {
        private static readonly string[] ControlTypes = { "瀏覽", "新增", "修改", "刪除" };

        #region Initialization

        private readonly IGetListPagedHelper<UserLog_GetList_Input_APIItem> _getListPagedHelper;

        public UserDataLogController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<UserDataLogController, UserLog, UserLog_GetList_Input_APIItem,
                    UserLog_GetList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetList
        
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(UserLog_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(UserLog_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.UID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之使用者 ID")))
                .Validate(i => i.TargetID.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之目標資料 ID")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<UserLog> GetListPagedOrderedQuery(UserLog_GetList_Input_APIItem input)
        {
            DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var query = DC.UserLog
                .Include(ul => ul.UserData)
                // 三個月內
                .Where(ul => threeMonthsAgo <= ul.CreDate)
                .AsQueryable();

            if (input.UID.IsAboveZero())
                query = query.Where(ul => ul.UID == input.UID);

            if (!input.TargetTable.IsNullOrWhiteSpace())
                query = query.Where(ul => ul.TargetTable.Contains(input.TargetTable));

            if (input.TargetID.IsAboveZero())
                query = query.Where(ul => ul.TargetID == input.TargetID);
            
            // 由新到舊
            return query.OrderByDescending(ul => ul.ULID);
        }

        public async Task<UserLog_GetList_Output_Row_APIItem> GetListPagedEntityToRow(UserLog entity)
        {
            return await Task.FromResult(new UserLog_GetList_Output_Row_APIItem
            {
                ULID = entity.ULID,
                UID = entity.UID,
                UserName = entity.UserData?.UserName ?? "",
                TargetTable = entity.TargetTable ?? "",
                TargetID = entity.TargetID,
                ControlType = entity.ControlType < ControlTypes.Length 
                    ? ControlTypes[entity.ControlType]
                    : "",
                RequestUrl = entity.RequestUrl ?? ""
            });
        }
        #endregion
    }
}