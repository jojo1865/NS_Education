using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report8;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 滿意度調查表報表的處理。
    /// </summary>
    public class Report8Controller : PublicClass, IPrintReport<Report8_Input_APIItem, Report8_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report8_Output_Row_APIItem>> GetResultAsync(
            Report8_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime().Date.AddDays(1) ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.B_SiteData
                        .Include(bs => bs.Resver_Site)
                        .Include(bs => bs.Resver_Site.Select(rs => rs.Resver_Head))
                        .Include(
                            bs => bs.Resver_Site.Select(rs => rs.Resver_Head).Select(rh => rh.Resver_Questionnaire))
                        .Where(bs => bs.ActiveFlag)
                        .Where(bs => !bs.DeleteFlag)
                    ;

                var results = (await query
                        .ToArrayAsync())
                    .Where(bs => bs.Resver_Site.Any())
                    .ToArray();

                // 取得資料，排除掉已刪除/未啟用的預約單，並且確實有 RQ 的
                // 然後依據 RS 數量排序

                foreach (B_SiteData bs in results)
                {
                    bs.Resver_Site = bs.Resver_Site
                        .Where(rs => !rs.DeleteFlag)
                        .Where(rs => !rs.Resver_Head.DeleteFlag)
                        .Where(rs => startTime <= rs.TargetDate)
                        .Where(rs => rs.TargetDate <= endTime)
                        .Where(rs => rs.Resver_Head.Resver_Questionnaire.Any())
                        .ToList();
                }

                Report8_Output_APIItem response = new Report8_Output_APIItem();
                response.SetByInput(input);

                response.Items = results.Select(bs =>
                    {
                        ILookup<string, Resver_Questionnaire> questionnaires = bs.Resver_Site
                            .Select(rs => rs.Resver_Head)
                            .SelectMany(rh => rh.Resver_Questionnaire)
                            .ToLookup(rq => rq.QuestionKey, rq => rq);

                        IEnumerable<Resver_Questionnaire> willUseAgainRows = questionnaires
                            .GetValueOrDefault("WillUseAgain") ?? Array.Empty<Resver_Questionnaire>();

                        int willUseAgainCount = willUseAgainRows
                            .Count(rq => rq.TextContent == "Y");

                        int totalRentCount = bs.Resver_Site
                            .Select(rs => rs.Resver_Head)
                            .DistinctBy(rh => rh.RHID)
                            .Count();

                        decimal percentage =
                            totalRentCount <= 0 ? 0 : Decimal.Divide(willUseAgainCount, totalRentCount);

                        return new Report8_Output_Row_APIItem
                        {
                            SiteName = bs.Title ?? "",
                            SiteCode = bs.Code ?? "",
                            RentCt = totalRentCount,
                            SiteSatisfied = GetQuestionnaireDictionary(questionnaires, "SiteSatisfied"),
                            DeviceSatisfied = GetQuestionnaireDictionary(questionnaires, "DeviceSatisfied"),
                            CleanSatisfied = GetQuestionnaireDictionary(questionnaires, "CleanSatisfied"),
                            NegotiatorSatisfied = GetQuestionnaireDictionary(questionnaires, "NegotiatorSatisfied"),
                            ServiceSatisfied = GetQuestionnaireDictionary(questionnaires, "ServiceSatisfied"),
                            MealSatisfied = GetQuestionnaireDictionary(questionnaires, "MealSatisfied"),
                            DessertSatisfied = GetQuestionnaireDictionary(questionnaires, "DessertSatisfied"),
                            WillUseAgainPercentage = percentage.ToString("P2")
                        };
                    })
                    .OrderByDescending(i => i.RentCt)
                    .ThenBy(i => i.SiteName)
                    .ThenBy(i => i.SiteCode)
                    .ToList();

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();
                return response;
            }
        }

        private static Dictionary<int, int> GetQuestionnaireDictionary(
            ILookup<string, Resver_Questionnaire> questionnaires, string key)
        {
            IEnumerable<Resver_Questionnaire> values =
                questionnaires.GetValueOrDefault(key) ?? Array.Empty<Resver_Questionnaire>();

            Dictionary<int, int> result = new Dictionary<int, int>();

            // 固定 populate 1~5
            for (int i = 1; i <= 5; i++)
            {
                if (!result.ContainsKey(i))
                    result[i] = 0;
            }

            // 寫入實際的值
            foreach (IGrouping<int, Resver_Questionnaire> grouping in values.GroupBy(rq => rq.NumberContent ?? 0))
            {
                result[grouping.Key] = grouping.Count();
            }

            return result;
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report8_Input_APIItem input)
        {
            return GetResponseJson(await GetResultAsync(input));
        }
    }
}