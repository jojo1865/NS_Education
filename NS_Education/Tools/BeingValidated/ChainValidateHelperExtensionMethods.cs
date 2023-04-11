﻿namespace NS_Education.Tools.BeingValidated
{
    public static class ChainValidateHelperExtensionMethods
    {
        /// <summary>
        /// 將此物件包裝成一個 BeingValidated 物件，進行驗證。
        /// </summary>
        /// <param name="target">物件</param>
        /// <param name="skipIfAlreadyInvalid">若為 true，則當有任一驗證未通過時，後續驗證就不會再實際執行。</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>此物件的 BeingValidated。</returns>
        public static IBeingValidated<T> StartValidate<T>(this T target, bool skipIfAlreadyInvalid = false)
        {
            return new BeingValidated<T>(target, skipIfAlreadyInvalid);
        }
    }
}