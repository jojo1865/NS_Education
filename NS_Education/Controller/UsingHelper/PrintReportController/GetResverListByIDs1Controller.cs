using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NPOI.SS.UserModel;
using NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds1;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.ExcelBuild;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 處理 /PrintReport/GetResverListByIDs1 的端點。<br/>
    /// 實際 Route 請參考 RouteConfig。
    /// </summary>
    public class GetResverListByIDs1Controller : PublicClass,
        IGetListAll<Resver_Head, PrintReport_GetResverListByIds1_Input_APIItem,
            PrintReport_GetResverListByIds1_Output_Row_APIItem>
    {
        #region Excel

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<FileContentResult> GetExcel(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            ExcelBuilder e = new ExcelBuilder
            {
                ReportTitle = "報價暨預約確認單",
                Columns = 9
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo();
            e.CreateHeader(info);

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "親愛的客戶，您好，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "感謝預約使用本中心之場地，敬請詳閱以下預約場地之細節，若確認無誤，敬請簽名");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "回傳至04-2389-3000");

            e.CreateRow();

            e.CreateRow()
                .DrawBorder(BorderDirection.Top | BorderDirection.Left | BorderDirection.Right, true)
                .SetValue(0, "主辦單位：")
                .SetValue(1, "南山人壽保險股份有限公司")
                .SetValueFromRight(1, "聯絡人：")
                .SetValueFromRight(0, "鄭博仁");

            e.CreateRow()
                .DrawBorder(BorderDirection.Left | BorderDirection.Right, true)
                .SetValue(0, "電話：")
                .SetValue(1, "(04)2334-1800")
                .SetValueFromRight(4, "傳真：")
                .SetValueFromRight(3, "(04)-2389-3000")
                .SetValueFromRight(1, "統一編號：")
                .SetValueFromRight(0, 11456006, CellType.Numeric);

            e.CreateRow()
                .DrawBorder(BorderDirection.Bottom | BorderDirection.Left | BorderDirection.Right, true)
                .SetValue(1, "0910-XXXXXX");

            e.CreateRow()
                .SetValue(0, "活動名稱：")
                .SetValue(1, "職業安全衛生教育訓練");

            e.CreateRow()
                .SetValue(0, "活動日期：")
                .SetValue(1, "2024/2/1~2024/2/2");

            e.CreateRow()
                .SetValue(0, "人數：")
                .SetValue(1, "10人");

            e.CreateRow()
                .SetValue(0, "團體報價：")
                .SetValue(1, "4,200元(含稅，不含旅行平安險)");

            e.CreateRow();

            e.CreateRow()
                .SetValue(0, "一、專案說明");

            e.CreateRow()
                .SetValue(0, "#場地費用");

            var fakeRows = new[]
            {
                new PrintReport_GetResverListByIds1_SiteItem_APIItem
                {
                    Date = "2024/2/1",
                    TimeSpans = "上午~下午",
                    SiteTitle = "316訓練教室",
                    TableTitle = "教室型",
                    FixedPrice = 4800,
                    QuotedPrice = 4320,
                },
                new PrintReport_GetResverListByIds1_SiteItem_APIItem
                {
                    Date = "2024/2/1",
                    TimeSpans = "晚上",
                    SiteTitle = "316訓練教室",
                    TableTitle = "教室型",
                    FixedPrice = 2400,
                    QuotedPrice = 2160,
                },
                new PrintReport_GetResverListByIds1_SiteItem_APIItem
                {
                    Date = "2024/2/2",
                    TimeSpans = "上午~下午",
                    SiteTitle = "316訓練教室",
                    TableTitle = "教室型",
                    FixedPrice = 4800,
                    QuotedPrice = 4320
                }
            };

            e.StartDefineTable<PrintReport_GetResverListByIds1_SiteItem_APIItem>()
                .SetDataRows(fakeRows)
                .StringColumn(0, "日期", d => d.Date)
                .StringColumn(1, "時段", d => d.TimeSpans)
                .StringColumn(2, "場地", d => d.SiteTitle)
                .StringColumn(4, "桌型", d => d.TableTitle)
                .PriceColumn(6, "定價", d => d.FixedPrice, true)
                .PriceColumn(7, "報價", d => d.QuotedPrice, true)
                .AddToBuilder(e);

            e.CreateRow();

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "◎");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "教室場地可使用時段：上午8:30-12:30，下午13:30-17:30，晚上18:00-21:00。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "承租場地依時段收費；超時費用以整點計費。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "場地特惠專案場地租金    折優惠。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "特別優惠訓練教室場地租金享9折優惠。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "特別優惠國際會議廳場地租金享1廳8折；2廳75折；3廳7折優惠。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "特別優惠階梯演講廳場地租金享78折優惠。");

            e.CreateRow();

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "優惠提供115,208,316為講師休息室；優惠提供110,111為行李置放室。");

            e.CreateRow();

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "請確認　貴公司發票抬頭為「                       」統一編號為「              」");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "是否為利害關係人交易　口是　口否                 ");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "萊斯查詢日期：     /    / ");

            e.CreateRow();

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "固定設備");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "316訓練教室:白板，前投投影機及螢幕，有線麥克風*1，無線麥克風(或耳掛式麥克風)*2");

            e.CreateRow();

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "◎");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "本中心僅提供活動場地租借。場地內所配置影音設備，台端可視活動性質所需無償使用之。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "惟就台端操作上開設備所導致影音軟體之毀損，本中心不負任何賠償責任。");
            e.CreateRow()
                .CombineCells()
                .SetValue(0, "#餐飲費用");

            return e.GetFile();
        }

        #endregion

        #region Initialization

        private readonly IGetListAllHelper<PrintReport_GetResverListByIds1_Input_APIItem> _getListAllHelper;

        public GetResverListByIDs1Controller()
        {
            _getListAllHelper =
                new GetListAllHelper<GetResverListByIDs1Controller, Resver_Head,
                    PrintReport_GetResverListByIds1_Input_APIItem,
                    PrintReport_GetResverListByIds1_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetResverListByIDs1

        // 實際 Route 請參考 RouteConfig。
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> GetList(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            bool isInputValid = input.StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(i => i.Id != null && i.Id.Any(),
                    () => AddError(EmptyNotAllowed("欲查詢之預約單 ID 集合", nameof(input.Id))))
                .Validate(i => i.Id.Distinct().Count() == i.Id.Count,
                    () => AddError(CopyNotAllowed("欲查詢之預約單 ID 集合", nameof(input.Id))))
                .IsValid();

            // 檢查所有 RHID 是否都存在
            bool isValid = isInputValid && // short-circuit
                           input.Id.Aggregate(true, (result, id) =>
                               result & // 一定走過所有資料，以便一次顯示所有找不到的錯誤訊息
                               id.StartValidate()
                                   .Validate(_ => DC.Resver_Head.Any(rh => !rh.DeleteFlag && rh.RHID == id),
                                       () => AddError(NotFound($"預約單 ID {id}", nameof(input.Id))))
                                   .IsValid()
                           );

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head> GetListAllOrderedQuery(
            PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            var query = DC.Resver_Head
                .Include(rh => rh.Customer)
                .Include(rh => rh.M_Resver_TimeSpan)
                .Include(rh => rh.M_Resver_TimeSpan.Select(rts => rts.D_TimeSpan))
                .Include(rh => rh.Resver_Site)
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Device.Select(rd => rd.B_Device)))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_SiteData))
                .Include(rh => rh.Resver_Site.Select(rs => rs.B_StaticCode))
                .AsQueryable();

            query = query.Where(rh => input.Id.Contains(rh.RHID));

            return query.OrderBy(rh => rh.RHID);
        }

        public async Task<PrintReport_GetResverListByIds1_Output_Row_APIItem> GetListAllEntityToRow(Resver_Head entity)
        {
            // 取得這筆資料的 M_Contact
            string tableName = DC.GetTableName<Resver_Head>();
            M_Contect[] contacts = await DC.M_Contect
                .Where(mc => mc.TargetTable == tableName && mc.TargetID == entity.RHID)
                .OrderBy(mc => mc.SortNo)
                .Take(2)
                .ToArrayAsync();

            var row = GetListAllPopulateRow(entity, contacts);

            return await Task.FromResult(row);
        }

        private PrintReport_GetResverListByIds1_Output_Row_APIItem GetListAllPopulateRow(Resver_Head entity,
            IReadOnlyList<M_Contect> contacts)
        {
            var row = new PrintReport_GetResverListByIds1_Output_Row_APIItem
            {
                RHID = entity.RHID,
                Code = entity.Code ?? entity.RHID.ToString(),
                CustomerTitle = entity.Customer?.TitleC ?? "",
                ContactTitle1 = contacts.Count >= 1
                    ? ContactTypeController.GetContactTypeTitle(contacts[0].ContectType) ?? ""
                    : "",
                ContactValue1 = contacts.Count >= 1 ? contacts[0].ContectData ?? "" : "",
                ContactTitle2 = contacts.Count >= 2
                    ? ContactTypeController.GetContactTypeTitle(contacts[1].ContectType) ?? ""
                    : "",
                ContactValue2 = contacts.Count >= 2 ? contacts[1].ContectData ?? "" : "",
                ContactName = entity.ContactName ?? "",
                Compilation = entity.Customer?.Compilation ?? "",
                Title = entity.Title ?? "",
                SDate = entity.SDate.ToFormattedStringDate(),
                EDate = entity.EDate.ToFormattedStringDate(),
                PeopleCt = entity.PeopleCt,
                QuotedPrice = entity.QuotedPrice,
                SiteItems = GetListAllPopulateRowSiteItems(entity)
            };
            return row;
        }

        private List<PrintReport_GetResverListByIds1_SiteItem_APIItem> GetListAllPopulateRowSiteItems(
            Resver_Head entity)
        {
            return entity.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .Select(rs => new PrintReport_GetResverListByIds1_SiteItem_APIItem
                {
                    RSID = rs.RSID,
                    Date = rs.TargetDate.ToFormattedStringDate(),
                    SiteTitle = rs.B_SiteData?.Title ?? "",
                    TableTitle = rs.B_StaticCode?.Title ?? "",
                    FixedPrice = rs.FixedPrice,
                    QuotedPrice = rs.QuotedPrice,
                    TimeSpanItems = GetListAllPopulateRowSiteItemTimeSpanItems(entity, rs),
                    DeviceItems = GetListAllPopulateRowSiteItemDeviceItems(rs)
                }).ToList();
        }

        private List<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem> GetListAllPopulateRowSiteItemTimeSpanItems(
            Resver_Head entity, Resver_Site rs)
        {
            return entity.M_Resver_TimeSpan
                .Where(rts =>
                    rts.TargetTable == DC.GetTableName<Resver_Site>()
                    && rts.TargetID == rs.RSID)
                .Select(rts => new PrintReport_GetResverListByIds1_TimeSpanItem_APIItem
                {
                    DTSID = rts.DTSID,
                    Title = rts.D_TimeSpan != null ? rts.D_TimeSpan.Title ?? "" : "",
                    TimeS = rts.D_TimeSpan != null
                        ? (rts.D_TimeSpan.HourS, rts.D_TimeSpan.MinuteS).ToFormattedHourAndMinute()
                        : "",
                    TimeE = rts.D_TimeSpan != null
                        ? (rts.D_TimeSpan.HourE, rts.D_TimeSpan.MinuteE).ToFormattedHourAndMinute()
                        : "",
                    Minutes = rts.D_TimeSpan != null
                        ? (rts.D_TimeSpan.HourS, rts.D_TimeSpan.MinuteS).GetMinutesUntil((rts.D_TimeSpan.HourE,
                            rts.D_TimeSpan.MinuteE))
                        : 0,
                }).ToList();
        }

        private static List<PrintReport_GetResverListByIds1_DeviceItem_APIItem>
            GetListAllPopulateRowSiteItemDeviceItems(Resver_Site rs)
        {
            return rs.Resver_Device
                .Where(rd => !rd.DeleteFlag)
                .Select(rd => new PrintReport_GetResverListByIds1_DeviceItem_APIItem
                {
                    RDID = rd.RDID,
                    TargetDate = rd.TargetDate.ToFormattedStringDate(),
                    BD_Title = rd.B_Device.Title ?? "",
                    SortNo = rd.SortNo,
                    Note = rd.Note ?? ""
                }).ToList();
        }

        #endregion
    }
}