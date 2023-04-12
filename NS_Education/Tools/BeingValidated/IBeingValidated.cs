using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    public interface IBeingValidated<T>
    {
        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        IBeingValidated<T> Validate(Func<T, bool> validation, Action onFail = null, Action<Exception> onException = null);

        /// <summary>
        /// 非同步地執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <param name="onFail">（可選）當驗證不通過時，執行的方法。</param>
        /// <param name="onException">（可選）當驗證過程發生 Exception 時，執行的方法。未設定時，不做任何 catch。</param>
        /// <returns>此物件本身。</returns>
        Task<IBeingValidated<T>> ValidateAsync(Func<T, Task<bool>> validation, Action onFail = null, Action<Exception> onException = null);
        
        /// <summary>
        /// 取得驗證結果。
        /// </summary>
        /// <returns>true：驗證通過；false：驗證失敗。</returns>
        bool IsValid();
    }
}