using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NsEduCore.Controllers.Messages;
using NsEduCore.Requests.Category;
using NsEduCore_DAL.Domains;
using NsEduCore_DAL.Services.Category;
using NsEduCore_Tools.BeingValidated;
using NsEduCore_Tools.Extensions;

namespace NsEduCore.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// 建立 CategoryController。
        /// </summary>
        /// <param name="categoryService">ICategoryService</param>
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        
        #region Submit

        /// <summary>
        /// 新增或更新一筆 Category。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>通用回傳訊息格式</returns>
        [HttpPost]
        public async Task<IActionResult> Submit(CategorySubmitRequest input)
        {
            // 1. 先對輸入驗證
            // |- a. 驗證建立者 ID
            // |- b. 驗證更新者 ID
            // |- c. 驗證父層選單 ID
            // +- d. 驗證分類所屬類別
            bool isValid = input.StartValidate()
                .Validate(i => i.CreUID.IsValidId(), () => AddError(CategoryControllerMessages.SubmitInvalidCreUid))
                .Validate(i => i.UpdUID.IsValidId(), () => AddError(CategoryControllerMessages.SubmitInvalidUpdUid))
                .Validate(i => i.ParentID >= 0, () => AddError(CategoryControllerMessages.SubmitInvalidParentId))
                .Validate(i => i.CategoryType.IsValidId(),
                    () => AddError(CategoryControllerMessages.SubmitInvalidCategoryType))
                .IsValid();

            if (!isValid)
                return Ok(GetReturnMessage());

            // 2. 依據 BCID 分歧。
            if (input.BCID.IsValidId())
                await SubmitUpdate(input);
            else
                await SubmitCreate(input);

            return Ok(GetReturnMessage());
        }
        
        #region Submit - 新增
        private async Task SubmitCreate(CategorySubmitRequest input)
        {
            // TODO: 之後改成 JWT 驗證時，要記得改 UID 取法
            try
            {
                await _categoryService.Create(ConvertToDomainForCreate(input));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AddError(CategoryControllerMessages.SubmitCreateFailed);
            }
            
        }

        private static Category ConvertToDomainForCreate(CategorySubmitRequest input)
        {
            return new Category
            {
                CreUID = input.CreUID,
                ParentID = input.ParentID,
                CategoryType = input.CategoryType,
                Code = input.Code,
                TitleC = input.TitleC,
                TitleE = input.TitleE,
                SortNo = input.SortNo,
                ActiveFlag = input.ActiveFlag,
                DeleteFlag = false
            };
        }

        #endregion
        
        #region Submit - 更新
        private async Task SubmitUpdate(CategorySubmitRequest input)
        {
            // TODO: 之後改成 JWT 驗證時，要記得改 UID 取法
            try
            {
                await _categoryService.Update(ConvertToDomainForUpdate(input));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AddError(CategoryControllerMessages.SubmitUpdateFailed);
            }
        }

        private static Category ConvertToDomainForUpdate(CategorySubmitRequest input)
        {
            return new Category
            {
                BCID = input.BCID,
                UpdUID = input.UpdUID,
                ParentID = input.ParentID,
                CategoryType = input.CategoryType,
                Code = input.Code,
                TitleC = input.TitleC,
                TitleE = input.TitleE,
                SortNo = input.SortNo,
                ActiveFlag = input.ActiveFlag,
                DeleteFlag = input.DeleteFlag
            };
        }

        #endregion
        
        #endregion
    }
}