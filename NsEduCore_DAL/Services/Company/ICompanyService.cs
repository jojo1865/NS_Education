using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsEduCore_DAL.Services.Company
{
    public interface ICompanyService
    {
        /// <summary>
        /// 查詢多筆非刪除模式的資料，並依照該公司的中文名稱排序後回傳。
        /// </summary>
        /// <param name="keyword">關鍵字。</param>
        /// <param name="companyTypeId">公司類型 ID。</param>
        /// <param name="startIndex">從第幾筆起開始回傳。</param>
        /// <param name="takeCount">總共回傳幾筆。</param>
        Task<IEnumerable<Domains.Company>> SelectMany(string keyword, int? companyTypeId, int startIndex, int takeCount);

        /// <summary>
        /// 取得這筆資料的分類中文名稱。
        /// </summary>
        /// <param name="company">資料</param>
        /// <returns>分類中文名稱</returns>
        string GetCategoryTitleC(Domains.Company company);

        /// <summary>
        /// 取得這筆資料的分類英文名稱。
        /// </summary>
        /// <param name="company">資料</param>
        /// <returns>分類英文名稱</returns>
        string GetCategoryTitleE(Domains.Company company);

        /// <summary>
        /// 取得這筆資料的所屬非刪除模式的部門數量。
        /// </summary>
        /// <param name="company">資料</param>
        /// <returns>所屬部門數量</returns>
        int GetDepartmentCount(Domains.Company company);
    }
}