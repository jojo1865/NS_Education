using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.TimeSpan;
using NS_Education.Models.APIItems.TimeSpan.GetList;
using NS_Education.Models.APIItems.TimeSpan.Submit;
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
    public class TimeSpanController : PublicClass,
        IGetListPaged<D_TimeSpan, TimeSpan_GetList_Input_APIItem, TimeSpan_GetList_Output_Row_APIItem>,
        IDeleteItem<D_TimeSpan>,
        ISubmit<D_TimeSpan, TimeSpan_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<TimeSpan_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<TimeSpan_Submit_Input_APIItem> _submitHelper;

        public TimeSpanController()
        {
            _getListPagedHelper = new GetListPagedHelper<TimeSpanController, D_TimeSpan, TimeSpan_GetList_Input_APIItem,
                TimeSpan_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new DeleteItemHelper<TimeSpanController, D_TimeSpan>(this);
            _submitHelper = new SubmitHelper<TimeSpanController, D_TimeSpan, TimeSpan_Submit_Input_APIItem>(this);
        }

        #endregion
        
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(TimeSpan_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(TimeSpan_GetList_Input_APIItem input)
        {
            // 輸入無須驗證
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<D_TimeSpan> GetListPagedOrderedQuery(TimeSpan_GetList_Input_APIItem input)
        {
            var query = DC.D_TimeSpan.AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(ts => ts.Title.Contains(input.Keyword) || ts.Code.Contains(input.Keyword));

            return query.OrderBy(ts => ts.HourS)
                .ThenBy(ts => ts.MinuteS)
                .ThenBy(ts => ts.HourE)
                .ThenBy(ts => ts.MinuteE)
                .ThenBy(ts => ts.DTSID);
        }

        public async Task<TimeSpan_GetList_Output_Row_APIItem> GetListPagedEntityToRow(D_TimeSpan entity)
        {
            // 計算 GetTimespan
            // 將兩種時間都換算成總分鐘數, 然後再相減
            int timeDiff = entity.HourS * 60 + entity.MinuteS - entity.HourE * 60 + entity.MinuteE;
            // 如果是負數的情況，當成 0 輸出
            timeDiff = Math.Max(timeDiff, 0);
            return await Task.FromResult(new TimeSpan_GetList_Output_Row_APIItem
            {
                DTSID = entity.DTSID,
                Code = entity.Code,
                Title = entity.Title,
                HourS = entity.HourS,
                MinuteS = entity.MinuteS,
                HourE = entity.HourE,
                MinuteE = entity.MinuteE,
                TimeS = (entity.HourS, entity.MinuteS).ToFormattedHourAndMinute(),
                TimeE = (entity.HourE, entity.MinuteE).ToFormattedHourAndMinute(),
                GetTimeSpan = timeDiff > 60 ? $"{timeDiff/60}小時{timeDiff%60}分鐘" : $"{timeDiff%60}分鐘"
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_TimeSpan.FirstOrDefaultAsync(q => q.DTSID == ID && !q.DeleteFlag);
            D_TimeSpan_APIItem Item = null;
            if (N != null)
            {
                DateTime DT_S = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourS + ":" +
                                                   N.MinuteS + ":00");
                DateTime DT_E = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourE + ":" +
                                                   N.MinuteE + ":00");
                TimeSpan TS = DT_E - DT_S;


                Item = new D_TimeSpan_APIItem
                {
                    DTSID = N.DTSID,

                    Code = N.Code,
                    Title = N.Title,

                    HourS = N.HourS,
                    MinuteS = N.MinuteS,
                    HourE = N.HourE,
                    MinuteE = N.MinuteE,
                    TimeS = N.HourS.ToString().PadLeft(2, '0') + ":" + N.MinuteS.ToString().PadLeft(2, '0'),
                    TimeE = N.HourE.ToString().PadLeft(2, '0') + ":" + N.MinuteE.ToString().PadLeft(2, '0'),
                    GetTimeSpan = (TS.Hours > 0 ? TS.Hours + "小時" : "") + TS.Minutes.ToString() + "分鐘",

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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.D_TimeSpan.FirstOrDefaultAsync(q => q.DTSID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesWithLogAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<D_TimeSpan> DeleteItemQuery(int id)
        {
            return DC.D_TimeSpan.Where(ts => ts.DTSID == id);
        }

        #endregion

        #region Submit

        private const string SubmitWrongStartTime = "起始時間應小於等於結束時間！";
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(TimeSpan_Submit_Input_APIItem.DTSID))]
        public async Task<string> Submit(TimeSpan_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(TimeSpan_Submit_Input_APIItem input)
        {
            return input.DTSID == 0;
        }

        #region Submit - Add
        public async Task<bool> SubmitAddValidateInput(TimeSpan_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DTSID == 0, () => AddError(WrongFormat("時段 ID")))
                .Validate(i => 0 <= i.HourS && i.HourS <= 23, () => AddError(WrongFormat("起始小時")))
                .Validate(i => 0 <= i.HourE && i.HourE <= 23, () => AddError(WrongFormat("結束小時")))
                .Validate(i => 0 <= i.MinuteS && i.MinuteS <= 59, () => AddError(WrongFormat("起始分鐘")))
                .Validate(i => 0 <= i.MinuteE && i.MinuteE <= 59, () => AddError(WrongFormat("結束分鐘")))
                .Validate(
                    i => input.HourS < input.HourE || input.HourS == input.HourE && input.MinuteS <= input.MinuteE,
                    () => AddError(SubmitWrongStartTime))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public async Task<D_TimeSpan> SubmitCreateData(TimeSpan_Submit_Input_APIItem input)
        {
            return await Task.FromResult(new D_TimeSpan
            {
                Code = input.Code,
                Title = input.Title,
                HourS = input.HourS,
                MinuteS = input.MinuteS,
                HourE = input.HourE,
                MinuteE = input.MinuteE
            });
        }
        #endregion

        #region Submit - Edit
        public async Task<bool> SubmitEditValidateInput(TimeSpan_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.DTSID.IsValidId(), () => AddError(EmptyNotAllowed("時段 ID")))
                .Validate(i => 0 <= i.HourS && i.HourS <= 23, () => AddError(WrongFormat("起始小時")))
                .Validate(i => 0 <= i.HourE && i.HourE <= 23, () => AddError(WrongFormat("結束小時")))
                .Validate(i => 0 <= i.MinuteS && i.MinuteS <= 59, () => AddError(WrongFormat("起始分鐘")))
                .Validate(i => 0 <= i.MinuteE && i.MinuteE <= 59, () => AddError(WrongFormat("結束分鐘")))
                .Validate(
                    i => input.HourS < input.HourE || input.HourS == input.HourE && input.MinuteS <= input.MinuteE,
                    () => AddError(SubmitWrongStartTime))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<D_TimeSpan> SubmitEditQuery(TimeSpan_Submit_Input_APIItem input)
        {
            return DC.D_TimeSpan.Where(ts => ts.DTSID == input.DTSID);
        }

        public void SubmitEditUpdateDataFields(D_TimeSpan data, TimeSpan_Submit_Input_APIItem input)
        {
            data.Code = input.Code;
            data.Title = input.Title;
            data.HourS = input.HourS;
            data.MinuteS = input.MinuteS;
            data.HourE = input.HourE;
            data.MinuteE = input.MinuteE;
        }
        #endregion

        #endregion
    }
}