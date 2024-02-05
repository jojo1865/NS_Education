using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems.Controller.Resver.Submit;
using NS_Education.Models.Entities;
using NS_Education.Tools.Extensions;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public partial class ResverController
    {
        private async Task<Resver_Head> _SubmitCreateData(Resver_Submit_Input_APIItem input, Resver_Head data = null)
        {
            // 基本參數初始化
            bool isAdd = SubmitIsAdd(input);
            IList<object> entitiesToAdd = new List<object>();

            // 取得主資料
            bool needsNewHead = data is null;
            Resver_Head head = needsNewHead ? SubmitFindOrCreateNew<Resver_Head>(input.RHID) : data;
            int originalHeadState = head.State;

            // 已結帳時，只允許處理預約回饋紀錄的值
            if (isAdd || head.State != (int)ReserveHeadGetListState.FullyPaid)
            {
                SubmitPopulateHeadValues(input, head);
                // 為新資料時, 先寫入 DB, 這樣才有 RHID 可以提供給後面的功能用
                if (head.RHID == 0)
                {
                    // 新增時，給 RHID
                    if (isAdd)
                    {
                        DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        DateTime monthEnd = monthStart.AddMonths(1);

                        int count = await DC.Resver_Head
                                        .Where(rh => monthStart <= rh.CreDate)
                                        .Where(rh => rh.CreDate < monthEnd)
                                        .CountAsync()
                                    + 1; // count 是 0-based.

                        string newCode = $"{DateTime.Now.Year % 100:00}{DateTime.Now.Month:00}{count:0000}";

                        head.RHID = Convert.ToInt32(newCode);
                    }

                    await DC.AddAsync(head);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                // 清理所有跟這張預約單有關的 ResverTimeSpan
                DC.M_Resver_TimeSpan.RemoveRange(head.M_Resver_TimeSpan);

                // 開始寫入值
                SubmitPopulateHeadContactItems(input, head, entitiesToAdd, isAdd);
                await SubmitPopulateHeadSiteItems(input, head, entitiesToAdd);
                SubmitPopulateHeadOtherItems(input, head, entitiesToAdd);
                SubmitPopulateHeadBillItems(input, head, entitiesToAdd);
            }

            SubmitPopulateHeadGiveBackItems(input, head, entitiesToAdd);
            SubmitPopulateQuestionnaireItems(input, head, entitiesToAdd);

            if (needsNewHead)
                WriteResverHeadLog(head.RHID, ReserveHeadGetListState.Draft);

            if (originalHeadState != head.State)
                WriteResverHeadLog(head.RHID, head.State);

            // 寫入 Db
            await DC.AddRangeAsync(entitiesToAdd);
            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            return head;
        }

        private void SubmitPopulateQuestionnaireItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            if (input.QuestionnaireItems is null)
                return;

            DC.Resver_Questionnaire.RemoveRange(head.Resver_Questionnaire);
            head.Resver_Questionnaire.Clear();

            foreach (var kvp in input.QuestionnaireItems)
            {
                bool tryParseNumber = int.TryParse(kvp.Value.ToString(), out int number);

                Resver_Questionnaire questionnaire = new Resver_Questionnaire
                {
                    QuestionKey = kvp.Key,
                    NumberContent = tryParseNumber ? number : (int?)null,
                    TextContent = kvp.Value.ToString(),
                    RHID = head.RHID
                };

                entitiesToAdd.Add(questionnaire);
                head.Resver_Questionnaire.Add(questionnaire);
            }
        }

        private void SubmitPopulateHeadGiveBackItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            // 刪除沒有在輸入中的 giveback
            var inputIds = input.GiveBackItems.Select(gbi => gbi.RGBID);
            DC.Resver_GiveBack.RemoveRange(head.Resver_GiveBack.Where(rgb => !inputIds.Contains(rgb.RGBID)));

            foreach (Resver_Submit_GiveBackItem_Input_APIItem item in input.GiveBackItems)
            {
                Resver_GiveBack giveBack = SubmitFindOrCreateNew<Resver_GiveBack>(item.RGBID, entitiesToAdd);
                if (giveBack.RHID != 0 && giveBack.RHID != head.RHID)
                {
                    AddErrorNotThisHead(item.RGBID, "預約回饋紀錄", giveBack.RHID);
                    continue;
                }

                giveBack.RHID = head.RHID;
                giveBack.Title = item.Title;
                giveBack.Description = item.Description;
                giveBack.BSCID16 = item.BSCID16;
            }
        }

        private void SubmitPopulateHeadBillItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            var inputIds = input.BillItems.Select(bi => bi.RBID);
            DC.Resver_Bill.RemoveRange(head.Resver_Bill.Where(rb => !inputIds.Contains(rb.RBID)));
            foreach (var item in input.BillItems)
            {
                Resver_Bill bill = SubmitFindOrCreateNew<Resver_Bill>(item.RBID, entitiesToAdd);
                if (bill.RHID != 0 && bill.RHID != head.RHID)
                {
                    AddErrorNotThisHead(bill.RBID, "繳費紀錄", bill.RHID);
                    continue;
                }

                bill.RHID = head.RHID;
                bill.BCID = item.BCID;
                bill.DPTID = item.DPTID;
                bill.Price = item.Price;
                bill.Note = item.Note;
                bill.PayFlag = item.PayFlag;
                bill.PayDate = item.PayDate.HasContent() ? item.PayDate.ParseDateTime() : SqlDateTime.MinValue.Value;
                bill.CheckUID = head.UpdUID;
            }
        }

        private void SubmitPopulateHeadOtherItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd)
        {
            var inputIds = input.OtherItems.Select(oi => oi.ROID);
            DC.Resver_Other.RemoveRange(head.Resver_Other.Where(ro => !inputIds.Contains(ro.ROID)));

            foreach (var item in input.OtherItems)
            {
                Resver_Other other = SubmitFindOrCreateNew<Resver_Other>(item.ROID, entitiesToAdd);
                if (other.RHID != 0 && other.RHID != head.RHID)
                {
                    AddErrorNotThisHead(other.ROID, "其他收費項目", other.RHID);
                    continue;
                }

                other.TargetDate = item.TargetDate.ParseDateTime().Date;
                other.RHID = head.RHID;
                other.DOPIID = item.DOPIID;
                other.BOCID = item.BOCID;
                other.BSCID = item.BSCID;
                other.PrintTitle = item.PrintTitle;
                other.PrintNote = item.PrintNote;
                other.UnitPrice = item.UnitPrice;
                other.FixedPrice = item.FixedPrice;
                other.Ct = item.Ct;
                other.QuotedPrice = item.QuotedPrice;
                other.SortNo = item.SortNo;
                other.Note = item.Note;
            }
        }

        private async Task SubmitPopulateHeadSiteItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            IList<object> entitiesToAdd)
        {
            var inputIds = input.SiteItems.Select(si => si.RSID);
            DC.Resver_Site.RemoveRange(head.Resver_Site.Where(rs => !inputIds.Contains(rs.RSID)));

            foreach (var item in input.SiteItems)
            {
                Resver_Site site = SubmitFindOrCreateNew<Resver_Site>(item.RSID);
                if (site.RHID != 0 && site.RHID != head.RHID)
                {
                    AddErrorNotThisHead(site.RSID, "場地", site.RHID);
                    continue;
                }

                site.TargetDate = item.TargetDate.ParseDateTime().Date;
                site.RHID = head.RHID;
                site.BSID = item.BSID;
                site.BOCID = item.BOCID;
                site.PrintTitle = item.PrintTitle;
                site.PrintNote = item.PrintNote;
                site.UnitPrice = item.UnitPrice;
                site.FixedPrice = item.FixedPrice;
                site.QuotedPrice = item.QuotedPrice;
                site.SortNo = item.SortNo;
                site.Note = item.Note;
                site.BSCID = item.BSCID;
                site.ArriveTimeStart =
                    item.ArriveTimeStart.TryParseTimeSpan(out TimeSpan start) ? start : (TimeSpan?)null;
                site.ArriveTimeEnd = item.ArriveTimeEnd.TryParseTimeSpan(out TimeSpan end) ? end : (TimeSpan?)null;
                site.ArriveDescription = item.ArriveDescription;
                site.TableDescription = item.TableDescription;
                site.SeatImage = item.SeatImage != null ? Convert.FromBase64String(item.SeatImage) : null;

                // 先儲存至 DB, 才有 RSID...
                if (site.RSID == 0)
                {
                    await DC.AddAsync(site);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                SubmitPopulateSiteItemTimeSpanItems(item, head, site);
                await SubmitPopulateSiteItemThrowItems(item, head, site, entitiesToAdd);
                await SubmitPopulateSiteItemDeviceItems(item, head, site);
            }
        }

        private async Task SubmitPopulateSiteItemDeviceItems(Resver_Submit_SiteItem_Input_APIItem item,
            Resver_Head head, Resver_Site site)
        {
            var inputIds = item.DeviceItems.Select(di => di.RDID);
            DC.Resver_Device.RemoveRange(site.Resver_Device.Where(rd => !inputIds.Contains(rd.RDID)));
            foreach (var deviceItem in item.DeviceItems)
            {
                Resver_Device device = SubmitFindOrCreateNew<Resver_Device>(deviceItem.RDID);
                if (device.Resver_Site != null && device.Resver_Site.RHID != 0 && device.Resver_Site.RHID != head.RHID)
                {
                    AddErrorNotThisHead(device.RDID, "場地設備", device.Resver_Site.RHID);
                    continue;
                }

                device.TargetDate = deviceItem.TargetDate.ParseDateTime().Date;
                device.RSID = site.RSID;
                device.BDID = deviceItem.BDID;
                device.Ct = deviceItem.Ct;
                device.BOCID = deviceItem.BOCID;
                device.PrintTitle = deviceItem.PrintTitle;
                device.PrintNote = deviceItem.PrintNote;
                device.UnitPrice = deviceItem.UnitPrice;
                device.FixedPrice = deviceItem.FixedPrice;
                device.QuotedPrice = deviceItem.QuotedPrice;
                device.SortNo = deviceItem.SortNo;
                device.Note = deviceItem.Note;

                site.Resver_Device.Add(device);
                // 先儲存至 DB 才有 RDID...
                if (device.RDID == 0)
                {
                    await DC.Resver_Device.AddAsync(device);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                // 寫入 device 的 TimeSpan
                int sortNo = 0;
                string deviceTableName = DC.GetTableName<Resver_Device>();
                foreach (var timeSpanItem in deviceItem.TimeSpanItems)
                {
                    M_Resver_TimeSpan ts = new M_Resver_TimeSpan
                    {
                        RHID = head.RHID,
                        TargetTable = deviceTableName,
                        TargetID = device.RDID,
                        DTSID = timeSpanItem,
                        SortNo = ++sortNo
                    };

                    head.M_Resver_TimeSpan.Add(ts);
                }
            }
        }

        private async Task SubmitPopulateSiteItemThrowItems(Resver_Submit_SiteItem_Input_APIItem item,
            Resver_Head head, Resver_Site site, IList<object> entitiesToAdd)
        {
            DC.Resver_Throw.RemoveRange(site.Resver_Throw.Where(rt => item.ThrowItems.All(ti => ti.RTID != rt.RTID)));
            foreach (var throwItem in item.ThrowItems)
            {
                Resver_Throw throwData = SubmitFindOrCreateNew<Resver_Throw>(throwItem.RTID);
                if (throwData.Resver_Site != null && throwData.Resver_Site.RHID != 0 &&
                    throwData.Resver_Site.RHID != head.RHID)
                {
                    AddErrorNotThisHead(throwData.RTID, "場地行程", throwData.Resver_Site.RHID);
                    continue;
                }

                throwData.TargetDate = throwItem.TargetDate.ParseDateTime().Date;
                throwData.RSID = site.RSID;
                throwData.BSCID = throwItem.BSCID;
                throwData.Title = throwItem.Title;
                throwData.BOCID = throwItem.BOCID;
                throwData.PrintTitle = throwItem.PrintTitle;
                throwData.PrintNote = throwItem.PrintNote;
                throwData.UnitPrice = throwItem.UnitPrice;
                throwData.FixedPrice = throwItem.FixedPrice;
                throwData.QuotedPrice = throwItem.QuotedPrice;
                throwData.SortNo = throwItem.SortNo;
                throwData.Note = throwItem.Note;

                site.Resver_Throw.Add(throwData);

                // 先儲存才有 RTID 給 Resver_TimeSpan 用...
                if (throwData.RTID == 0)
                {
                    await DC.AddAsync(throwData);
                    await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
                }

                SubmitPopulateSiteItemThrowItemTimeSpanItems(throwData, throwItem, head);
                SubmitPopulateSiteItemThrowItemThrowFoodItems(throwData, throwItem, entitiesToAdd);
            }
        }

        private void SubmitPopulateSiteItemThrowItemThrowFoodItems(Resver_Throw throwData,
            Resver_Submit_ThrowItem_Input_APIItem throwItem, IList<object> entitiesToAdd)
        {
            DC.Resver_Throw_Food.RemoveRange(
                throwData.Resver_Throw_Food.Where(rtf => throwItem.FoodItems.All(fi => fi.RTFID != rtf.RTFID)));

            foreach (Resver_Submit_FoodItem_Input_APIItem foodItem in throwItem.FoodItems)
            {
                Resver_Throw_Food throwFood = SubmitFindOrCreateNew<Resver_Throw_Food>(foodItem.RTFID, entitiesToAdd);
                if (throwFood.Resver_Throw != null && throwFood.Resver_Throw.RTID != 0 &&
                    throwFood.Resver_Throw.RTID != throwData.RTID)
                {
                    AddErrorNotThisThrow(throwFood.RTFID, "餐飲補充資料", throwFood.Resver_Throw.RTID);
                    continue;
                }

                throwFood.RTID = throwData.RTID;
                throwFood.DFCID = foodItem.DFCID;
                throwFood.BSCID = foodItem.BSCID;
                throwFood.BPID = foodItem.BPID;
                throwFood.Ct = foodItem.Ct;
                throwFood.UnitPrice = foodItem.UnitPrice;
                throwFood.Price = foodItem.Price;
                throwFood.ArriveTime =
                    foodItem.ArriveTime.TryParseTimeSpan(out TimeSpan arrive) ? arrive : (TimeSpan?)null;
            }
        }

        private void SubmitPopulateSiteItemThrowItemTimeSpanItems(Resver_Throw throwData,
            Resver_Submit_ThrowItem_Input_APIItem throwItem,
            Resver_Head head)
        {
            string throwTableName = DC.GetTableName<Resver_Throw>();
            int sortNo = 0;
            foreach (var timeSpanItem in throwItem.TimeSpanItems)
            {
                M_Resver_TimeSpan resverTimeSpan = new M_Resver_TimeSpan
                {
                    RHID = head.RHID,
                    TargetTable = throwTableName,
                    TargetID = throwData.RTID,
                    DTSID = timeSpanItem,
                    SortNo = ++sortNo
                };
                head.M_Resver_TimeSpan.Add(resverTimeSpan);
            }
        }

        private void SubmitPopulateSiteItemTimeSpanItems(Resver_Submit_SiteItem_Input_APIItem item, Resver_Head head,
            Resver_Site site)
        {
            int sortNo = 0;
            string siteTableName = DC.GetTableName<Resver_Site>();

            foreach (var timeSpanItem in item.TimeSpanItems)
            {
                M_Resver_TimeSpan resverTimeSpan = new M_Resver_TimeSpan
                {
                    RHID = head.RHID,
                    TargetTable = siteTableName,
                    TargetID = site.RSID,
                    DTSID = timeSpanItem,
                    SortNo = ++sortNo
                };

                head.M_Resver_TimeSpan.Add(resverTimeSpan);
            }
        }

        private void SubmitPopulateHeadContactItems(Resver_Submit_Input_APIItem input, Resver_Head head,
            ICollection<object> entitiesToAdd, bool isAdd)
        {
            string tableName = DC.GetTableName<Resver_Head>();

            if (!isAdd)
            {
                // 先清除所有原本有的 M_Contect
                var originalContacts = DC.M_Contect
                    .Where(c => c.TargetTable == tableName && c.TargetID == input.RHID)
                    .AsEnumerable();

                DC.M_Contect.RemoveRange(originalContacts);
            }

            if (input.ContactType1 != null)
            {
                M_Contect contact = SubmitFindOrCreateNew<M_Contect>(0, entitiesToAdd);
                contact.ContectType = input.ContactType1.Value;
                contact.TargetTable = tableName;
                contact.TargetID = head.RHID;
                contact.ContectData = input.ContactData1;
                contact.SortNo = 1;
            }

            if (input.ContactType2 != null)
            {
                M_Contect contact = SubmitFindOrCreateNew<M_Contect>(0, entitiesToAdd);
                contact.ContectType = input.ContactType2.Value;
                contact.TargetTable = tableName;
                contact.TargetID = head.RHID;
                contact.ContectData = input.ContactData2;
                contact.SortNo = 2;
            }
        }

        private void SubmitPopulateHeadValues(Resver_Submit_Input_APIItem input, Resver_Head head)
        {
            int billSum = input.BillItems
                .Where(bi => bi.PayFlag)
                .Sum(bi => bi.Price);

            ReserveHeadGetListState state = head.DeleteFlag ? ReserveHeadGetListState.Deleted
                : input.FinishDeal ? ReserveHeadGetListState.FullyPaid
                : head.CheckInFlag ? ReserveHeadGetListState.CheckedIn
                : head.CheckFlag ? ReserveHeadGetListState.Checked
                : ReserveHeadGetListState.Draft;

            head.State = (int)state;
            head.BSCID11 = input.BSCID11;
            head.Title = input.Title;
            head.SDate = input.SDate.ParseDateTime().Date;
            head.EDate = input.EDate.ParseDateTime().Date;
            head.PeopleCt = input.PeopleCt;
            head.CID = input.CID;
            head.CustomerTitle = input.CustomerTitle;
            head.ContactName = input.ContactName;
            head.MK_BUID = input.MK_BUID;
            head.MK_Phone = input.MK_Phone;
            head.OP_BUID = input.OP_BUID;
            head.OP_Phone = input.OP_Phone;
            head.Note = input.Note;
            head.FixedPrice = input.FixedPrice;
            head.QuotedPrice = input.QuotedPrice;
            head.MKT = input.MKT;
            head.Owner = input.Owner;
            head.ParkingNote = input.ParkingNote;

            if (input.FinishDeal)
                WriteResverHeadLog(head.RHID, (int)ReserveHeadGetListState.FullyPaid,
                    input.FinishDealDate.ParseDateTime());
        }

        private T SubmitFindOrCreateNew<T>(int id, ICollection<object> entitiesToAdd = null)
            where T : class
        {
            T t = null;
            if (id != 0)
                t = DC.Set<T>().Find(id);
            if (t != null
                && (t.GetIfHasProperty<T, bool?>(DbConstants.ActiveFlag) ?? true)
                && (t.GetIfHasProperty<T, bool?>(DbConstants.DeleteFlag) ?? false) == false)
            {
                // 取得這個物件的 navigationProperties
                var objectContext = ((IObjectContextAdapter)DC).ObjectContext;

                var entityType = objectContext
                    .MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.CSpace)
                    .FirstOrDefault(et => et.Name == t.GetType().Name);

                if (entityType == null)
                    return t;

                var navigationProperties = entityType
                    .NavigationProperties;

                // 讀取所有 FK property
                foreach (string propertyName in navigationProperties.Select(navigationProperty =>
                             navigationProperty.Name))
                {
                    objectContext.LoadProperty(t, propertyName);
                }

                return t;
            }

            t = Activator.CreateInstance<T>();
            entitiesToAdd?.Add(t);
            return t;
        }
    }
}