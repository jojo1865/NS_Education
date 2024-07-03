using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Models.Entities
{
    /// <summary>
    /// 針對 Resver_Site 的擴充方法
    /// </summary>
    public static class ResverSiteExtension
    {
        /// <summary>
        /// 傳入一個預約場地，其所有預約時段與計算所需的參數，取得這個預約場地在每個時段的報價
        /// </summary>
        /// <param name="rs">預約場地</param>
        /// <param name="timeSpans">所有時段</param>
        /// <returns>
        /// 依據時段的價格基數加權，並且回傳集合如下：<br/>
        /// 當 currentIndex 小於 maxCount - 1（不是最後一個時段）時：無條件捨去後的金額<br/>
        /// 當 currentIndex 等於 maxCount - 1 （是最後一個時段時）：完整報價減掉其他所有時段的金額
        /// </returns>
        public static IEnumerable<decimal> GetQuotedPriceByTimeSpan(this Resver_Site rs,
            IEnumerable<D_TimeSpan> timeSpans)
        {
            // 因為舊系統的場地價是依時段填
            // 但新系統只填單一個報價 for 預約場地
            // 所以在某些依時段把場地報價區分開來的舊系統既有報表，顯示報價時，就會有落差
            // 所以需要透過這個方法特殊計算
            // 雖然每行細項會有 (0 ~ 時段數) 塊的差異
            // 但最後總價會相同

            timeSpans = timeSpans.ToArray();

            if (!timeSpans.Any())
                return Array.Empty<decimal>();

            // 所有價格基數（計算加權時的分母）
            decimal totalPercentages = timeSpans.Sum(ts => ts.PriceRatePercentage);

            // 分母是 0, 全部回傳 0
            if (totalPercentages is 0)
                return timeSpans.Select(ts => 0m);

            // 先算第 (1 ... n-1) 個時段
            ICollection<decimal> results = timeSpans
                .Take(timeSpans.Count() - 1)
                .Select(dts =>
                {
                    decimal weight = Decimal.Divide(dts.PriceRatePercentage, totalPercentages);
                    decimal rawPrice = rs.QuotedPrice * weight;
                    return Math.Floor(rawPrice);
                })
                .ToList();

            // 最後一個時段的價格就是 (總報價 - 其他所有時段的報價)
            decimal othersSum = results.Sum(r => r);
            decimal last = rs.QuotedPrice - othersSum;

            results.Add(last);

            return results;
        }
    }
}