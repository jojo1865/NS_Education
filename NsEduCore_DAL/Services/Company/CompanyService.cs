using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NsEduCore_DAL.Models;
using NsEduCore_DAL.Models.Data;
using NsEduCore_Tools.Extensions;

namespace NsEduCore_DAL.Services.Company
{
    public class CompanyService : ICompanyService
    {
        private readonly NsDataContext _context;

        public CompanyService(NsDataContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Domains.Company>> SelectMany(string keyword, int? companyTypeId, int startIndex, int takeCount)
        {
            if (takeCount <= 0)
                return new List<Domains.Company>();
            
            // 1. 當有輸入分類 ID 時，只查對應分類 ID 的公司。
            // 2. 當有輸入關鍵詞時，查詢公司中文名、英文名、編號。
            // 3. 依據中文名排序。
            // 4. 同時納入會用到的關聯表。
            // 5. 跳到 startIndex, 並只取 takeCount 筆。
            List<D_Company> queried = await _context.D_Company
                .Where(c => companyTypeId.IsNullOrZeroOrLess() || c.BCID == companyTypeId)
                .Where(c => keyword.IsNullOrWhitespace() || c.TitleC.Contains(keyword))
                .OrderBy(c => c.TitleC)
                .Include(c => c.BC)
                .Include(c => c.D_Department)
                .Skip(startIndex)
                .Take(takeCount)
                .ToListAsync();

            // 6. 轉換成 domain。
            return ConvertToDomain(queried);
        }

        public string GetCategoryTitleC(Domains.Company company)
        {
            return company.Data.BC.TitleC;
        }

        public string GetCategoryTitleE(Domains.Company company)
        {
            return company.Data.BC.TitleE;
        }

        public int GetDepartmentCount(Domains.Company company)
        {
            return company.Departments.Count;
        }

        public Domains.Company SelectById(int id)
        {
            throw new System.NotImplementedException();
        }

        private static IEnumerable<Domains.Company> ConvertToDomain(List<D_Company> queried)
        {
            return queried.Select(q => new Domains.Company
            {
                DCID = q.DCID,
                BCID = q.BCID,
                TitleC = q.TitleC,
                TitleE = q.TitleE,
                Code = q.Code,
                ActiveFlag = q.ActiveFlag,
                DeleteFlag = q.DeleteFlag,
                CreDate = q.CreDate,
                CreUID = q.CreUID,
                UpdDate = q.UpdDate,
                UpdUID = q.UpdUID,
                Data = q,
                Departments = q.D_Department
            });
        }
    }
}