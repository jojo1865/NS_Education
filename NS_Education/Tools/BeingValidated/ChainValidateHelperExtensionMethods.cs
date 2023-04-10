using System;

namespace NS_Education.Tools
{
    public static class ChainValidateHelperExtensionMethods
    {
        /// <summary>
        /// 將此物件包裝成一個 BeingValidated 物件，進行驗證。
        /// </summary>
        /// <param name="target">物件</param>
        /// <param name="lazy">是否啟用 lazy 模式：若為 true，則當有任一驗證未通過時，後續驗證就不會再實際執行。</param>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>此物件的 BeingValidated。</returns>
        public static IBeingValidated<T> StartValidate<T>(T target, bool lazy = false)
        {
            return new BeingValidated<T>(target, lazy);
        }
    }
}