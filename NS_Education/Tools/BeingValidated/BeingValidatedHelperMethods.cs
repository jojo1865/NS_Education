using System.Collections.Generic;

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

        /// <summary>
        /// 將此集合包裝成一個 BeingValidated 物件，對其當中的原素進行驗證。
        /// </summary>
        /// <param name="target">集合</param>
        /// <param name="skipIfAlreadyInvalid">若為 true，則當有任一驗證未通過時，後續驗證就不會再實際執行。（可選）<br/>
        /// 預設為 false。</param>
        /// <typeparam name="TCollection">整個集合的類型</typeparam>
        /// <typeparam name="TElement">集合中包含元素的類型</typeparam>
        /// <returns>此物件的 BeingValidated。</returns>
        public static IBeingValidated<TElement, IEnumerable<TElement>> StartValidateElements<TElement>(this IEnumerable<TElement> target,
            bool skipIfAlreadyInvalid = false)
        {
            return new BeingValidatedEnumerable<TElement, IEnumerable<TElement>>(target, skipIfAlreadyInvalid);
        }
    }
}