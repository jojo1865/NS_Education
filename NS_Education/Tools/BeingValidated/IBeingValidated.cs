using System;

namespace NS_Education.Tools
{
    public interface IBeingValidated<T>
    {
        /// <summary>
        /// 執行驗證。
        /// </summary>
        /// <param name="validation">一個接收被包裝物件的類型並回傳 bool 值的驗證方法。</param>
        /// <returns>此物件本身。</returns>
        BeingValidated<T> Validate(Func<T, bool> validation);

        /// <summary>
        /// 取得驗證結果。
        /// </summary>
        /// <returns>true：驗證通過；false：驗證失敗。</returns>
        bool Result();
    }
}