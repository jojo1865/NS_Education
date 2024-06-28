using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using Microsoft.Ajax.Utilities;
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
        public async Task<ActionResult> GetExcel(PrintReport_GetResverListByIds1_Input_APIItem input)
        {
            ICollection<PrintReport_GetResverListByIds1_Output_Row_APIItem> data =
                await _getListAllHelper.GetRows<PrintReport_GetResverListByIds1_Output_Row_APIItem>(input);

            if (data is null)
                return GetContentResult();

            ExcelBuilder excelBuilder = new ExcelBuilder
            {
                ReportTitle = "報價暨預約確認單",
                Columns = 9
            };

            ExcelBuilderInfo info = await GetExcelBuilderInfo(data.Count);

            foreach (PrintReport_GetResverListByIds1_Output_Row_APIItem item in data)
            {
                GetExcel_MakePage(info, item, excelBuilder);
                info.NowPage++;
            }

            return excelBuilder.GetFile();
        }

        private void GetExcel_MakePage(ExcelBuilderInfo info,
            PrintReport_GetResverListByIds1_Output_Row_APIItem data,
            ExcelBuilder e)
        {
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
                .CombineCells(0, 2) // 主辦單位簽名：
                .CombineCells(3, 4) // 底線
                .CombineCells(5, 6) // 日期：
                .CombineCells(7, 8) // 底線
                .Align(0, HorizontalAlignment.Right)
                .Align(5, HorizontalAlignment.Right)
                .SetValue(0, "主辦單位簽名：")
                .SetValue(5, "日期：")
                .DrawBorder(BorderDirection.Bottom, 3, 4)
                .DrawBorder(BorderDirection.Bottom, 7, 8);

            e.CreateRow();

            e.CreateRow()
                .DrawBorder(BorderDirection.Top | BorderDirection.Left | BorderDirection.Right, true)
                .CombineCells(1, 2)
                .SetValue(0, "主辦單位：")
                .SetValue(1, data.CustomerTitle)
                .SetValueFromRight(1, "聯絡人：")
                .SetValueFromRight(0, data.ContactName);

            e.CreateRow()
                .DrawBorder(BorderDirection.Bottom | BorderDirection.Left | BorderDirection.Right, true)
                .CombineCells(1, 2)
                .CombineCells(5, 6)
                .SetValue(7, "統一編號：")
                .SetValue(8, data.Compilation)
                .Align(1, HorizontalAlignment.Left)
                .Align(5, HorizontalAlignment.Left)
                .Align(8, HorizontalAlignment.Right);

            if (data.ContactValue1.HasContent())
            {
                e.NowRow()
                    .SetValue(0, $"{data.ContactTitle1}：")
                    .SetValue(1, data.ContactValue1);
            }

            if (data.ContactValue2.HasContent())
            {
                e.NowRow()
                    .SetValue(4, $"{data.ContactTitle2}：")
                    .SetValue(5, data.ContactValue2);
            }

            e.CreateRow()
                .SetValue(0, "活動名稱：")
                .SetValue(1, data.Title);

            e.CreateRow()
                .SetValue(0, "活動日期：")
                .SetValue(1, $"{data.SDate}~{data.EDate}");

            e.CreateRow()
                .SetValue(0, "人數：")
                .SetValue(1, $"{data.PeopleCt}人");

            e.CreateRow()
                .SetValue(0, "團體報價：")
                .SetValue(1, $"{data.QuotedPrice.ToTaxIncluded():N0}元(含稅，不含旅行平安險)");

            e.CreateRow();

            e.CreateRow()
                .SetValue(0, "一、專案說明");

            e.CreateRow()
                .SetValue(0, "#場地費用");

            e.StartDefineTable<PrintReport_GetResverListByIds1_SiteItem_APIItem>()
                .StringColumn(0, "日期", d => d.Date)
                .StringColumn(1, "時段", d => d.TimeSpans)
                .StringColumn(2, "場地", d => d.SiteTitle)
                .StringColumn(4, "桌型", d => d.TableTitle)
                .NumberColumn(6, "定價", d => d.FixedPrice, true)
                .NumberColumn(7, "報價", d => d.QuotedPrice, true)
                .SetDataRows(data.SiteItems)
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

            foreach (PrintReport_GetResverListByIds1_SiteItem_APIItem site in data.SiteItems.DistinctBy(si =>
                         si.SiteTitle))
            {
                string devicesJoined = String.Join("，", site.DeviceItems.Select(di => $"{di.Title}*{di.Ct}"));
                e.CreateRow()
                    .CombineCells()
                    .SetValue(0, $"{site.SiteTitle}:{devicesJoined}");
            }

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

            e.StartDefineTable<PrintReport_GetResverListByIds1_FoodItem_APIItem>()
                .StringColumn(0, "日期", f => f.Date.ToFormattedStringDate())
                .StringColumn(1, "內容", f => f.Content)
                .StringColumn(2, "餐種", f => f.FoodType)
                .NumberColumn(6, "數量", f => f.Count)
                .NumberColumn(7, "單價", f => f.SinglePrice)
                .NumberColumn(8, "總價", f => f.TotalPrice, true)
                .SetDataRows(data.FoodItems)
                .AddToBuilder(e);

            e.CreateRow();

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "◎");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "請提前告知實際用餐人數，若有素食需求，亦請一併告知數量。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "提供紫外線殺菌冷、熱飲用水及紙杯。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "餐飲訂單數量，請最遲於活動前三個工作天作最後數量確認，倘未通知而發生任何相關費用或損害，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "請　貴單位自行負擔或與餐飲廠商自行協商解決，與本中心無涉。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "主辦單位自訂餐飲酌收清潔服務費。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "◎");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "本中心停車場共有車位167個，請依告示牌之使用狀況，分別駛入地下二樓或地下三樓，採先到先停，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "滿位時請勿駛入。為方便貴賓，本停車場採「管進不管出」方式停車，各車輛駕駛人應自負責其車輛及");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "物品之安全，本場只供停車，不負保管責任。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "請協助提醒於週遭停車之學員，勿將貴重物品放置車內，以維護財物安全。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "各車輛請停放於劃格線內，如有危害行、停車順暢或安全等之虞者，得予以拖吊移置他處。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "請於停車當日之活動結束將車輛駛離本停車場，若因故需將車輛停放過夜，請事先告知警衛室，未告");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "知原因連續停放三天（含）以上，其後續車輛處置所產生之相關費用(拖吊費等)車輛毀損責任及法律");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "責任由車主自負。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "◎");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "二、付款方式");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "1.收取訂金作業");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "客戶預約本中心場地，請在「報價暨預約確認單」簽名回傳後七日內以匯款支付訂金，訂金金額為");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "場地費用之20%。於上述規定期限內未付訂金時，本中心保有取消該場地預約之權利。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "完成訂金匯款後，請將匯款單傳真至(04)2389-3000；");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "匯款銀行：合作金庫銀行五洲分行");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "匯款帳號：0411705539901");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "匯款戶名：南山人壽保險股份有限公司");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "2.場地預約取消及變更作業");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "a.於場地使用日之三個月以前辦理取消者，所繳的金額扣除手續費後無息全數退還。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "b.於場地使用日之二個月前，但未滿三個月辦理取消者，所繳的金額退還75%。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "c.於場地使用日之一個月前，但未滿二個月辦理取消者，所繳的金額退還50%。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "d.於場地使用前一個月內辦理取消者，所繳的金額不予退還。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "e.於場地使用前一個月內不接受更換縮減場地退差價事宜。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "3.場地預約延期作業");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "凡同一活動場次之場地預約如因天災地變或不可抗力之因素需展延，得經本中心同意僅以一次為限，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "於展延通知日後3個月內使用完畢，但不接受更換縮減場地退差價事宜，逾期則所繳的訂金不予退還。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "4.場地費用尾款請於活動當日以現金或信用卡結帳。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "本中心場地租金、設備租金及其他收費服務項目，以場租發票開立。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "有關餐飲係代向廠商洽訂，請以現金支付給廠商，發票由廠商開立。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "餐飲訂單數量，請最遲於活動前七個工作天作最後數量確認，確認後因故取消發生任何相關費用或");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "損害，請　貴單位自行負擔或與廠商自行協商解決，與本中心無涉。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "旅行平安保險費用請於活動前以信用卡繳納，南山人壽確認後將寄予傷害暨健康保險費收據；或於");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "活動當日以現金繳納，本中心將代繳至承保單位，並於收到傷害暨健康保險費收據後立即");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "轉交　貴單位。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "三、注意事項");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "＊『為保障著作權及肖像權，請主辦單位(下稱 貴單位)於使用本中心場地及設備期間，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "須確實遵循「著作權法」及民法保護肖像權之相關規定，使用合法授權之影片、音樂、");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "文字著述、現場演奏或表演或肖像。如有任何違法情事，請 貴單位負完全責任，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "，與本中心無涉。』");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "＊已充分閱讀與瞭解『場地使用暨代辦服務說明』、『國際會議廳場地使用規範』");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "『展場作業注意事項』注意事項與相關內容。(內容載於本中心網站www.ns-etc.com.tw)");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "＊因颱風、豪雨或地震等天災，台中市政府或行政院人事行政總處公佈台中市停止上班，");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "本中心依政府規定休館，當天活動請擇期辦理。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "請於收悉本確認單之日起7日內簽名回傳，以利場地保留作業。如遇特殊情形請於確認詢問");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "當日回覆場地確認與否。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "希望在不久的將來能為您及　貴單位之貴賓，提供本中心高品質的環境及體貼的服務。");

            e.CreateRow()
                .CombineCells()
                .SetValue(0, "若有疑問及指教，敬請不吝來電指教，本中心當竭誠為您服務。");

            e.CreateRow()
                .CombineCells(6, 8)
                .SetValue(6, "南山人壽教育訓練中心");

            e.CreateRow()
                .CombineCells(6, 8)
                .SetValue(6, "　　　　　　陳 雅 芳    謹啟");

            e.CreateRow()
                .CombineCells(6, 8)
                .SetValue(6, "　　　　　　Tel：(04)2334-1800");

            e.CreateRow()
                .CombineCells(6, 8)
                .SetValue(6, "　　　　　　Fax：(04)2389-3000");
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
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw))
                .Include(rh => rh.Resver_Site.Select(rs => rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food)))
                .Include(rh => rh.Resver_Site.Select(rs =>
                    rs.Resver_Throw.Select(rt => rt.Resver_Throw_Food.Select(rtf => rtf.D_FoodCategory))))
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
                SiteItems = GetListAllPopulateRowSiteItems(entity),
                FoodItems = entity.Resver_Site
                    .Where(rs => !rs.DeleteFlag)
                    .SelectMany(rs => rs.Resver_Throw)
                    .Where(rt => !rt.DeleteFlag)
                    .SelectMany(rt => rt.Resver_Throw_Food.Select(rtf =>
                        new PrintReport_GetResverListByIds1_FoodItem_APIItem
                        {
                            Date = rt.TargetDate,
                            Content = rt.Title,
                            FoodType = rtf.D_FoodCategory.Title,
                            Count = rtf.Ct,
                            SinglePrice = rtf.UnitPrice
                        }))
            };
            return row;
        }

        private List<PrintReport_GetResverListByIds1_SiteItem_APIItem> GetListAllPopulateRowSiteItems(
            Resver_Head entity)
        {
            string tableName = DC.GetTableName<Resver_Site>();

            int[] rsIds = entity.Resver_Site.Select(rs => rs.RSID).Distinct().ToArray();

            ILookup<int, D_TimeSpan> rsIdToTimeSpans = DC.M_Resver_TimeSpan
                .Include(rts => rts.D_TimeSpan)
                .Where(rts => rts.TargetTable == tableName)
                .Where(rts => rsIds.Contains(rts.TargetID))
                .ToLookup(rts => rts.TargetID, rts => rts.D_TimeSpan);

            return entity.Resver_Site
                .Where(rs => !rs.DeleteFlag)
                .Select(rs =>
                {
                    D_TimeSpan[] timeSpans = rsIdToTimeSpans
                        .Where(x => x.Key == rs.RSID)
                        .SelectMany(dts => dts)
                        .OrderBy(dts => dts.HourS)
                        .ThenBy(dts => dts.MinuteS)
                        .ThenBy(dts => dts.HourE)
                        .ThenBy(dts => dts.MinuteE)
                        .ToArray();

                    return new PrintReport_GetResverListByIds1_SiteItem_APIItem
                    {
                        RSID = rs.RSID,
                        Date = rs.TargetDate.ToFormattedStringDate(),
                        TimeSpans = String.Join("~", new[]
                                { timeSpans.FirstOrDefault(), timeSpans.LastOrDefault() }
                            .Where(ts => ts != null)
                            .Select(ts => ts.Title)
                            .Distinct()),
                        SiteTitle = rs.B_SiteData?.Title ?? "",
                        TableTitle = rs.B_StaticCode?.Title ?? "",
                        FixedPrice = rs.FixedPrice,
                        QuotedPrice = rs.QuotedPrice,
                        DeviceItems = rs.B_SiteData?.GetDevicesFromSiteNotes()
                            .Devices
                            .Select(d => new PrintReport_GetResverListByIds1_DeviceItem_APIItem
                            {
                                Title = d.DeviceName,
                                Ct = d.Count ?? 0
                            })
                            .ToArray() ?? Array.Empty<PrintReport_GetResverListByIds1_DeviceItem_APIItem>()
                    };
                }).ToList();
        }

        #endregion
    }
}