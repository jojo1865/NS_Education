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
        /// 取得 Department 的下拉選單。
        /// </summary>
        /// <param name="dbSet">Department 的 DbSet</param>
        /// <param name="ddIdToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<ICollection<BaseResponseRowForSelectable>> GetDepartmentSelectable(this DbSet<D_Department> dbSet,
            int ddIdToSelect)
        {
            return await dbSet
                .Where(dd => dd.ActiveFlag && !dd.DeleteFlag)
                .Select(dd => new BaseResponseRowForSelectable
                {
                    ID = dd.DDID,
                    Title = dd.TitleC ?? "",
                    SelectFlag = dd.DDID == ddIdToSelect
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
        
        /// <summary>
        /// 取得 BusinessUser 的下拉選單。
        /// </summary>
        /// <param name="dbSet">BusinessUser 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetBusinessUserSelectable(this DbSet<BusinessUser> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(bu => bu.ActiveFlag && !bu.DeleteFlag)
                .Select(bu => new BaseResponseRowForSelectable
                {
                    ID = bu.BUID,
                    Title = bu.Name ?? bu.Code ?? "",
                    SelectFlag = bu.BUID == idToSelect
                })
                .ToListAsync();
        }
        
        /// <summary>
        /// 取得 FoodCategory 的下拉選單。
        /// </summary>
        /// <param name="dbSet">FoodCategory 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetFoodCategorySelectable(this DbSet<D_FoodCategory> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(fc => fc.ActiveFlag && !fc.DeleteFlag)
                .Select(fc => new BaseResponseRowForSelectable
                {
                    ID = fc.DFCID,
                    Title = fc.Title ?? fc.Code ?? "",
                    SelectFlag = fc.DFCID == idToSelect
                })
                .ToListAsync();
        }

        /// <summary>
        /// 取得 Partner 的下拉選單。
        /// </summary>
        /// <param name="dbSet">FoodCategory 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetPartnerSelectable(this DbSet<B_Partner> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(p => p.ActiveFlag && !p.DeleteFlag)
                .Select(p => new BaseResponseRowForSelectable
                {
                    ID = p.BPID,
                    Title = p.Title ?? p.Code ?? "",
                    SelectFlag = p.BPID == idToSelect
                })
                .ToListAsync();
        }
        
        /// <summary>
        /// 取得 Device 的下拉選單。
        /// </summary>
        /// <param name="dbSet">Device 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetOtherPayItemSelectable(this DbSet<B_Device> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(d => d.ActiveFlag && !d.DeleteFlag)
                .Select(d => new BaseResponseRowForSelectable
                {
                    ID = d.BDID,
                    Title = d.Title ?? d.Code ?? "",
                    SelectFlag = d.BDID == idToSelect
                })
                .ToListAsync();
        }
        
        /// <summary>
        /// 取得 OtherPayItem 的下拉選單。
        /// </summary>
        /// <param name="dbSet">OtherPayItem 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetOtherPayItemSelectable(this DbSet<D_OtherPayItem> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(opi => opi.ActiveFlag && !opi.DeleteFlag)
                .Select(opi => new BaseResponseRowForSelectable
                {
                    ID = opi.DOPIID,
                    Title = opi.Title ?? opi.Code ?? "",
                    SelectFlag = opi.DOPIID == idToSelect
                })
                .ToListAsync();
        }
        
        /// <summary>
        /// 取得 PayType 的下拉選單。
        /// </summary>
        /// <param name="dbSet">PayType 的 DbSet</param>
        /// <param name="idToSelect">選擇一筆資料的 ID，此筆資料 SelectFlag 設為 true</param>
        /// <returns><see cref="BaseResponseRowForSelectable"/> 的 List</returns>
        public static async Task<List<BaseResponseRowForSelectable>> GetOtherPayItemSelectable(this DbSet<D_PayType> dbSet
            ,int idToSelect)
        {
            return await dbSet
                .Where(pt => pt.ActiveFlag && !pt.DeleteFlag)
                .Select(pt => new BaseResponseRowForSelectable
                {
                    ID = pt.DPTID,
                    Title = pt.Title ?? "",
                    SelectFlag = pt.DPTID == idToSelect
                })
                .ToListAsync();
        }
    }
}