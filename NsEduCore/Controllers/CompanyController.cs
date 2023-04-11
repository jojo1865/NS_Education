using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NsEduCore.Controllers.Messages;
using NsEduCore.Requests.Company;
using NsEduCore.Responses.Company;
using NsEduCore_DAL.Domains;
using NsEduCore_DAL.Services.Company;
using NsEduCore_DAL.Services.User;
using NsEduCore_Tools.Extensions;

namespace NsEduCore.Controllers
{
    /// <summary>
    /// 公司資料相關的 Controller。
    /// </summary>
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;

        /// <summary>
        /// 建立一個 CompanyController。
        /// </summary>
        /// <param name="companyService">ICompanyService</param>
        /// <param name="userService">IUserService</param>
        public CompanyController(ICompanyService companyService, IUserService userService)
        {
            _companyService = companyService;
            _userService = userService;
        }

        #region GetList
        
        /// <summary>
        /// 取得 Company 列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>通用訊息回傳格式。若查詢成功時，另會包含 Items。</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetList(
            #if DEBUG
            [FromQuery] // 避免 Swagger UI 誤解這是 body request
            #endif
            CompanyGetListRequest input)
        {
            // 1. 查詢，並依照分頁取得資料
            List<Company> queried = (await _companyService.SelectMany(input.Keyword
                , input.CompanyTypeId
                , input.GetStartIndex()
                , input.PageRowCount)).ToList();

            if (HasError)
                return Ok(GetReturnMessage());
            
            // 2. 塞值到 CompanyGetListResponse 中
            CompanyGetListResponse response = new CompanyGetListResponse
            {
                NowPage = input.NowPage ?? 0,
                PageRowCount = input.PageRowCount,
                AllItemCt = queried.Count,
                Items = queried.Select(ToCompanyGetListResponseItem)
            };

            // 3. 回傳
            return Ok(GetReturnMessage(response));
        }

        private CompanyGetListResponseItem ToCompanyGetListResponseItem(Company company)
        {
            return new CompanyGetListResponseItem
            {
                DCID = company.DCID,
                BCID = company.BCID,
                BC_TitleC = _companyService.GetCategoryTitleC(company),
                BC_TitleE = _companyService.GetCategoryTitleE(company),
                Code = company.Code,
                TitleC = company.TitleC,
                TitleE = company.TitleE,
                DepartmentCt = _companyService.GetDepartmentCount(company),
                ActiveFlag = company.ActiveFlag,
                CreDate = company.CreDate.ToFormattedString(),
                CreUser = _userService.GetUsername(company.CreUID),
                CreUID = company.CreUID,
                UpdDate = company.UpdUID.IsValidId() ? company.UpdDate.ToFormattedString() : String.Empty,
                UpdUser = company.UpdUID.IsValidId() ? _userService.GetUsername(company.UpdUID) : String.Empty,
                UpdUID = company.UpdUID
            };
        }

        #endregion

        #region GetInfoById
        /// <summary>
        /// 查詢單筆公司資料。
        /// </summary>
        /// <returns>
        /// 成功時：公司資料<br/>
        /// 無資料或輸入有誤時：通用訊息回傳格式。
        /// </returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetInfoById(CompanyGetRequest input)
        {
            if (input.Id.IsValidId())
            {
                AddError(CompanyControllerMessages.GetIdIsInvalid);
                return Ok(GetReturnMessage());
            }

            Company queried = _companyService.SelectById(input.Id);

            if (queried == null)
            {
                AddError(CompanyControllerMessages.GetNotFound);
                return Ok(GetReturnMessage());
            }

            return Ok();
        }
        #endregion

        #region Submit

        #region Submit - 新增

        #endregion

        #region Submit - 更新

        #endregion

        #endregion

        #region ChangeActive

        #endregion

        #region DeleteItem

        #endregion
    }
}