using System.Linq;
using NS_Education.Models.Entities;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    public static class CustomerExtension
    {
        /// <summary>
        /// 取得這筆客戶的有效預約紀錄數。
        /// </summary>
        /// <param name="customer">客戶</param>
        /// <returns>有效預約紀錄數</returns>
        public static int GetDealtReservationCount(this Customer customer)
        {
            return customer.Resver_Head.AsQueryable().Where(ResverHeadExpression.IsDealtExpression).Count();
        }
    }
}