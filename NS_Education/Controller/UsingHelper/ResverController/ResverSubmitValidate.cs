using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.Resver.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public partial class ResverController
    {
        public async Task<bool> SubmitAddValidateInput(Resver_Submit_Input_APIItem input)
        {
            return await SubmitValidateInput(input);
        }

        private async Task<bool> SubmitValidateInput(Resver_Submit_Input_APIItem input)
        {
            bool isAdd = SubmitIsAdd(input);

            // 修改時，有一些值需要參照已有資料
            if (!isAdd)
            {
                // 先確認預約單狀態，如果是已中止，直接報錯
                Resver_Head head = await DC.Resver_Head
                    .FirstOrDefaultAsync(rh => rh.RHID == input.RHID);

                if (head != null && head.State == (int)ReserveHeadGetListState.Terminated)
                {
                    AddError(1, "預約單已中止，無法更新！");
                    return false;
                }
            }

            DateTime headStartDate = default;
            DateTime headEndDate = default;

            // 主預約單
            int billSum = input.BillItems
                .Where(bi => bi.PayFlag)
                .Sum(bi => bi.Price);

            bool isHeadValid = await input.StartValidate()
                .ForceSkipIf(i => !i.FinishDeal)
                .Validate(i => billSum >= input.QuotedPrice,
                    () => AddError(ExpectedValue("繳費紀錄已付總額", nameof(input.BillItems), input.QuotedPrice)))
                .Validate(i => i.FinishDealDate.TryParseDateTime(out _),
                    () => AddError(WrongFormat("結帳日期", nameof(input.FinishDealDate))))
                .StopForceSkipping()
                .Validate(i => isAdd ? i.RHID == 0 : i.RHID.IsZeroOrAbove(),
                    () => AddError(WrongFormat("預約單 ID", nameof(input.RHID))))
                .ValidateAsync(
                    async i => isAdd || await DC.Resver_Head.ValidateIdExists(i.RHID, nameof(Resver_Head.RHID)),
                    () => AddError(NotFound("預約單 ID", nameof(input.RHID))))
                .ValidateAsync(async i => await SubmitValidateStaticCode(i.BSCID11, StaticCodeType.ResverSource),
                    () => AddError(NotFound("預約來源 ID", nameof(input.BSCID11))))
                .Validate(i => i.Title.HasContent(), () => AddError(EmptyNotAllowed("預約單名稱", nameof(input.Title))))
                .Validate(i => i.SDate.TryParseDateTime(out headStartDate),
                    () => AddError(WrongFormat("預約單起始日", nameof(input.SDate))))
                .Validate(i => i.EDate.TryParseDateTime(out headEndDate),
                    () => AddError(WrongFormat("預約單結束日", nameof(input.EDate))))
                .Validate(i => headEndDate.Date >= headStartDate.Date,
                    () => AddError(MinLargerThanMax("預約單起始日", nameof(input.SDate), "預約單結束日", nameof(input.EDate))))
                .ValidateAsync(async i => await SubmitValidateCustomerId(i.CID),
                    () => AddError(NotFound("客戶", nameof(input.CID))))
                .Validate(i => i.CustomerTitle.HasContent(),
                    () => AddError(EmptyNotAllowed("客戶名稱", nameof(input.CustomerTitle))))
                .Validate(i => i.Title.HasLengthBetween(1, 100),
                    () => AddError(LengthOutOfRange("預約單名稱", nameof(input.Title), 1, 100)))
                .Validate(i => i.CustomerTitle.HasLengthBetween(1, 100),
                    () => AddError(LengthOutOfRange("客戶名稱", nameof(input.CustomerTitle), 1, 100)))
                .Validate(i => i.ContactName.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("聯絡人名稱", nameof(input.ContactName), 0, 50)))
                .Validate(i => i.MK_Phone.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("MK 業務電話", nameof(input.MK_Phone), 0, 50)))
                .Validate(i => i.OP_Phone.HasLengthBetween(0, 50),
                    () => AddError(LengthOutOfRange("OP 業務電話", nameof(input.OP_Phone), 0, 50)))
                .Validate(i => i.Note.HasLengthBetween(0, 10),
                    () => AddError(LengthOutOfRange("備註", nameof(input.Note), 0, 10)))
                .ValidateAsync(async i => await SubmitValidateMKBusinessUser(i.MK_BUID),
                    () => AddError(NotFound("MK 業務", nameof(input.MK_BUID))))
                .ValidateAsync(async i => await SubmitValidateOPBusinessUser(i.OP_BUID),
                    () => AddError(NotFound("OP 業務", nameof(input.OP_BUID))))
                .Validate(i => i.ContactType1 == null || SubmitValidateContactType(i.ContactType1.Value),
                    () => AddError(NotFound($"聯絡方式1", nameof(input.ContactType1))))
                .Validate(i => i.ContactData1.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("聯絡方式1", nameof(input.ContactData1), 0, 30)))
                .Validate(i => i.ContactType2 == null || SubmitValidateContactType(i.ContactType2.Value),
                    () => AddError(NotFound($"聯絡方式2", nameof(input.ContactType2))))
                .Validate(i => i.ContactData2.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("聯絡方式2", nameof(input.ContactData2), 0, 30)))
                .Validate(i => i.MKT.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("MKT", nameof(input.MKT), 0, 30)))
                .Validate(i => i.Owner.HasLengthBetween(0, 30),
                    () => AddError(LengthOutOfRange("Owner", nameof(input.Owner), 0, 30)))
                .IsValid();

            // short-circuit
            if (!isHeadValid)
                return false;

            // 主預約單 -> 場地列表
            bool isSiteItemsValid = input.SiteItems.All(item =>
                item.StartValidate()
                    .Validate(si => isAdd ? si.RSID == 0 : si.RSID.IsZeroOrAbove(),
                        () => AddError(WrongFormat($"場地預約單 ID（{item.RSID}）", nameof(item.RSID))))
                    .Validate(
                        si => si.RSID == 0 || Task
                            .Run(() => DC.Resver_Site.ValidateIdExists(si.RSID, nameof(Resver_Site.RSID))).Result,
                        () => AddError(NotFound($"場地預約單 ID（{item.RSID}）", nameof(item.RSID))))
                    // 檢查所有場地的目標日期都位於 head 的日期範圍
                    .Validate(si => si.TargetDate.TryParseDateTime(out DateTime siteTargetDate)
                                    && headStartDate.Date <= siteTargetDate.Date &&
                                    siteTargetDate.Date <= headEndDate.Date,
                        () => AddError(OutOfRange($"場地使用日期（{item.TargetDate}）",
                            nameof(item.TargetDate),
                            headStartDate.ToFormattedStringDate(),
                            headEndDate.ToFormattedStringDate())))
                    .Validate(si => Task.Run(() => SubmitValidateSiteData(si.BSID)).Result,
                        () => AddError(NotFound($"場地 ID（{item.BSID}）", nameof(item.BSID))))
                    .Validate(si => Task.Run(() => SubmitValidateOrderCode(si.BOCID, OrderCodeType.Site)).Result,
                        () => AddError(NotFound($"預約場地的入帳代號 ID（{item.BOCID}）", nameof(item.BOCID))))
                    .Validate(si => Task.Run(() => SubmitValidateStaticCode(si.BSCID, StaticCodeType.SiteTable)).Result,
                        () => AddError(NotFound($"預約場地的桌型 ID（{item.BSCID}）", nameof(item.BSCID))))
                    .Validate(si => si.PrintTitle.HasLengthBetween(0, 100),
                        () => AddError(LengthOutOfRange("帳單列印名稱", nameof(item.PrintTitle), 0, 100)))
                    .Validate(si => si.PrintNote.HasLengthBetween(0, 100),
                        () => AddError(LengthOutOfRange("帳單列印說明", nameof(item.PrintNote), 0, 100)))
                    .Validate(
                        si => si.ArriveTimeStart.IsNullOrWhiteSpace() || si.ArriveTimeStart.TryParseTimeSpan(out _),
                        () => AddError(WrongFormat("活動抵達最早時間", nameof(item.ArriveTimeStart))))
                    .Validate(si => si.ArriveTimeEnd.IsNullOrWhiteSpace() || si.ArriveTimeEnd.TryParseTimeSpan(out _),
                        () => AddError(WrongFormat("活動抵達最晚時間", nameof(item.ArriveTimeEnd))))
                    .IsValid());

            // 檢查場地的總可容納人數大於等於預約單要求人數
            IEnumerable<int> siteItemIds = input.SiteItems.Select(si => si.BSID);
            int totalSize =
                await DC.B_SiteData.Where(sd => siteItemIds.Contains(sd.BSID)).SumAsync(sd => (int?)sd.BasicSize) ?? 0;

            isSiteItemsValid = isSiteItemsValid &&
                               input.SiteItems.StartValidate()
                                   .Validate(si => totalSize >= input.PeopleCt,
                                       () => AddError(TooLarge($"預約人數（{input.PeopleCt}）", nameof(input.PeopleCt),
                                           $"場地可容納人數（{totalSize}）")))
                                   .IsValid();

            // 主預約單 -> 場地列表 -> 時段列表
            bool isSiteItemTimeSpanItemValid = isSiteItemsValid
                                               && SubmitValidateTimeSpanItems(
                                                   input.SiteItems.SelectMany(si => si.TimeSpanItems), null)
                                               && await SubmitValidateSiteItemsAllTimeSpanFree(input)
                ;

            // 主預約單 -> 場地列表 -> 行程列表
            bool isSiteItemThrowItemValid = isSiteItemsValid &&
                                            input.SiteItems
                                                .All(si => si.ThrowItems.All(item =>
                                                    item.StartValidate()
                                                        .Validate(ti => isAdd ? ti.RTID == 0 : ti.RTID.IsZeroOrAbove(),
                                                            () => AddError(WrongFormat($"行程預約單 ID（{item.RTID}）",
                                                                nameof(item.RTID))))
                                                        .Validate(
                                                            ti => ti.RTID == 0 || Task.Run(() =>
                                                                DC.Resver_Throw.ValidateIdExists(ti.RTID,
                                                                    nameof(Resver_Throw.RTID))).Result,
                                                            () => AddError(NotFound($"行程預約單 ID（{item.RTID}）",
                                                                nameof(item.RTID))))
                                                        .Validate(
                                                            ti => Task.Run(() =>
                                                                SubmitValidateStaticCode(ti.BSCID,
                                                                    StaticCodeType.ResverThrow)).Result,
                                                            () => AddError(WrongFormat($"預約類型（{item.BSCID}）",
                                                                nameof(item.BSCID))))
                                                        .Validate(
                                                            ti => Task.Run(() =>
                                                                    SubmitValidateOrderCode(ti.BOCID,
                                                                        OrderCodeType.Throw))
                                                                .Result,
                                                            () => AddError(NotFound($"預約行程的入帳代號 ID（{item.BOCID}）",
                                                                nameof(item.BOCID))))
                                                        .Validate(ti => ti.Title.HasLengthBetween(0, 100),
                                                            () => AddError(LengthOutOfRange("行程名稱", nameof(item.Title),
                                                                0, 100)))
                                                        .Validate(ti => ti.PrintTitle.HasLengthBetween(0, 100),
                                                            () => AddError(LengthOutOfRange("行程的帳單列印名稱",
                                                                nameof(item.PrintTitle), 0, 100)))
                                                        .Validate(ti => ti.PrintNote.HasLengthBetween(0, 100),
                                                            () => AddError(LengthOutOfRange("行程的帳單列印說明",
                                                                nameof(item.PrintNote), 0, 100)))
                                                        .IsValid()));

            // 主預約單 -> 場地列表 -> 行程列表 -> 時段列表
            bool isSiteItemThrowItemTimeSpanItemValid = isSiteItemThrowItemValid &&
                                                        input.SiteItems.StartValidateElements()
                                                            .Validate(si =>
                                                                SubmitValidateTimeSpanItems(
                                                                    si.ThrowItems.SelectMany(ti => ti.TimeSpanItems),
                                                                    SubmitValidateGetTimeSpans(
                                                                        si.TimeSpanItems)
                                                                )
                                                            )
                                                            .IsValid();

            // 主預約單 -> 場地列表 -> 行程列表 -> 餐飲補充列表
            bool isSiteItemThrowItemFoodItemValid = isSiteItemsValid &&
                                                    input.SiteItems
                                                        .SelectMany(si => si.ThrowItems)
                                                        .SelectMany(ti => ti.FoodItems)
                                                        .StartValidateElements()
                                                        .Validate(
                                                            fi => isAdd
                                                                ? fi.RTFID == 0
                                                                : fi.RTFID.IsZeroOrAbove(),
                                                            fi => AddError(
                                                                WrongFormat($"行程餐飲預約單 ID（{fi.RTFID}）",
                                                                    nameof(fi.RTFID))))
                                                        .Validate(
                                                            fi => fi.RTFID == 0 || Task.Run(() =>
                                                                DC.Resver_Throw_Food.ValidateIdExists(fi.RTFID,
                                                                    nameof(Resver_Throw_Food.RTFID))).Result,
                                                            fi => AddError(
                                                                NotFound($"行程餐飲預約單 ID（{fi.RTFID}）", nameof(fi.RTFID))))
                                                        .Validate(fi => SubmitValidateFoodCategory(fi.DFCID),
                                                            fi => AddError(
                                                                NotFound($"預約行程的餐種 ID（{fi.DFCID}）", nameof(fi.DFCID))))
                                                        .Validate(
                                                            fi => Task.Run(() =>
                                                                SubmitValidateStaticCode(fi.BSCID,
                                                                    StaticCodeType.Cuisine)).Result,
                                                            fi => AddError(
                                                                NotFound($"預約行程的餐別 ID（{fi.BSCID}）", nameof(fi.BSCID))))
                                                        .Validate(fi => SubmitValidatePartner(fi.BPID),
                                                            fi => AddError(
                                                                NotFound($"預約行程的廠商 ID（{fi.BPID}）", nameof(fi.BPID))))
                                                        .Validate(
                                                            fi => fi.ArriveTime.IsNullOrWhiteSpace() ||
                                                                  fi.ArriveTime.TryParseTimeSpan(out _),
                                                            fi => AddError(WrongFormat("送達時間", nameof(fi.ArriveTime))))
                                                        .IsValid()
                ;

            // 主預約單 -> 場地列表 -> 設備列表
            bool isSiteItemDeviceItemValid = isSiteItemsValid &&
                                             input.SiteItems.All(si => si.DeviceItems.All(item =>
                                                 item.StartValidate()
                                                     .Validate(di => isAdd ? di.RDID == 0 : di.RDID.IsZeroOrAbove(),
                                                         () => AddError(WrongFormat($"設備預約單 ID（{item.RDID}）",
                                                             nameof(item.RDID))))
                                                     .Validate(di => di.RDID == 0 || Task.Run(() =>
                                                             DC.Resver_Device.ValidateIdExists(item.RDID,
                                                                 nameof(Resver_Device.RDID))).Result
                                                         , () => AddError(NotFound($"設備預約單 ID（{item.RDID}）", nameof(item.RDID))))
                                                     .Validate(di => SubmitValidateDevice(di.BDID),
                                                         () => AddError(NotFound($"預約設備 ID（{item.BDID}）",
                                                             nameof(item.BDID))))
                                                     .Validate(
                                                         di => Task.Run(() =>
                                                                 SubmitValidateOrderCode(di.BOCID,
                                                                     OrderCodeType.Device))
                                                             .Result,
                                                         () => AddError(NotFound($"預約設備的入帳代號 ID（{item.BOCID}）",
                                                             nameof(item.BOCID))))
                                                     .Validate(di => di.PrintTitle.HasLengthBetween(0, 100),
                                                         () => AddError(LengthOutOfRange("預約設備的帳單列印名稱",
                                                             nameof(item.PrintTitle), 0, 100)))
                                                     .Validate(di => di.PrintNote.HasLengthBetween(0, 100),
                                                         () => AddError(LengthOutOfRange("預約設備的帳單列印說明",
                                                             nameof(item.PrintNote), 0, 100)))
                                                     .IsValid()
                                             ));

            // 主預約單 -> 場地列表 -> 設備列表 -> 時段列表
            bool isSiteItemDeviceItemTimeSpanItemValid = isSiteItemDeviceItemValid
                                                         && input.SiteItems.StartValidateElements()
                                                             .Validate(si =>
                                                                 SubmitValidateTimeSpanItems(
                                                                     si.DeviceItems.SelectMany(di => di.TimeSpanItems),
                                                                     SubmitValidateGetTimeSpans(
                                                                         si.TimeSpanItems)
                                                                 )
                                                             )
                                                             .IsValid();

            // 主預約單 -> 其他收費項目列表
            bool isOtherItemValid = await
                input.OtherItems.StartValidateElements()
                    .Validate(oi => isAdd ? oi.ROID == 0 : oi.ROID.IsZeroOrAbove(),
                        item => AddError(WrongFormat($"其他收費項目預約單 ID（{item.ROID}）", nameof(item.ROID))))
                    .Validate(
                        oi => oi.ROID == 0 || Task.Run(() =>
                            DC.Resver_Other.ValidateIdExists(oi.ROID, nameof(Resver_Other.ROID))).Result,
                        item => AddError(NotFound($"其他收費項目預約單 ID（{item.ROID}）", nameof(item.ROID))))
                    // 檢查所有項目的日期都與主預約單相符
                    .Validate(oi => oi.TargetDate.TryParseDateTime(out DateTime otherItemDate)
                                    && headStartDate.Date <= otherItemDate.Date
                                    && otherItemDate.Date <= headEndDate.Date,
                        item => AddError(OutOfRange($"其他收費項目的預計使用日期（{item.TargetDate}）",
                            nameof(item.TargetDate),
                            headStartDate.ToFormattedStringDate(), headEndDate.ToFormattedStringDate()))
                    )
                    .Validate(oi => SubmitValidateOtherPayItem(oi.DOPIID),
                        item => AddError(NotFound($"其他收費項目 ID（{item.DOPIID}）", nameof(item.DOPIID))))
                    .ValidateAsync(
                        async oi => await SubmitValidateOrderCode(oi.BOCID, OrderCodeType.OtherPayItem) ||
                                    await SubmitValidateOrderCode(oi.BOCID, OrderCodeType.General),
                        item => AddError(NotFound($"其他收費項目的入帳代號 ID（{item.BOCID}）", nameof(item.BOCID))))
                    .ValidateAsync(
                        async oi => await DC.B_StaticCode.ValidateStaticCodeExists(oi.BSCID, StaticCodeType.Unit),
                        item => AddError(NotFound($"其他收費項目的單位別 ID（{item.BSCID}）", nameof(item.BSCID))))
                    .Validate(oi => oi.PrintTitle.HasLengthBetween(0, 100),
                        item => AddError(LengthOutOfRange("其他收費項目的帳單列印名稱", nameof(item.PrintTitle), 0, 100)))
                    .Validate(oi => oi.PrintNote.HasLengthBetween(0, 100),
                        item => AddError(LengthOutOfRange("其他收費項目的帳單列印說明", nameof(item.PrintNote), 0, 100)))
                    .IsValid();

            // 主預約單 -> 繳費紀錄列表
            bool isBillItemValid =
                input.BillItems.All(item => item.StartValidate()
                    .Validate(bi => isAdd ? bi.RBID == 0 : bi.RBID.IsZeroOrAbove(),
                        () => AddError(WrongFormat($"繳費紀錄預約單 ID（{item.RBID}）", nameof(item.RBID))))
                    .Validate(
                        bi => bi.RBID == 0 || Task
                            .Run(() => DC.Resver_Bill.ValidateIdExists(bi.RBID, nameof(Resver_Bill.RBID))).Result,
                        () => AddError(NotFound($"繳費紀錄預約單 ID（{item.RBID}）", nameof(item.RBID))))
                    .Validate(bi => SubmitValidateCategory(bi.BCID, CategoryType.PayType),
                        () => AddError(NotFound($"繳費類別 ID（{item.BCID}）", nameof(item.BCID))))
                    .Validate(bi => SubmitValidatePayType(bi.DPTID),
                        () => AddError(NotFound($"繳費紀錄的付款方式 ID（{item.DPTID}）", nameof(item.DPTID))))
                    .Validate(
                        bi => bi.PayDate.IsNullOrWhiteSpace() ||
                              bi.PayDate.TryParseDateTime(out _),
                        () => AddError(WrongFormat($"付款時間（{item.PayDate}）", nameof(item.PayDate))))
                    .IsValid());

            // 已付總額不得超過 head 總價
            isBillItemValid = isBillItemValid &&
                              input.BillItems.StartValidate()
                                  .Validate(billItems => billItems
                                                             .Where(bi => bi.PayFlag)
                                                             .Sum(bi => bi.Price)
                                                         <= input.QuotedPrice,
                                      () => AddError(
                                          TooLarge("繳費紀錄的已繳總額", nameof(input.QuotedPrice), input.QuotedPrice)))
                                  .IsValid();

            // 主預約單 -> 預約回饋紀錄列表
            bool isGiveBackItemValid = await
                input.GiveBackItems.StartValidateElements()
                    .Validate(gbi => isAdd ? gbi.RGBID == 0 : gbi.RGBID.IsZeroOrAbove(),
                        gbi => AddError(WrongFormat($"預約回饋預約單 ID（{gbi.RGBID}）", nameof(gbi.RGBID))))
                    .ValidateAsync(
                        async gbi =>
                            gbi.RGBID == 0 ||
                            await DC.Resver_GiveBack.ValidateIdExists(gbi.RGBID, nameof(Resver_GiveBack.RGBID)),
                        gbi => AddError(NotFound($"預約回饋預約單 ID（{gbi.RGBID}）", nameof(gbi.RGBID))))
                    .ValidateAsync(
                        async gbi =>
                            await DC.B_StaticCode.ValidateStaticCodeExists(gbi.BSCID16, StaticCodeType.GiveBackScore),
                        gbi => AddError(NotFound($"回饋分數 ID（{gbi.BSCID16}）", nameof(gbi.BSCID16))))
                    .Validate(gbi => gbi.Title.HasLengthBetween(0, 100),
                        gbi => AddError(LengthOutOfRange("預約回饋的標題", nameof(gbi.Title), 0, 100)))
                    .Validate(gbi => gbi.Description.HasLengthBetween(0, 100),
                        gbi => AddError(LengthOutOfRange("預約回饋的內容", nameof(gbi.Description), 0, 100)))
                    .IsValid();

            // 輸入都正確後，才計算各項目價格
            bool isEverythingValid = isSiteItemsValid
                                     && isSiteItemTimeSpanItemValid
                                     && isSiteItemThrowItemValid
                                     && isSiteItemThrowItemTimeSpanItemValid
                                     && isSiteItemThrowItemFoodItemValid
                                     && isSiteItemDeviceItemValid
                                     && isSiteItemDeviceItemTimeSpanItemValid
                                     && isOtherItemValid
                                     && isBillItemValid
                                     && isGiveBackItemValid;

            return await Task.FromResult(isEverythingValid);
        }

        private async Task<bool> SubmitValidateSiteItemsAllTimeSpanFree(Resver_Submit_Input_APIItem input)
        {
            bool isValid = true;
            foreach (Resver_Submit_SiteItem_Input_APIItem si in input.SiteItems)
            {
                // 1. 取得這個場地當天所有 RTS，包括場地本身、場地的父場地、場地的子場地
                B_SiteData siteData =
                    await DC.B_SiteData.FirstOrDefaultAsync(sd =>
                        sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == si.BSID);
                M_Resver_TimeSpan[] allResverTimeSpans = SubmitGetAllResverTimeSpanFromSiteItem(input, si)
                    .ToArray();

                // 2. RTS 的 DTSID = 當天已被占用的 DTSID，從輸入中抓出此類 DTSID
                isValid &= allResverTimeSpans
                    .StartValidateElements()
                    .Validate(rts => si.TimeSpanItems.All(tsi => tsi != rts.DTSID),
                        rts => AddError(22,
                            $"{siteData?.Title ?? $"場地 ID {si.BSID}"} 欲預約的時段（{rts.D_TimeSpan.GetTimeRangeFormattedString()}）當天已被預約了！")
                    )
                    .IsValid();


                // 3. 所有 TimeSpanItem 的 DTS 時段不可與 allResverTimeSpans 任一者的 DTS 時段重疊
                // 先查出所有輸入 DTSID 的 DTS 資料
                var inputDtsIds = si.TimeSpanItems;
                List<D_TimeSpan> allInputDts = await DC.D_TimeSpan
                    .Where(dts =>
                        dts.ActiveFlag
                        && !dts.DeleteFlag
                        && inputDtsIds.Any(id => id == dts.DTSID)
                    )
                    .ToListAsync();

                // 每個 DTS 和 RTS 比對一次，看是否有重疊的部分
                foreach (D_TimeSpan dts in allInputDts)
                {
                    isValid &= allResverTimeSpans
                        // 排除同一場地預約單
                        .Where(rts => rts.TargetID != si.RSID)
                        .Aggregate(true, (result, rts) => result & rts.StartValidate()
                            .Validate(_ => rts.DTSID == dts.DTSID || !rts.D_TimeSpan.IsCrossingWith(dts),
                                () => AddError(23,
                                    $"{siteData?.Title ?? $"場地 ID {si.BSID}"} 欲預約的時段（{dts.GetTimeRangeFormattedString()}）與當天另一個已被預約的時段（{rts.D_TimeSpan.GetTimeRangeFormattedString()}）部分重疊！")
                            )
                            .IsValid());
                }
            }

            return isValid;
        }

        private IEnumerable<M_Resver_TimeSpan> SubmitGetAllResverTimeSpanFromSiteItem(Resver_Submit_Input_APIItem input,
            Resver_Submit_SiteItem_Input_APIItem si)
        {
            string resverSiteTableName = DC.GetTableName<Resver_Site>();
            DateTime targetDate = si.TargetDate.ParseDateTime();
            return DC.B_SiteData.Where(sd =>
                        sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == si.BSID)
                    .AsEnumerable()
                    .Concat(DC.M_SiteGroup
                        .Where(sg =>
                            sg.ActiveFlag && !sg.DeleteFlag &&
                            sg.MasterID == si.BSID)
                        .Select(sg => sg.B_SiteData1)
                        .AsEnumerable())
                    .Concat(DC.M_SiteGroup
                        .Where(sg =>
                            sg.ActiveFlag && !sg.DeleteFlag &&
                            sg.GroupID == si.BSID)
                        .Select(sg => sg.B_SiteData)
                        .AsEnumerable())
                    // 取得除去本預約單以外，每個場地在指定日期當天的預約
                    .SelectMany(sd => sd.Resver_Site.Where(rs => rs.RHID != input.RHID)
                        .Where(rs => !rs.DeleteFlag)
                        .Where(rs => rs.TargetDate.Date == targetDate.Date))
                    // 取得每個場地的預約時段
                    .SelectMany(rs => DC.M_Resver_TimeSpan
                        .Include(rts => rts.D_TimeSpan)
                        // 這裡的 TargetID == rs.RSID 處理的對象是「本預約單以外的 RS」，所以不會造成只搜到這張場地預約單自己的情況
                        .Where(rts =>
                            rts.TargetTable == resverSiteTableName &&
                            rts.TargetID == rs.RSID)
                    )
                ;
        }

        private bool SubmitValidatePayType(int payTypeId)
        {
            return payTypeId.IsAboveZero() &&
                   DC.D_PayType.Any(pt => pt.ActiveFlag && !pt.DeleteFlag && pt.DPTID == payTypeId);
        }

        private bool SubmitValidateCategory(int categoryId, CategoryType categoryType)
        {
            return Task.Run(() => DC.B_Category.ValidateCategoryExists(categoryId, categoryType)).Result;
        }

        private bool SubmitValidateOtherPayItem(int otherPayItemId)
        {
            return otherPayItemId.IsAboveZero() &&
                   DC.D_OtherPayItem.Any(opi => opi.ActiveFlag && !opi.DeleteFlag && opi.DOPIID == otherPayItemId);
        }

        private bool SubmitValidateDevice(int deviceId)
        {
            return deviceId.IsAboveZero() &&
                   DC.B_Device.Any(bd => bd.ActiveFlag && !bd.DeleteFlag && bd.BDID == deviceId);
        }

        private bool SubmitValidatePartner(int partnerId)
        {
            return Task.Run(() => DC.B_Partner.ValidatePartnerExists(partnerId)).Result;
        }

        private bool SubmitValidateFoodCategory(int foodCategoryId)
        {
            return foodCategoryId.IsAboveZero() && DC.D_FoodCategory.Any(dfc =>
                dfc.ActiveFlag && !dfc.DeleteFlag && dfc.DFCID == foodCategoryId);
        }

        private Dictionary<int, D_TimeSpan> SubmitValidateGetTimeSpansDictionary(IEnumerable<int> DtsIds)
        {
            return DC.D_TimeSpan.Where(dts => DtsIds.Contains(dts.DTSID)).ToDictionary(dts => dts.DTSID, dts => dts);
        }

        private IEnumerable<D_TimeSpan> SubmitValidateGetTimeSpans(IEnumerable<int> DtsIds)
        {
            return DC.D_TimeSpan.Where(dts => DtsIds.Contains(dts.DTSID)).ToArray();
        }

        /// <summary>
        /// 驗證輸入的 TimeSpanItem 是否格式正確。<br/>
        /// 當 parentTimeSpan 不為 null 時，驗證時段是否都包含於 parentTimeSpan 中的時段（考慮 DTSID 與實際時間）
        /// </summary>
        /// <param name="items">輸入</param>
        /// <param name="parentTimeSpan">用於檢查的上層項目預約時段</param>
        /// <returns>
        /// true：時段皆正確。<br/>
        /// false：有時段格式錯誤，或是不存在於上層項目預約的時段中。
        /// </returns>
        private bool SubmitValidateTimeSpanItems(IEnumerable<int> items,
            IEnumerable<D_TimeSpan> parentTimeSpan)
        {
            int[] inputDtsIds = items.ToArray();

            // 查出輸入的 TimeSpanItem 的所有對應的 DTS
            Dictionary<int, D_TimeSpan> dtsData = SubmitValidateGetTimeSpansDictionary(inputDtsIds);

            // 驗證所有 DTSID 都存在
            bool isInputValid = inputDtsIds.StartValidateElements()
                .Validate(id => dtsData.ContainsKey(id),
                    id => AddError(NotFound($"預約時段 ID {id}", "TimeSpanItems")))
                .IsValid();

            if (!isInputValid)
                return false;

            // 驗證所有 items 的區間都存在於 parentTimeSpan 中
            // 如果沒有傳入 parentTimeSpan, 表示沒有限制

            if (parentTimeSpan == null)
                return true;

            bool isValid = dtsData.Values.StartValidateElements()
                .Validate(dts => parentTimeSpan.Any(parent => parent.DTSID == dts.DTSID || parent.IsIncluding(dts))
                    , dts => AddError(2, $"欲預約的時段（{dts.GetTimeRangeFormattedString()}）並不存在於上層項目的預約時段！"))
                .IsValid();
            return isValid;
        }

        private async Task<bool> SubmitValidateOrderCode(int orderCodeId, OrderCodeType codeType)
        {
            return await DC.B_OrderCode.ValidateOrderCodeExists(orderCodeId, codeType);
        }

        private async Task<bool> SubmitValidateSiteData(int siteDataId)
        {
            return siteDataId.IsAboveZero() &&
                   await DC.B_SiteData.AnyAsync(sd => sd.ActiveFlag && !sd.DeleteFlag && sd.BSID == siteDataId);
        }

        private static bool SubmitValidateContactType(int contactType)
        {
            return ContactTypeController.GetContactTypeList().Any(ct => ct.ID == contactType);
        }

        private async Task<bool> SubmitValidateOPBusinessUser(int businessUserId)
        {
            return businessUserId.IsAboveZero() && await DC.BusinessUser.AnyAsync(bu =>
                bu.ActiveFlag && !bu.DeleteFlag && bu.OPsalesFlag && bu.BUID == businessUserId);
        }

        private async Task<bool> SubmitValidateMKBusinessUser(int businessUserId)
        {
            return businessUserId.IsAboveZero() && await DC.BusinessUser.AnyAsync(bu =>
                bu.ActiveFlag && !bu.DeleteFlag && bu.MKsalesFlag && bu.BUID == businessUserId);
        }

        private async Task<bool> SubmitValidateCustomerId(int customerId)
        {
            return customerId.IsAboveZero() &&
                   await DC.Customer.AnyAsync(c => c.ActiveFlag && !c.DeleteFlag && c.CID == customerId);
        }

        private async Task<bool> SubmitValidateStaticCode(int staticCodeId, StaticCodeType codeType)
        {
            return await DC.B_StaticCode.ValidateStaticCodeExists(staticCodeId, codeType);
        }

        private void AddErrorNotThisHead(int itemId, string itemName, int dataHeadId)
        {
            if (itemId != 0)
                AddError(3, $"欲更新的{itemName}（ID {itemId}）並不屬於此預約單（該{itemName}對應預約單 ID：{dataHeadId}）！");
            else
                AddError(3, $"欲新增的{itemName}並不屬於此預約單（該{itemName}對應預約單 ID：{dataHeadId}）！");
        }

        private void AddErrorNotThisThrow(int itemId, string itemName, int dataThrowId)
        {
            if (itemId != 0)
                AddError(3, $"欲更新的{itemName}（ID {itemId}）並不屬於此預約行程（該預約行程 ID：{dataThrowId}）！");
            else
                AddError(3, $"欲新增的{itemName}並不屬於此預約行程（該預約行程 ID：{dataThrowId}）！");
        }
    }
}