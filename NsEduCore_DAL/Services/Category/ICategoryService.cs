using System.Threading.Tasks;

namespace NsEduCore_DAL.Services.Category
{
    public interface ICategoryService
    {
        /// <summary>
        /// 建立一筆資料。
        /// </summary>
        /// <param name="category">domain model</param>
        Task Create(Domains.Category category);
        
        /// <summary>
        /// 更新一筆資料。
        /// </summary>
        /// <param name="category">domain model</param>
        Task Update(Domains.Category category);
    }
}