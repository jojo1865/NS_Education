using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    /// <summary>
    /// 針對 BeingValidated 使用非同步方法時的小幫手，包含一系列擴充方法
    /// </summary>
    public static class BeingValidatedTaskHelperMethods
    {
        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<TInput, TOutput>> Validate<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Func<TInput, bool> validation
            , Action<TInput> onFail = null
            , Action<TInput, Exception> onException = null)
        {
            IBeingValidated<TInput, TOutput> obj = await beingValidated;
            return obj.Validate(validation, onFail, onException);
        }

        /// <inheritdoc cref="Validate{TInput,TOutput}(System.Threading.Tasks.Task{NS_Education.Tools.BeingValidated.IBeingValidated{TInput,TOutput}},System.Func{TInput,bool},System.Action{TInput},System.Action{TInput,System.Exception})"/>
        public static async Task<IBeingValidated<TInput, TOutput>> Validate<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Func<TInput, bool> validation
            , Action onFail = null
            , Action<Exception> onException = null)
            => await beingValidated.Validate(validation, _ => onFail?.Invoke(), (_, e) => onException?.Invoke(e));

        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
        /// <param name="validation">一個接收被包裝物件的類型並處理的 void 方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<TInput, TOutput>> Validate<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Action<TInput> validation
            , Action<TInput, Exception> onException = null)
        {
            IBeingValidated<TInput, TOutput> obj = await beingValidated;
            return obj.Validate(validation, onException);
        }

        /// <inheritdoc cref="Validate{TInput,TOutput}(System.Threading.Tasks.Task{NS_Education.Tools.BeingValidated.IBeingValidated{TInput,TOutput}},System.Action{TInput},System.Action{TInput,System.Exception})"/>
        public static async Task<IBeingValidated<TInput, TOutput>> Validate<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Action<TInput> validation
            , Action<Exception> onException)
            => await beingValidated.Validate(validation, (_, e) => onException?.Invoke(e));

        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<TInput, TOutput>> ValidateAsync<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Func<TInput, Task<bool>> validation
            , Action<TInput> onFail = null
            , Action<TInput, Exception> onException = null)
        {
            IBeingValidated<TInput, TOutput> obj = await beingValidated;
            return await obj.ValidateAsync(validation, onFail, onException);
        }

        /// <inheritdoc cref="ValidateAsync{TInput,TOutput}(System.Threading.Tasks.Task{NS_Education.Tools.BeingValidated.IBeingValidated{TInput,TOutput}},System.Func{TInput,System.Threading.Tasks.Task{bool}},System.Action{TInput},System.Action{TInput,System.Exception})"/>
        public static async Task<IBeingValidated<TInput, TOutput>> ValidateAsync<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Func<TInput, Task<bool>> validation
            , Action onFail = null
            , Action<Exception> onException = null)
            => await beingValidated.ValidateAsync(validation, _ => onFail?.Invoke(), (_, e) => onException?.Invoke(e));

        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
        /// <param name="validation">一個接收被包裝物件的類型並處理的 void 方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<TInput, TOutput>> ValidateAsync<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Func<TInput, Task> validation
            , Action<TInput, Exception> onException = null)
        {
            IBeingValidated<TInput, TOutput> obj = await beingValidated;
            return await obj.ValidateAsync(validation, onException);
        }

        /// <inheritdoc cref="ValidateAsync{TInput,TOutput}(System.Threading.Tasks.Task{NS_Education.Tools.BeingValidated.IBeingValidated{TInput,TOutput}},System.Func{TInput,System.Threading.Tasks.Task},System.Action{TInput,System.Exception})"/>
        public static async Task<IBeingValidated<TInput, TOutput>> ValidateAsync<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated
            , Func<TInput, Task> validation
            , Action<Exception> onException)
            => await beingValidated.ValidateAsync(validation, (_, e) => onException?.Invoke(e));

        /// <summary>
        /// 取得驗證結果。
        /// </summary>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證失敗。
        /// </returns>
        public static async Task<bool> IsValid<T>(this Task<IBeingValidated<T, T>> beingValidated)
        {
            IBeingValidated<T, T> obj = await beingValidated;
            return obj.IsValid();
        }

        /// <summary>
        /// 在執行驗證的中途，修改是否在已發生錯誤時跳過後續驗證的設定。
        /// </summary>
        /// <param name="beingValidated">IBeingValidated 物件</param>
        /// <param name="setTo">欲設定的新值。（可選）忽略時，預設值為 true。</param>
        /// <returns>此物件本身。</returns>
        public static async Task<IBeingValidated<TInput, TOutput>> SkipIfAlreadyInvalid<TInput, TOutput>(
            this Task<IBeingValidated<TInput, TOutput>> beingValidated, bool setTo = true)
        {
            IBeingValidated<TInput, TOutput> obj = await beingValidated;
            return obj.SkipIfAlreadyInvalid(setTo);
        }
    }
}