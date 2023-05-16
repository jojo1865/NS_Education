using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.TimeSpan.GetInfoById;
using NS_Education.Models.APIItems.Controller.TimeSpan.GetList;
using NS_Education.Models.APIItems.Controller.TimeSpan.Submit;
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
    public class TimeSpanController : PublicClass,
        IGetListPaged<D_TimeSpan, TimeSpan_GetList_Input_APIItem, TimeSpan_GetList_Output_Row_APIItem>,
        IGetInfoById<D_TimeSpan, TimeSpan_GetInfoById_Output_APIItem>,
        IDeleteItem<D_TimeSpan>,
        ISubmit<D_TimeSpan, TimeSpan_Submit_Input_APIItem>,
        IChangeActive<D_TimeSpan>
    {
        #region Initialization

        private readonly IGetListPagedHelper<TimeSpan_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;

        private readonly ISubmitHelper<TimeSpan_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public TimeSpanController()
        {
            _getListPagedHelper = new GetListPagedHelper<TimeSpanController, D_TimeSpan, TimeSpan_GetList_Input_APIItem,
                TimeSpan_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new DeleteItemHelper<TimeSpanController, D_TimeSpan>(this);
            _submitHelper = new SubmitHelper<TimeSpanController, D_TimeSpan, TimeSpan_Submit_Input_APIItem>(this);
            _changeActiveHelper = new ChangeActiveHelper<TimeSpanController, D_TimeSpan>(this);
            _getInfoByIdHelper = new GetInfoByIdHelper<TimeSpanController, D_TimeSpan, TimeSpan_GetInfoById_Output_APIItem>(this);
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
                PriceRate = (entity.PriceRatePercentage / 100m).ToString("0.00", CultureInfo.InvariantCulture),
                GetTimeSpan = (entity.HourS, entity.MinuteS).FormatTimeSpanUntil((entity.HourE, entity.MinuteE))
            });
        }

        #endregion

        #region GetInfoById

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<D_TimeSpan> GetInfoByIdQuery(int id)
        {
            return DC.D_TimeSpan.Where(ts => ts.DTSID == id);
        }

        public async Task<TimeSpan_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(D_TimeSpan entity)
        {
            return await Task.FromResult(new TimeSpan_GetInfoById_Output_APIItem
            {
                DTSID = entity.DTSID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                HourS = entity.HourS,
                MinuteS = entity.MinuteS,
                HourE = entity.HourE,
                MinuteE = entity.MinuteE,
                TimeS = (entity.HourS, entity.MinuteS).ToFormattedHourAndMinute(),
                TimeE = (entity.HourE, entity.MinuteE).ToFormattedHourAndMinute(),
                PriceRate = (entity.PriceRatePercentage / 100m).ToString("0.00", CultureInfo.InvariantCulture),
                GetTimeSpan = (entity.HourS, entity.MinuteS).FormatTimeSpanUntil((entity.HourE, entity.MinuteE))
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

        public IQueryable<D_TimeSpan> ChangeActiveQuery(int id)
        {
            return DC.D_TimeSpan.Where(ts => ts.DTSID == id);
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<D_TimeSpan> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.D_TimeSpan.Where(ts => ids.Contains(ts.DTSID));
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

            decimal priceRate = 0m;
            
            bool isValid = input.StartValidate()
                .Validate(i => i.DTSID == 0, () => AddError(WrongFormat("時段 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("編碼")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.HourS.IsInBetween(0, 23), () => AddError(OutOfRange("起始小時", 0, 23)))
                .Validate(i => i.HourE.IsInBetween(0, 24), () => AddError(OutOfRange("結束小時", 0, 24)))
                .Validate(i => i.MinuteS.IsInBetween(0, 59), () => AddError(OutOfRange("起始分鐘", 0, 59)))
                .Validate(i => i.MinuteE.IsInBetween(0, 59), () => AddError(OutOfRange("結束分鐘", 0, 59)))
                .Validate(
                    i => input.HourS < input.HourE || input.HourS == input.HourE && input.MinuteS <= input.MinuteE,
                    () => AddError(SubmitWrongStartTime))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.PriceRate.HasContent(), () => AddError(EmptyNotAllowed("價格基數")))
                .Validate(i => decimal.TryParse(i.PriceRate, out priceRate), () => AddError(WrongFormat("價格基數")))
                .Validate(i => priceRate.IsInBetween(0m, 1m), () => AddError(OutOfRange("價格基數", 0, 1)))
                .Validate(i => (priceRate * 100m % 1m) == 0m, () => AddError(TooLong("價格基數的小數後位數", 2)))
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
                MinuteE = input.MinuteE,
                PriceRatePercentage = (int)(decimal.Parse(input.PriceRate) * 100m) // 小數後會被捨去
            });
        }
        #endregion

        #region Submit - Edit
        public async Task<bool> SubmitEditValidateInput(TimeSpan_Submit_Input_APIItem input)
        {
            decimal priceRate = 0m;
            
            bool isValid = input.StartValidate()
                .Validate(i => i.DTSID.IsAboveZero(), () => AddError(EmptyNotAllowed("時段 ID")))
                .Validate(i => i.Code.HasContent(), () => AddError(EmptyNotAllowed("編碼")))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("中文名稱")))
                .Validate(i => i.HourS.IsInBetween(0, 23), () => AddError(OutOfRange("起始小時", 0, 23)))
                .Validate(i => i.HourE.IsInBetween(0, 24), () => AddError(OutOfRange("結束小時", 0, 24)))
                .Validate(i => i.MinuteS.IsInBetween(0, 59), () => AddError(OutOfRange("起始分鐘", 0, 59)))
                .Validate(i => i.MinuteE.IsInBetween(0, 59), () => AddError(OutOfRange("結束分鐘", 0, 59)))
                .Validate(
                    i => input.HourS < input.HourE || input.HourS == input.HourE && input.MinuteS <= input.MinuteE,
                    () => AddError(SubmitWrongStartTime))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.PriceRate.HasContent(), () => AddError(EmptyNotAllowed("價格基數")))
                .Validate(i => decimal.TryParse(i.PriceRate, out priceRate), () => AddError(WrongFormat("價格基數")))
                .Validate(i => priceRate.IsInBetween(0m, 1m), () => AddError(OutOfRange("價格基數", 0, 1)))
                .Validate(i => (priceRate * 100m % 1m) == 0m, () => AddError(TooLong("價格基數的小數後位數", 2)))
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
            data.PriceRatePercentage = (int)(decimal.Parse(input.PriceRate) * 100m); // 小數後會被捨去
        }
        #endregion

        #endregion
    }
}