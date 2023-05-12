using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    /// <summary>
    /// 針對 BeingValidated 使用非同步方法時的小幫手，包含一系列擴充方法
    /// </summary>
    public static class BeingValidatedAsyncHelperMethods
    {
        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
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
        /// <param name="beingValidated">IBeingValidated 物件</param>
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
        /// <param name="beingValidated">IBeingValidated 物件</param>
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
        /// <param name="beingValidated">IBeingValidated 物件</param>
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

        /// <summary>
        /// 在執行驗證的中途，修改是否在已發生錯誤時跳過後續驗證的設定。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
        /// <param name="setTo">欲設定的新值。（可選）忽略時，預設值為 true。</param>
        /// <typeparam name="T">Generic Type。</typeparam>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<T>> SkipIfAlreadyInvalid<T>(this Task<IBeingValidated<T>> beingValidated, bool setTo = true)
        {
            IBeingValidated<T> obj = await beingValidated;
            return obj.SkipIfAlreadyInvalid(setTo);
        }
    }
}