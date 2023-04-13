using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    public static class ChainValidateHelperExtensionMethods
    {
        /// <summary>
        /// 將此物件包裝成一個 BeingValidated 物件，進行驗證。
        /// </summary>
        /// <param name="target">物件</param>
        /// <param name="skipIfAlreadyInvalid">若為 true，則當有任一驗證未通過時，後續驗證就不會再實際執行。（可選）<br/>
        /// 預設為 false。</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>此物件的 BeingValidated。</returns>
        public static IBeingValidated<T> StartValidate<T>(this T target, bool skipIfAlreadyInvalid = false)
        {
            return new BeingValidated<T>(target, skipIfAlreadyInvalid);
        }

        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<T>> Validate<T>(this Task<IBeingValidated<T>> beingValidated
            , Func<T, bool> validation
            , Action onFail = null
            , Action<Exception> onException = null)
        {
            IBeingValidated<T> obj = await beingValidated;
            return obj.Validate(validation, onFail, onException);
        }

        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並處理的 void 方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<T>> Validate<T>(this Task<IBeingValidated<T>> beingValidated
            , Action<T> validation
            , Action<Exception> onException = null)
        {
            IBeingValidated<T> obj = await beingValidated;
            return obj.Validate(validation, onException);
        }

        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<T>> ValidateAsync<T>(this Task<IBeingValidated<T>> beingValidated
            , Func<T, Task<bool>> validation
            , Action onFail = null
            , Action<Exception> onException = null)
        {
            IBeingValidated<T> obj = await beingValidated;
            return await obj.ValidateAsync(validation, onFail, onException);
        }

        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並處理的 void 方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<T>> ValidateAsync<T>(this Task<IBeingValidated<T>> beingValidated
            , Func<T, Task> validation
            , Action<Exception> onException = null)
        {
            IBeingValidated<T> obj = await beingValidated;
            return await obj.ValidateAsync(validation, onException);
        }

        /// <summary>
        /// 取得驗證結果。
        /// </summary>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證失敗。
        /// </returns>
        public static async Task<bool> IsValid<T>(this Task<IBeingValidated<T>> beingValidated)
        {
            IBeingValidated<T> obj = await beingValidated;
            return obj.IsValid();
        }
    }
}