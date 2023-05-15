using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    public interface IBeingValidated<TInput, TOutput>
    {
        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        IBeingValidated<TInput, TOutput> Validate(Func<TInput, bool> validation, Action<TInput> onFail = null, Action<TInput, Exception> onException = null);

        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並處理的 void 方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        IBeingValidated<TInput, TOutput> Validate(Action<TInput> validation, Action<TInput, Exception> onException = null);
        
        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        Task<IBeingValidated<TInput, TOutput>> ValidateAsync(Func<TInput, Task<bool>> validation, Action<TInput> onFail = null, Action<TInput, Exception> onException = null);
        
        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並處理的 void 方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        Task<IBeingValidated<TInput, TOutput>> ValidateAsync(Func<TInput, Task> validation, Action<TInput, Exception> onException = null);
        
        /// <summary>
        /// 取得驗證結果。
        /// </summary>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證失敗。
        /// </returns>
        bool IsValid();

        /// <summary>
        /// 在執行驗證的中途，修改是否在已發生錯誤時跳過後續驗證的設定。
        /// </summary>
        /// <param name="setTo">欲設定的新值。（可選）忽略時，預設值為 true。</param>
        /// <typeparam name="T">Generic Type。</typeparam>
        /// <returns>此物件本身。</returns>
        IBeingValidated<TInput, TOutput> SkipIfAlreadyInvalid(bool setTo = true);
    }
}