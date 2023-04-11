using System;
using System.Linq;
using System.Threading.Tasks;
using NsEduCore_DAL.Models;
using NsEduCore_DAL.Models.Data;

namespace NsEduCore_DAL.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly NsDataContext _context;

        public CategoryService(NsDataContext context)
        {
            _context = context;
        }
        
        public async Task Create(Domains.Category category)
        {
            B_Category entity = new B_Category
            {
                CategoryType = category.CategoryType,
                ParentID = category.ParentID,
                Code = category.Code,
                TitleC = category.TitleC,
                TitleE = category.TitleE,
                SortNo = category.SortNo,
                ActiveFlag = category.ActiveFlag,
                DeleteFlag = false,
                CreDate = DateTime.Now,
                CreUID = category.CreUID,
                UpdDate = DateTime.Now,
                UpdUID = 0
            };

            _context.B_Category.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Domains.Category category)
        {
            B_Category queried = _context.B_Category.FirstOrDefault(c => c.BCID == category.BCID);

            if (queried == null)
                throw new NullReferenceException("查無資料");
            
            queried.CategoryType = category.CategoryType;
            queried.ParentID = category.ParentID;
            queried.Code = category.Code;
            queried.TitleC = category.TitleC;
            queried.TitleE = category.TitleE;
            queried.SortNo = category.SortNo;
            queried.ActiveFlag = category.ActiveFlag;
            queried.DeleteFlag = category.DeleteFlag;
            queried.UpdDate = DateTime.Now;
            queried.UpdUID = category.UpdUID;
            
            await _context.SaveChangesAsync();
        }
    }
}