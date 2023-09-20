using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetList;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetLogKeepDays;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.GetTypeList;
using NS_Education.Models.APIItems.Controller.UserData.UserLog.SubmitLogKeepDays;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;
using WebGrease.Css.Extensions;

namespace NS_Education.Controller.UsingHelper.UserDataController
{
    /// <summary>
    /// 處理操作紀錄 Log 的 API。<br/>
    /// 處理的是 UserLog，但因為目前開的 Route 為 UserData，因此還是歸類在 UserDataController，
    /// </summary>
    public class UserDataLogController : PublicClass,
        IGetListLocal<UserLog_GetTypeList_Output_APIItem>,
        IGetListPaged<UserLogView, UserLog_GetList_Input_APIItem, UserLog_GetList_Output_Row_APIItem>
    {
        private static readonly string[] UserLogTypes = { "瀏覽", "新增", "修改", "刪除" };
        private static readonly string[] UserPasswordLogTypes = { "成功登入系統", "從系統登出", "更改密碼" };

        private static readonly IList<UserLog_GetTypeList_Output_APIItem> typeList = UserLogTypes
            .Select((s, i) => new UserLog_GetTypeList_Output_APIItem
            {
                Title = s,
                UserLogType = i,
                UserPasswordLogType = -i - 1,
                Type = (UserLogType)(0 + i)
            })
            .Concat(UserPasswordLogTypes.Select((s, i) => new UserLog_GetTypeList_Output_APIItem
            {
                Title = s,
                UserLogType = -i - 1,
                UserPasswordLogType = i,
                Type = (UserLogType)(4 + i)
            })).ToSafeReadOnlyCollection();

        private readonly IGetListLocalHelper _getListLocalHelper;
        private readonly IGetListPagedHelper<UserLog_GetList_Input_APIItem> _getListPagedHelper;

        #region Initialization

        public UserDataLogController()
        {
            _getListLocalHelper =
                new GetListLocalHelper<UserDataLogController, UserLog_GetTypeList_Output_APIItem>(this);
            _getListPagedHelper =
                new GetListPagedHelper<UserDataLogController, UserLogView, UserLog_GetList_Input_APIItem,
                    UserLog_GetList_Output_Row_APIItem>(this);
        }

        #endregion

        #region SubmitLogKeepDays

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> SubmitLogKeepDays(UserLog_SubmitLogKeepDays_Input_APIItem input)
        {
            // 這支輸入沒有 ActiveFlag，所以不使用 Helper
            B_StaticCode data = await DC.B_StaticCode
                .Where(sc => sc.BSCID == DbConstants.SafetyControlLogKeepDaysBSCID)
                .FirstOrDefaultAsync();

            if (data is null)
            {
                AddError(NotFound());
                return GetResponseJson();
            }

            try
            {
                data.SortNo = input.KeepDays;
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }

            return GetResponseJson();
        }

        #endregion

        #region GetLogKeepDays

        /// <summary>
        /// 處理取得紀錄保留天數的設定值的端點，實際 Route 請參照 RouteConfig。
        /// </summary>
        /// <returns>紀錄保留天數（通用訊息回傳格式）</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetLogKeepDays()
        {
            // 無輸入，無法使用 helper。
            // 1. 查詢資料
            B_StaticCode keepDaysStaticCode =
                await DC.B_StaticCode.FirstOrDefaultAsync(sc => sc.BSCID == DbConstants.SafetyControlLogKeepDaysBSCID);

            if (keepDaysStaticCode == null)
            {
                AddError(NotFound());
                return GetResponseJson();
            }

            // 2. 轉成回傳物件，設值，回傳
            UserLog_GetLogKeepDays_Output_APIItem response = new UserLog_GetLogKeepDays_Output_APIItem
            {
                KeepDays = keepDaysStaticCode.SortNo
            };

            return GetResponseJson(response);
        }

        #endregion

        #region GetList

        [NonAction]
        public Task<string> GetList(UserLog_GetList_Input_APIItem input)
        {
            throw new NotImplementedException("Use GetUserLogList instead.");
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetUserLogList(UserLog_GetList_Input_APIItem input)
        {
            // 如果有輸入 Type，覆寫篩選
            GetUserLogListSanitizeInput(input);
            return await _getListPagedHelper.GetPagedList(input);
        }

        private void GetUserLogListSanitizeInput(UserLog_GetList_Input_APIItem input)
        {
            UserLogType? type = (UserLogType?)input.Type;

            if (type == null)
                return;

            switch (type)
            {
                case UserLogType.Show:
                    input.UserLogType = 0;
                    input.UserPasswordLogType = -1;
                    break;
                case UserLogType.Add:
                    input.UserLogType = 1;
                    input.UserPasswordLogType = -1;
                    break;
                case UserLogType.Edit:
                    input.UserLogType = 2;
                    input.UserPasswordLogType = -1;
                    break;
                case UserLogType.Delete:
                    input.UserLogType = 3;
                    input.UserPasswordLogType = -1;
                    break;
                case UserLogType.Login:
                    input.UserLogType = -1;
                    input.UserPasswordLogType = 0;
                    break;
                case UserLogType.Logout:
                    input.UserLogType = -1;
                    input.UserPasswordLogType = 1;
                    break;
                case UserLogType.ChangePassword:
                    input.UserLogType = -1;
                    input.UserPasswordLogType = 2;
                    break;
                default:
                    AddError(NotSupportedValue("查詢種類", nameof(input.Type), "未知的 Log 種類！"));
                    break;
            }
        }

        public async Task<bool> GetListPagedValidateInput(UserLog_GetList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.NowPage.IsZeroOrAbove(), () => AddError(WrongFormat("查詢分頁", nameof(input.NowPage))))
                .Validate(i => i.CutPage.IsZeroOrAbove(), () => AddError(WrongFormat("分頁筆數", nameof(input.CutPage))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<UserLogView> GetListPagedOrderedQuery(UserLog_GetList_Input_APIItem input)
        {
            var query = DC.UserLogView
                .AsNoTracking()
                .AsQueryable();

            if (input.Keyword.HasContent())
                query = query.Where(ulv =>
                    ulv.TableName.Contains(input.Keyword) || ulv.RequestUrl.Contains(input.Keyword));

            if (input.Type.HasValue)
                query = query.Where(ulv => ulv.TypeNo == input.Type);
            else if (input.UserLogType > -1)
                query = query.Where(ulv => ulv.TypeNo == input.UserLogType);
            else if (input.UserPasswordLogType > -1)
                query = query.Where(ulv => ulv.TypeNo == input.UserPasswordLogType + 4);

            return query.OrderByDescending(ulv => ulv.CreDate);
        }

        public async Task<UserLog_GetList_Output_Row_APIItem> GetListPagedEntityToRow(UserLogView entity)
        {
            return await Task.FromResult(new UserLog_GetList_Output_Row_APIItem
            {
                Time = entity.CreDate.ToFormattedStringDateTime(),
                Actor = entity.UserName,
                EventType = entity.Type,
                Description = GetDescription(entity),
                CreDate = entity.CreDate
            });
        }

        private static string GetDescription(UserLogView entity)
        {
            StringBuilder sb = new StringBuilder("使用者");

            sb.Append(entity.Type);

            if (entity.TableName.HasContent())
                sb.Append(entity.TableName + (entity.TableName.EndsWith("檔") ? "" : "檔"));

            return sb.ToString();
        }

        #endregion

        #region GetTypeList

        /// <summary>
        /// 提供「事件類型」的端點，確切 Route 請參考 RouteConfig。
        /// </summary>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            return await _getListLocalHelper.GetListLocal();
        }

        /// <inheritdoc />
        public async Task<ICollection<UserLog_GetTypeList_Output_APIItem>> GetListLocalResults()
        {
            return await Task.FromResult(typeList);
        }

        #endregion
    }
}