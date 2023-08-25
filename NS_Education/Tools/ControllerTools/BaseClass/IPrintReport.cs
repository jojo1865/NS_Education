using System.Threading.Tasks;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.ControllerTools.BaseClass
{
    /// <summary>
    /// 匯出報表工具物件的介面。
    /// </summary>
    /// <typeparam name="TInput">輸入</typeparam>
    /// <typeparam name="TOutputRow">輸出資料列類型</typeparam>
    public interface IPrintReport<in TInput, TOutputRow>
    {
        /// <summary>
        /// 取得報表結果。
        /// </summary>
        /// <param name="input">輸入</param>
        /// <returns>包含 TOutputRow 的 <see cref="CommonResponseForPagedList{T}"/></returns>
        Task<CommonResponseForPagedList<TOutputRow>> GetResultAsync(TInput input);
    }
}