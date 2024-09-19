using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.MonthlyTimeSpans.GetInfoById;
using NS_Education.Models.APIItems.Controller.MonthlyTimeSpans.GetList;
using NS_Education.Models.APIItems.Controller.MonthlyTimeSpans.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper
{
    /// <summary>
    /// 讀/寫每月可用時段數用的 Controller。
    /// </summary>
    public class MonthlyTimeSpansController : PublicClass
    {
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList()
        {
            // 以 2000 年起至今 + 3年的所有年份為基準，查詢 DB

            const int startYear = 2000;
            int endYear = DateTime.Now.Year + 3;

            ILookup<int, Monthly_TimeSpans> data = (await DC.Monthly_TimeSpans
                    .Where(mts => startYear <= mts.Year)
                    .Where(mts => mts.Year <= endYear)
                    .Where(mts => 1 <= mts.Month)
                    .Where(mts => mts.Month <= 12)
                    .ToArrayAsync())
                .ToLookup(mts => mts.Year, mts => mts);

            IEnumerable<MonthlyTimeSpans_GetList_Output_Row_APIItem> transformed = Enumerable
                .Range(startYear, endYear - startYear + 1)
                .Reverse()
                .Select((y, idx) =>
                {
                    MonthlyTimeSpans_GetList_Output_Row_APIItem item = new MonthlyTimeSpans_GetList_Output_Row_APIItem
                    {
                        Year = y,
                        SetMonths = data.GetValueOrEmpty(y)
                            .GroupBy(mts => mts.Month)
                            .Count()
                    };

                    item.SetIndex(idx + 1);

                    return item;
                });

            CommonResponseForList<MonthlyTimeSpans_GetList_Output_Row_APIItem> result =
                new CommonResponseForList<MonthlyTimeSpans_GetList_Output_Row_APIItem>
                {
                    Items = transformed.ToList()
                };

            return GetResponseJson(result);
        }

        #endregion

        #region GetInfoByYear

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoByYear(int year)
        {
            // 依據西元年查詢所有資料，並且 populate 成 12 個月

            Dictionary<int, Monthly_TimeSpans> monthToData = (await DC.Monthly_TimeSpans
                    .Where(mts => mts.Year == year)
                    .Where(mts => 1 <= mts.Month)
                    .Where(mts => mts.Month <= 12)
                    .ToArrayAsync())
                .GroupBy(mts => mts.Month)
                .ToDictionary(g => g.Key, g => g.First());

            MonthlyTimeSpans_GetInfoById_Output_APIItem result = new MonthlyTimeSpans_GetInfoById_Output_APIItem
            {
                Year = year,
                MonthlyCt = Enumerable.Range(1, 12)
                    .Select(i => monthToData.GetValueOrDefault(i)?.TimeSpanCt ?? 0)
                    .ToArray()
            };

            return GetResponseJson(result);
        }

        #endregion

        #region Submit

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> Submit(MonthlyTimeSpans_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.MonthlyCt != null,
                    () => AddError(EmptyNotAllowed("每月時段數", nameof(input.MonthlyCt))))
                .Validate(i => i.MonthlyCt.Length == 12,
                    () => AddError(ExpectedValue("每月時段數的長度", nameof(input.MonthlyCt), 12)))
                .Validate(i => i.MonthlyCt.All(m => m.IsZeroOrAbove()),
                    () => AddError(OutOfRange("每月時段數", nameof(input.MonthlyCt), 0)))
                .IsValid();

            if (!isValid)
                return GetResponseJson();

            // 到 DB 查資料, 最多 12 筆

            Dictionary<int, Monthly_TimeSpans> monthToData = (await DC.Monthly_TimeSpans
                    .Where(mts => mts.Year == input.Year)
                    .OrderBy(mts => mts.CreDate)
                    .ToArrayAsync())
                .GroupBy(mts => mts.Month)
                .ToDictionary(g => g.Key, g => g.First());

            for (int i = 1; i <= 12; i++)
            {
                // 如果 monthToData 有資料舊更新既有資料
                // 否則做 insert

                if (!monthToData.TryGetValue(i, out Monthly_TimeSpans data))
                {
                    data = new Monthly_TimeSpans
                    {
                        Year = input.Year,
                        Month = i
                    };

                    DC.Monthly_TimeSpans.Add(data);
                }

                data.TimeSpanCt = input.MonthlyCt[i - 1];
            }

            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            return GetResponseJson(new SubmitHelperIdResponse
            {
                ID = input.Year
            });
        }

        #endregion
    }
}