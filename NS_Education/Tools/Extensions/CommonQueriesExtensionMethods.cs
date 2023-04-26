using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems;
using NS_Education.Models.Entities;

namespace NS_Education.Tools.Extensions
{
    public static class CommonQueriesExtensionMethods
    {
        /// <summary>
        /// 取得 StaticCode 的下拉選單。
        /// </summary>
        /// <param name="dbSet">StaticCode 的 DbSet</param>
        /// <param name="codeType">欲顯示的 CodeType</param>
        /// <param name="bscIdToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<ICollection<BaseResponseRowForSelectable>> GetStaticCodeSelectable(this DbSet<B_StaticCode> dbSet
            , int? codeType, int bscIdToSelect)
        {
            if (codeType == null)
                return new List<BaseResponseRowForSelectable>();
            
            return await dbSet
                .Where(sc => sc.ActiveFlag && !sc.DeleteFlag && sc.CodeType == codeType)
                .Select(sc => new BaseResponseRowForSelectable
                {
                    ID = sc.BSCID,
                    Title = sc.Title ?? "",
                    SelectFlag = sc.BSCID == bscIdToSelect
                })
                .ToListAsync();
        }

        /// <summary>
        /// 取得 Hall 的下拉選單。
        /// </summary>
        /// <param name="dbSet">Hall 的 DbSet</param>
        /// <param name="dhIdToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<ICollection<BaseResponseRowForSelectable>> GetHallSelectable(this DbSet<D_Hall> dbSet,
            int dhIdToSelect)
        {
            return await dbSet
                .Where(dh => dh.ActiveFlag && !dh.DeleteFlag)
                .Select(dh => new BaseResponseRowForSelectable
                {
                    ID = dh.DHID,
                    Title = dh.TitleC ?? "",
                    SelectFlag = dh.DHID == dhIdToSelect
                })
                .ToListAsync();
        }
        
        /// <summary>
        /// 取得 OrderCode 的下拉選單。
        /// </summary>
        /// <param name="dbSet">OrderCode 的 DbSet</param>
        /// <param name="codeType">欲顯示的 CodeType</param>
        /// <param name="bocIdToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<ICollection<BaseResponseRowForSelectable>> GetOrderCodeSelectable(this DbSet<B_OrderCode> dbSet
            , int? codeType, int bocIdToSelect)
        {
            if (codeType == null)
                return new List<BaseResponseRowForSelectable>();
            
            return await dbSet
                .Where(oc => oc.ActiveFlag && !oc.DeleteFlag && oc.CodeType == codeType)
                .Select(oc => new BaseResponseRowForSelectable
                {
                    ID = oc.BOCID,
                    Title = oc.Title ?? "",
                    SelectFlag = oc.BOCID == bocIdToSelect
                })
                .ToListAsync();
        }

        /// <summary>
        /// 取得 Category 的下拉選單。
        /// </summary>
        /// <param name="dbSet">Category 的 DbSet</param>
        /// <param name="categoryType">欲顯示的 CategoryType</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<ICollection<BaseResponseRowForSelectable>> GetCategorySelectable(this DbSet<B_Category> dbSet
            , int? categoryType, int idToSelect)
        {
            if (categoryType == null)
                return new List<BaseResponseRowForSelectable>();
            
            return await dbSet
                .Where(c => c.CategoryType == categoryType && c.ActiveFlag && !c.DeleteFlag)
                .Select(c => new BaseResponseRowForSelectable
                {
                    ID = c.BCID,
                    Title = c.TitleC ?? c.TitleE ?? "",
                    SelectFlag = c.BCID == idToSelect
                }).ToListAsync();
        }

        /// <summary>
        /// 取得 Customer 的下拉選單。
        /// </summary>
        /// <param name="dbSet">Customer 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetCustomerSelectable(this DbSet<Customer> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(c => c.ActiveFlag && !c.DeleteFlag)
                .Select(c => new BaseResponseRowForSelectable
                {
                    ID = c.CID,
                    Title = c.TitleC ?? c.TitleE ?? "",
                    SelectFlag = c.CID == idToSelect
                })
                .ToListAsync();
        }
    }
}