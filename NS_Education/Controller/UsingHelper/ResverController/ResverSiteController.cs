using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.Resver.GetResverSiteList;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    /// <summary>
    /// 處理預約時搜尋場地的 API。
    /// </summary>
    public class ResverSiteController : PublicClass
    {
        #region GetList

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.None)]
        public async Task<string> GetResverSiteList(Resver_GetResverSiteList_Input_APIItem input)
        {
            // 這個功能有特殊需求：當輸入 freeDate 時需要用 Resver_TimeSpan 篩選，但 RTS 並不是直接 FK 關係，
            // 不好在 Helper 製作 Query 的過程中表示。
            // 因此，這裡不使用 Helper。

            // 1. 驗證輸入
            bool isValid = await GetResverSiteListValidateInput(input);

            if (!isValid)
                return GetResponseJson();

            // 2. 套用查詢
            IQueryable<B_SiteData> query = GetResverSiteListMakeQuery(input);

            B_SiteData[] sites = await query.ToArrayAsync();

            var queryResult = await GetResverSiteListFilterByFreeDate(input, sites);

            // 3. 轉換成輸出結果
            int index = input.ReverseOrder ? queryResult.Count - 1 : 0;
            CommonResponseForPagedList<Resver_GetResverSiteList_Output_Row_APIItem> response =
                new CommonResponseForPagedList<Resver_GetResverSiteList_Output_Row_APIItem>
                {
                    Items = queryResult.Keys.Select(bs => new Resver_GetResverSiteList_Output_Row_APIItem
                    {
                        Index = input.ReverseOrder ? index-- : index++,
                        BSID = bs.BSID,
                        Code = bs.Code ?? "",
                        Title = bs.Title ?? "",
                        BOCID = bs.BOCID,
                        BOC_Code = bs.B_OrderCode?.Code ?? "",
                        BOC_Title = bs.B_OrderCode?.Title ?? "",
                        BOC_PrintTitle = bs.B_OrderCode?.PrintTitle ?? "",
                        BOC_PrintNote = bs.B_OrderCode?.PrintNote ?? "",
                        Items = queryResult[bs]
                    }).ToList()
                };

            // 取得總筆數
            response.SetByInput(input);
            response.AllItemCt = response.Items.Count;

            // 切分頁 ... 因為 freeDate 的處理無法在 EF 做，所以無法照一般流程丟兩次（第一次取 Count，第二次取切完的結果）
            // 所以這支到這裡才切

            response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();


            return GetResponseJson(response);
        }

        private async Task<Dictionary<B_SiteData, Resver_GetResverSiteList_TimeSpan_Output_APIItem[]>>
            GetResverSiteListFilterByFreeDate(
                Resver_GetResverSiteList_Input_APIItem input,
                B_SiteData[] sites)
        {
            // 如果沒有 FreeDate，直接傳一個 B_SiteData : empty 的 LookUp
            if (!input.FreeDate.TryParseDateTime(out DateTime freeDate))
                return sites.ToDictionary(sd => sd,
                    sd => Array.Empty<Resver_GetResverSiteList_TimeSpan_Output_APIItem>());

            Dictionary<B_SiteData, Resver_GetResverSiteList_TimeSpan_Output_APIItem[]> result =
                new Dictionary<B_SiteData, Resver_GetResverSiteList_TimeSpan_Output_APIItem[]>();

            // 有 FreeDate
            // 1. 查詢所有可用的 D_TimeSpan
            D_TimeSpan[] allDts = await DC.D_TimeSpan.Where(dts => dts.ActiveFlag && !dts.DeleteFlag).ToArrayAsync();
            string rtsTableName = DC.GetTableName<Resver_Site>();

            foreach (B_SiteData siteData in sites.Distinct())
            {
                // 2. 查詢所有此場地在當天已經占用的時段
                var occupiedDts = siteData.Resver_Site
                    .Where(rs => !rs.DeleteFlag && rs.TargetDate.Date == freeDate.Date)
                    .SelectMany(rs => DC.M_Resver_TimeSpan
                        .Include(rts => rts.D_TimeSpan)
                        .Where(rts => rts.TargetTable == rtsTableName && rts.TargetID == rs.RSID))
                    .Select(rts => rts.D_TimeSpan)
                    .ToHashSet();

                // 3. 產生回傳用物件
                var rows = allDts
                    .Select(dts => new Resver_GetResverSiteList_TimeSpan_Output_APIItem
                    {
                        DTSID = dts.DTSID,
                        Title = dts.Title,
                        AllowResverFlag = !occupiedDts.Any(rts => rts.IsCrossingWith(dts))
                    })
                    .ToArray();

                // 如果所有時段都沒空，不加入回傳結果
                if (!rows.Any(r => r.AllowResverFlag))
                    continue;

                result[siteData] = rows;
            }

            return result;
        }

        private IOrderedQueryable<B_SiteData> GetResverSiteListMakeQuery(Resver_GetResverSiteList_Input_APIItem input)
        {
            var query = DC.B_SiteData
                .Include(sd => sd.B_OrderCode)
                .Include(sd => sd.Resver_Site)
                .AsQueryable();

            if (input.BSCID1.IsAboveZero())
                query = query.Where(sd => sd.BSCID1 == input.BSCID1);

            if (input.PeopleCt.IsAboveZero())
                query = query.Where(sd => sd.MaxSize >= input.PeopleCt);

            if (input.ActiveFlag.IsInBetween(0, 1))
                query = query.Where(sd => sd.ActiveFlag == (input.ActiveFlag == 1));

            query = query.Where(sd => sd.DeleteFlag == (input.DeleteFlag == 1));

            return !input.ReverseOrder ? query.OrderBy(sd => sd.BSID) : query.OrderByDescending(sd => sd.BSID);
        }

        private async Task<bool> GetResverSiteListValidateInput(Resver_GetResverSiteList_Input_APIItem input)
        {
            return await input.StartValidate()
                .Validate(i => i.FreeDate.IsNullOrWhiteSpace() || i.FreeDate.TryParseDateTime(out _),
                    () => AddError(WrongFormat("欲篩選之有空日期", nameof(input.FreeDate))))
                .Validate(i => i.BSCID1.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選之樓別 ID", nameof(input.BSCID1))))
                .ValidateAsync(
                    async i => i.BSCID1 == 0 ||
                               await DC.B_StaticCode.ValidateStaticCodeExists(i.BSCID1, StaticCodeType.Floor),
                    i => AddError(NotFound($"欲篩選之樓別 ID（{i.BSCID1}）", nameof(i.BSCID1))))
                .Validate(i => i.PeopleCt.IsZeroOrAbove(),
                    () => AddError(WrongFormat("欲篩選之可容納人數", nameof(input.PeopleCt))))
                .IsValid();
        }

        #endregion
    }
}