using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Resver.GetResverSiteList;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    /// <summary>
    /// 處理預約時搜尋場地的 API。<br/>
    /// 因為目前開的 Route 為 Resver，因此還是歸類在 MenuDataController。
    /// </summary>
    public class ResverSiteController : PublicClass,
        IGetListPaged<B_SiteData, Resver_GetResverSiteList_Input_APIItem, Resver_GetResverSiteList_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListPagedHelper<Resver_GetResverSiteList_Input_APIItem> _getListPagedHelper;

        public ResverSiteController()
        {
            _getListPagedHelper = new GetListPagedHelper<ResverSiteController, B_SiteData, Resver_GetResverSiteList_Input_APIItem,
                Resver_GetResverSiteList_Output_Row_APIItem>(this);
        }

        #endregion
        
        #region GetList

        // Input.FreeDate 的暫存處
        private DateTime freeDate;
        
        // 實際 Route 請參考 RouteConfig
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Resver_GetResverSiteList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(Resver_GetResverSiteList_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.BSCID1.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之樓別 ID")))
                .Validate(i => i.PeopleCt.IsZeroOrAbove(), () => AddError(WrongFormat("欲篩選之最小可容納人數")))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<B_SiteData> GetListPagedOrderedQuery(Resver_GetResverSiteList_Input_APIItem input)
        {
            var query = DC.B_SiteData
                .Include(sd => sd.B_OrderCode)
                .AsQueryable();

            if (input.BSCID1.IsAboveZero())
                query = query.Where(sd => sd.BSCID1 == input.BSCID1);

            if (input.PeopleCt.IsAboveZero())
                query = query.Where(sd => sd.MaxSize >= input.PeopleCt);
            
            if (input.FreeDate.TryParseDateTime(out freeDate))
                query = GetListFilterQueryByFreeDate(query);

            return query.OrderBy(sd => sd.BSID);
        }

        private IQueryable<B_SiteData> GetListFilterQueryByFreeDate(IQueryable<B_SiteData> query)
        {
            // 如果沒有任何 freeDate, 直接折回
            if (freeDate == default)
                return query;
            
            // 只允許在 freeDate 當天，有任何時段沒被占用的資料

            return query.ToList()
                .Where(
                    HasUnoccupiedDts
                )
                .AsQueryable();
        }

        private bool HasUnoccupiedDts(B_SiteData sd)
        {
            // 取得所有 DTSID
            HashSet<int> dtsIds = DC.D_TimeSpan
                .Where(dts => !dts.DeleteFlag && dts.ActiveFlag)
                .Select(dts => dts.DTSID)
                .ToHashSet();

            return dtsIds
                // 排除所有已佔用 DTSID
                .Except(
                    // 所有已佔用的 DTSID 的 enumerable
                    GetOccupiedDts(sd)
                )
                // 只保留仍有空閒 DTSID 的資料
                .Any();
        }

        private IEnumerable<int> GetOccupiedDts(B_SiteData sd)
        {
            if (freeDate == default)
                return Array.Empty<int>();
            
            return sd.Resver_Site
                .Where(rs => !rs.DeleteFlag && rs.TargetDate == freeDate)
                .SelectMany(rs => DC.M_Resver_TimeSpan
                    .Where(rts =>
                        rts.TargetTable == DC.GetTableName<Resver_Site>() && rts.TargetID == rs.RSID)
                    .Select(rts => rts.DTSID)
                    .AsEnumerable()
                )
                .Distinct();
        }

        public async Task<Resver_GetResverSiteList_Output_Row_APIItem> GetListPagedEntityToRow(B_SiteData entity)
        {
            HashSet<int> occupiedDts = GetOccupiedDts(entity).ToHashSet();
            
            return await Task.FromResult(new Resver_GetResverSiteList_Output_Row_APIItem
            {
                BSID = entity.BSID,
                Code = entity.Code ?? "",
                Title = entity.Title ?? "",
                BOCID = entity.BOCID,
                BOC_Code = entity.B_OrderCode?.Code ?? "",
                BOC_Title = entity.B_OrderCode?.Title ?? "",
                BOC_PrintTitle = entity.B_OrderCode?.PrintTitle ?? "",
                BOC_PrintNote = entity.B_OrderCode?.PrintNote ?? "",
                Items = freeDate == default 
                    ? new List<Resver_GetResverSiteList_TimeSpan_Output_APIItem>()
                    : DC.D_TimeSpan
                        .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                        .OrderBy(dts => dts.HourS)
                        .ThenBy(dts => dts.MinuteS)
                        .ThenBy(dts => dts.HourE)
                        .ThenBy(dts => dts.MinuteE)
                        .AsEnumerable()
                        .Select(dts => new Resver_GetResverSiteList_TimeSpan_Output_APIItem
                        {
                            DTSID = dts.DTSID,
                            Title = dts.Title ?? "",
                            AllowResverFlag = occupiedDts.Contains(dts.DTSID)
                        }).ToList()
            });
        }

        #endregion
    }
}