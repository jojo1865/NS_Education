using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.CustomerQuestion.GetInfoById;
using NS_Education.Models.APIItems.CustomerQuestion.GetList;
using NS_Education.Models.APIItems.CustomerQuestion.Submit;
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
    public class CustomerQuestionController : PublicClass,
        IGetListPaged<CustomerQuestion, CustomerQuestion_GetList_Input_APIItem,
            CustomerQuestion_GetList_Output_Row_APIItem>,
        IGetInfoById<CustomerQuestion, CustomerQuestion_GetInfoById_Output_APIItem>,
        IDeleteItem<CustomerQuestion>,
        ISubmit<CustomerQuestion, CustomerQuestion_Submit_Input_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<CustomerQuestion_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IGetInfoByIdHelper _getInfoByIdHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<CustomerQuestion_Submit_Input_APIItem> _submitHelper;

        public CustomerQuestionController()
        {
            _getListPagedHelper =
                new GetListPagedHelper<CustomerQuestionController, CustomerQuestion,
                    CustomerQuestion_GetList_Input_APIItem, CustomerQuestion_GetList_Output_Row_APIItem>(this);

            _getInfoByIdHelper =
                new GetInfoByIdHelper<CustomerQuestionController, CustomerQuestion,
                    CustomerQuestion_GetInfoById_Output_APIItem>(this);

            _deleteItemHelper =
                new DeleteItemHelper<CustomerQuestionController, CustomerQuestion>(this);

            _submitHelper =
                new SubmitHelper<CustomerQuestionController, CustomerQuestion, CustomerQuestion_Submit_Input_APIItem>(
                    this);
        }

        #endregion

        #region GetList

        private const string GetListDateRangeIncorrect = "欲篩選之問題發生期間起始日期不得大於最後日期！";

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(CustomerQuestion_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(CustomerQuestion_GetList_Input_APIItem input)
        {
            DateTime sDate = default;
            DateTime eDate = default;
            bool isValid = input.StartValidate()
                .Validate(i => i.CID.IsValidIdOrZero(), () => AddError(WrongFormat("欲篩選之客戶 ID")))
                .Validate(i => i.SDate.IsNullOrWhiteSpace() || i.SDate.TryParseDateTime(out sDate),
                    () => AddError(WrongFormat("欲篩選之拜訪期間起始日期")))
                .Validate(i => i.EDate.IsNullOrWhiteSpace() || i.EDate.TryParseDateTime(out eDate),
                    () => AddError(WrongFormat("欲篩選之拜訪期間最後日期")))
                .Validate(i => sDate.Date <= eDate.Date, () => AddError(GetListDateRangeIncorrect))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<CustomerQuestion> GetListPagedOrderedQuery(
            CustomerQuestion_GetList_Input_APIItem input)
        {
            var query = DC.CustomerQuestion
                .Include(cq => cq.C)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(cq => cq.AskTitle.Contains(input.Keyword) || cq.AskArea.Contains(input.Keyword));

            if (input.CID.IsValidId())
                query = query.Where(cq => cq.CID == input.CID);

            if (input.SDate.TryParseDateTime(out DateTime sDate))
                query = query.Where(cq => cq.AskDate.Date >= sDate.Date);

            if (input.EDate.TryParseDateTime(out DateTime eDate))
                query = query.Where(cq => cq.AskDate.Date <= eDate.Date);

            if (input.ResponseType.IsInBetween(0, 1))
                query = query.Where(cq => cq.ResponseFlag == (input.ResponseType == 1));

            return query.OrderBy(cq => cq.ResponseFlag)
                .ThenByDescending(cq => cq.AskDate)
                .ThenBy(cq => cq.CQID);
        }

        public async Task<CustomerQuestion_GetList_Output_Row_APIItem> GetListPagedEntityToRow(CustomerQuestion entity)
        {
            return await Task.FromResult(new CustomerQuestion_GetList_Output_Row_APIItem
            {
                CQID = entity.CQID,
                CID = entity.CID,
                C_TitleC = entity.C?.TitleC ?? "",
                C_TitleE = entity.C?.TitleE ?? "",
                AskDate = entity.AskDate.ToFormattedStringDate(),
                AskTitle = entity.AskTitle ?? "",
                AskArea = entity.AskArea ?? "",
                AskDescription = entity.AskDescription ?? "",
                ResponseFlag = entity.ResponseFlag,
                ResponseUser = entity.ResponseFlag ? entity.ResponseUser ?? "" : "",
                ResponseDescription = entity.ResponseFlag ? entity.ResponseDestriotion ?? "" : "",
                ResponseDate = entity.ResponseFlag ? entity.ResponseDate.ToFormattedStringDate() : ""
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

        public IQueryable<CustomerQuestion> GetInfoByIdQuery(int id)
        {
            return DC.CustomerQuestion
                .Include(cq => cq.C)
                .Where(cq => cq.CQID == id);
        }

        public async Task<CustomerQuestion_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(
            CustomerQuestion entity)
        {
            return await Task.FromResult(new CustomerQuestion_GetInfoById_Output_APIItem
            {
                CQID = entity.CQID,
                CID = entity.CID,
                C_TitleC = entity.C?.TitleC ?? "",
                C_TitleE = entity.C?.TitleE ?? "",
                C_List = await DC.Customer.GetCustomerSelectable(entity.CID),
                AskDate = entity.AskDate.ToFormattedStringDate(),
                AskTitle = entity.AskTitle ?? "",
                AskArea = entity.AskArea ?? "",
                AskDescription = entity.AskDescription ?? "",
                ResponseFlag = entity.ResponseFlag,
                ResponseUser = entity.ResponseFlag ? entity.ResponseUser ?? "" : "",
                ResponseDescription = entity.ResponseFlag ? entity.ResponseDestriotion ?? "" : "",
                ResponseDate = entity.ResponseFlag ? entity.ResponseDate.ToFormattedStringDate() : ""
            });
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<CustomerQuestion> DeleteItemQuery(int id)
        {
            return DC.CustomerQuestion.Where(cq => cq.CQID == id);
        }

        #endregion

        #region Submit

        private string SubmitResponseDateNotAfterAskDate = "回答時間不得小於問題發生時間！";
        
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(CustomerQuestion_Submit_Input_APIItem.CQID))]
        public async Task<string> Submit(CustomerQuestion_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(CustomerQuestion_Submit_Input_APIItem input)
        {
            return input.CQID == 0;
        }

        #region Submit - Add

        public async Task<bool> SubmitAddValidateInput(CustomerQuestion_Submit_Input_APIItem input)
        {
            DateTime askDate = default;
            DateTime responseDate = default;
            var validation = input.StartValidate(true)
                .Validate(i => i.CQID == 0, () => AddError(WrongFormat("問題紀錄 ID")))
                .Validate(i => i.CID.IsValidId(), () => AddError(EmptyNotAllowed("客戶 ID")))
                .Validate(i => i.AskDate.TryParseDateTime(out askDate), () => AddError(WrongFormat("問題發生時間")))
                .Validate(i => !i.AskTitle.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("問題主旨")))
                .Validate(i => !i.AskDescription.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("問題內容")));

            // 若傳入內容表示已回答，則回答者相關的欄位需要檢核
            if (input.ResponseFlag)
            {
                validation.Validate(i => !i.ResponseUser.IsNullOrWhiteSpace(),
                        () => AddError(EmptyNotAllowed("回答者姓名")))
                    .Validate(i => !i.ResponseDescription.IsNullOrWhiteSpace(),
                        () => AddError(EmptyNotAllowed("回答內容")))
                    .Validate(i => i.ResponseDate.TryParseDateTime(out responseDate),
                        () => AddError(WrongFormat("回答時間")))
                    .Validate(i => responseDate >= askDate,
                        () => AddError(SubmitResponseDateNotAfterAskDate));
            }

            return await Task.FromResult(validation.IsValid());
        }

        public async Task<CustomerQuestion> SubmitCreateData(CustomerQuestion_Submit_Input_APIItem input)
        {
            input.AskDate.TryParseDateTime(out var askDate);
            
            // 只在已回答狀態時才處理 ResponseDate
            DateTime responseDate = default;
            if (input.ResponseFlag)
                input.ResponseDate.TryParseDateTime(out responseDate);
            
            return await Task.FromResult(new CustomerQuestion
            {
                CID = input.CID,
                AskDate = askDate,
                AskTitle = input.AskTitle,
                AskArea = input.AskArea,
                AskDescription = input.AskDescription,
                ResponseFlag = input.ResponseFlag,
                ResponseUser = input.ResponseUser,
                ResponseDestriotion = input.ResponseDescription,
                ResponseDate = responseDate
            });
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(CustomerQuestion_Submit_Input_APIItem input)
        {
            DateTime askDate = default;
            DateTime responseDate = default;
            var validation = input.StartValidate(true)
                .Validate(i => i.CQID.IsValidId(), () => AddError(EmptyNotAllowed("問題紀錄 ID")))
                .Validate(i => i.CID.IsValidId(), () => AddError(EmptyNotAllowed("客戶 ID")))
                .Validate(i => i.AskDate.TryParseDateTime(out askDate), () => AddError(WrongFormat("問題發生時間")))
                .Validate(i => !i.AskTitle.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("問題主旨")))
                .Validate(i => !i.AskDescription.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("問題內容")));

            // 若傳入內容表示已回答，則回答者相關的欄位需要檢核
            if (input.ResponseFlag)
            {
                validation.Validate(i => !i.ResponseUser.IsNullOrWhiteSpace(),
                        () => AddError(EmptyNotAllowed("回答者姓名")))
                    .Validate(i => !i.ResponseDescription.IsNullOrWhiteSpace(),
                        () => AddError(EmptyNotAllowed("回答內容")))
                    .Validate(i => i.ResponseDate.TryParseDateTime(out responseDate),
                        () => AddError(WrongFormat("回答時間")))
                    .Validate(i => responseDate >= askDate,
                        () => AddError(SubmitResponseDateNotAfterAskDate));
            }

            return await Task.FromResult(validation.IsValid());
        }

        public IQueryable<CustomerQuestion> SubmitEditQuery(CustomerQuestion_Submit_Input_APIItem input)
        {
            return DC.CustomerQuestion.Where(cq => cq.CQID == input.CQID);
        }

        public void SubmitEditUpdateDataFields(CustomerQuestion data, CustomerQuestion_Submit_Input_APIItem input)
        {
            input.AskDate.TryParseDateTime(out var askDate);
            
            // 只在已回答狀態時才處理 ResponseDate
            DateTime responseDate = default;
            if (input.ResponseFlag)
                input.ResponseDate.TryParseDateTime(out responseDate);
            
            data.CID = input.CID;
            data.AskDate = askDate;
            data.AskTitle = input.AskTitle;
            data.AskArea = input.AskArea;
            data.AskDescription = input.AskDescription;
            data.ResponseFlag = input.ResponseFlag;
            data.ResponseUser = input.ResponseUser;
            data.ResponseDestriotion = input.ResponseDescription;
            data.ResponseDate = responseDate;
        }

        #endregion

        #endregion
    }
}