using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    /// <summary>
    /// BeingValidate 的小幫手，包含一系列擴充方法。
    /// </summary>
    public static class BeingValidatedHelperMethods
    {
        /// <summary>
        /// 將此物件包裝成一個 BeingValidated 物件，進行驗證。
        /// </summary>
        /// <param name="target">物件</param>
        /// <param name="skipIfAlreadyInvalid">若為 true，則當有任一驗證未通過時，後續驗證就不會再實際執行。（可選）<br/>
        /// 預設為 false。</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>此物件的 BeingValidated。</returns>
        public static IBeingValidated<T, T> StartValidate<T>(this T target, bool skipIfAlreadyInvalid = false)
        {
            return new BeingValidated<T>(target, skipIfAlreadyInvalid);
        }

        /// <inheritdoc cref="IBeingValidated{TInput,TOutput}.Validate(Func{TInput, bool}, Action{TInput}, Action{TInput, Exception})"/>
        public static IBeingValidated<TInput, TOutput> Validate<TInput, TOutput>(
            this IBeingValidated<TInput, TOutput> target, Func<TInput, bool> validation, Action onFail = null,
            Action<Exception> onException = null)
        {
            // 這個擴充方法用於當使用者不需要在 onFail 或 onException 取得輸入時，提供的語法糖，同時也是向後相容。
            return target.Validate(validation, _ => onFail?.Invoke(), (_, e) => onException?.Invoke(e));
        }

        /// <inheritdoc cref="IBeingValidated{TInput,TOutput}.Validate(Action{TInput}, Action{TInput, Exception})"/>
        public static IBeingValidated<TInput, TOutput> Validate<TInput, TOutput>(
            this IBeingValidated<TInput, TOutput> target,
            Action<TInput> validation,
            Action<Exception> onException = null)
        {
            // 這個擴充方法用於當使用者不需要在 onFail 或 onException 取得輸入時，提供的語法糖，同時也是向後相容。
            return target.Validate(validation, (_, e) => onException?.Invoke(e));
        }

        /// <inheritdoc cref="IBeingValidated{TInput,TOutput}.ValidateAsync(Func{TInput, Task{bool}}, Action{TInput}, Action{TInput, Exception})"/>
        public static async Task<IBeingValidated<TInput, TOutput>> ValidateAsync<TInput, TOutput>(
            this IBeingValidated<TInput, TOutput> target,
            Func<TInput, Task<bool>> validation,
            Action onFail = null,
            Action<Exception> onException = null)
        {
            // 這個擴充方法用於當使用者不需要在 onFail 或 onException 取得輸入時，提供的語法糖，同時也是向後相容。
            return await target.ValidateAsync(validation, _ => onFail?.Invoke(), (_, e) => onException?.Invoke(e));
        }

        /// <inheritdoc cref="IBeingValidated{TInput,TOutput}.ValidateAsync(Func{TInput, Task}, Action{TInput, Exception})"/>
        public static async Task<IBeingValidated<TInput, TOutput>> ValidateAsync<TInput, TOutput>(
            this IBeingValidated<TInput, TOutput> target,
            Func<TInput, Task> validation,
            Action<Exception> onException = null)
        {
            // 這個擴充方法用於當使用者不需要在 onFail 或 onException 取得輸入時，提供的語法糖，同時也是向後相容。
            return await target.ValidateAsync(validation, (_, e) => onException?.Invoke(e));
        }

        /// <summary>
        /// 將此集合包裝成一個 BeingValidated 物件，對其當中的元素進行驗證。
        /// </summary>
        /// <param name="target">集合</param>
        /// <param name="skipIfAlreadyInvalid">若為 true，則當有任一驗證未通過時，後續驗證就不會再實際執行。（可選）<br/>
        /// 預設為 false。</param>
        /// <typeparam name="TElement">集合中包含元素的類型</typeparam>
        /// <returns>此物件的 BeingValidated。</returns>
        public static IBeingValidated<TElement, IEnumerable<TElement>> StartValidateElements<TElement>(
            this IEnumerable<TElement> target,
            bool skipIfAlreadyInvalid = false)
        {
            return new BeingValidatedEnumerable<TElement, IEnumerable<TElement>>(target, skipIfAlreadyInvalid);
        }
    }
}