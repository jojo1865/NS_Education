using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NsEduCore.Responses;
using NsEduCore.Responses.cReturnMessage;

namespace NsEduCore.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public abstract class BaseController : Controller
    {
        private readonly List<string> errors = new();

        /// <summary>
        /// 新增一筆錯誤訊息。
        /// </summary>
        /// <param name="s">錯誤訊息</param>
        protected void AddError(string s)
        {
            errors.Add(s);
        }

        /// <summary>
        /// 取得此 Controller 當下狀態的本專案的標準回傳訊息類型。
        /// </summary>
        /// <returns>cReturnMessage</returns>
        protected cReturnMessage GetReturnMessage()
        {
            return new cReturnMessage
            {
                Success = HasError(),
                Message = CreateErrorMessage()
            };
        }
        
        /// <summary>
        /// 依據此 Controller 當下狀態，針對傳入的 cReturnMessageInfusableAbstract 設定標準回傳訊息欄位。
        /// </summary>
        /// <param name="infusable">繼承 cReturnMessageInfusableAbstract 的輸出物件</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>設定好標準回傳訊息欄位的原物件</returns>
        protected T GetReturnMessage<T>(T infusable) where T : cReturnMessageInfusableAbstract
        {
            infusable.Success = HasError();
            infusable.Message = CreateErrorMessage();

            return infusable;
        }

        private string CreateErrorMessage()
        {
            return String.Join(";", errors);
        }

        private bool HasError()
        {
            return errors.Count == 0;
        }
    }
}