using NS_Education.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.TimeSpan;
using NS_Education.Models.Entities;

namespace NS_Education.Controllers
{
    public class TimeSpanController : PublicClass
    {
        [HttpGet]
        public async Task<string> GetList(string KeyWord = "", int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_TimeSpan.Where(q => !q.DeleteFlag);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.Title.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_TimeSpan_List ListData = new D_TimeSpan_List();
            ListData.Items = new List<D_TimeSpan_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q => q.ActiveFlag).OrderBy(q => q.Code);
            else
                Ns = Ns.OrderBy(q => q.Title).Skip((NowPage - 1) * CutPage).Take(CutPage);
            
            var NsList = await Ns.ToListAsync();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                DateTime DT_S = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourS + ":" + N.MinuteS + ":00");
                DateTime DT_E = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourE + ":" + N.MinuteE + ":00");
                TimeSpan TS = DT_E - DT_S;
                ListData.Items.Add(new D_TimeSpan_APIItem
                {
                    DTSID = N.DTSID,

                    Code = N.Code,
                    Title = N.Title,

                    HourS = N.HourS,
                    MinuteS = N.MinuteS,
                    HourE = N.HourE,
                    MinuteE = N.MinuteE,
                    TimeS = N.HourS.ToString().PadLeft(2, '0') + ":" + N.MinuteS.ToString().PadLeft(2, '0'),
                    TimeE = N.HourE.ToString().PadLeft(2, '0') + ":" + N.MinuteE.ToString().PadLeft(2, '0'),
                    GetTimeSpan = (TS.Hours > 0 ? TS.Hours + "小時" : "") + TS.Minutes.ToString() + "分鐘",
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                }); ;
            }

            return ChangeJson(ListData);
        }

        [HttpGet]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_TimeSpan.FirstOrDefaultAsync(q => q.DTSID == ID && !q.DeleteFlag);
            D_TimeSpan_APIItem Item = null;
            if (N != null)
            {
                DateTime DT_S = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourS + ":" + N.MinuteS + ":00");
                DateTime DT_E = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourE + ":" + N.MinuteE + ":00");
                TimeSpan TS = DT_E - DT_S;


                Item = new D_TimeSpan_APIItem
                {
                    DTSID = N.DTSID,

                    Code = N.Code,
                    Title = N.Title,

                    HourS = N.HourS,
                    MinuteS = N.MinuteS,
                    HourE = N.HourE,
                    MinuteE = N.MinuteE,
                    TimeS = N.HourS.ToString().PadLeft(2, '0') + ":" + N.MinuteS.ToString().PadLeft(2, '0'),
                    TimeE = N.HourE.ToString().PadLeft(2, '0') + ":" + N.MinuteE.ToString().PadLeft(2, '0'),
                    GetTimeSpan = (TS.Hours > 0 ? TS.Hours + "小時" : "") + TS.Minutes.ToString() + "分鐘",

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }

            return ChangeJson(Item);
        }
        [HttpGet]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag, int UID)
        {
            Error = "";
            if (UID == 0)
                Error += "缺少更新者ID,無法更新;";
            else
            {
                var N_ = await DC.D_TimeSpan.FirstOrDefaultAsync(q => q.DTSID == ID && !q.DeleteFlag);
                if (N_ != null)
                {
                    N_.ActiveFlag = ActiveFlag;
                    N_.UpdDate = DT;
                    N_.UpdUID = UID;
                    await DC.SaveChangesAsync();
                }
                else
                    Error += "查無資料,無法更新;";
            }

            return ChangeJson(GetMsgClass(Error));
        }
        [HttpGet]
        public async Task<string> DeleteItem(int ID, int UID)
        {
            Error = "";
            if (UID == 0)
                Error += "缺少更新者ID,無法更新;";
            else
            {
                var N_ = await DC.D_TimeSpan.FirstOrDefaultAsync(q => q.DTSID == ID);
                if (N_ != null)
                {
                    N_.DeleteFlag = true;
                    N_.UpdDate = DT;
                    N_.UpdUID = UID;
                    await DC.SaveChangesAsync();
                }
                else
                    Error += "查無資料,無法更新;";
            }

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpPost]
        public async Task<string> Submit(D_TimeSpan N)
        {
            Error = "";
            if (N.DTSID == 0)
            {
                if (N.CreUID == 0)
                    Error += "缺少建立者ID,無法更新;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.HourS < 0 || N.HourS > 23)
                    Error += "請輸入起始的小時;";
                if (N.MinuteS < 0 || N.MinuteS > 59)
                    Error += "請輸入起始的分鐘數;";
                if (N.HourE < 0 || N.HourE > 23)
                    Error += "請輸入結束的小時;";
                if (N.MinuteE < 0 || N.MinuteE > 59)
                    Error += "請輸入結束的分鐘數;";

                DateTime DT_S = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourS + ":" + N.MinuteS + ":00");
                DateTime DT_E = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourE + ":" + N.MinuteE + ":00");
                if(DT_E<=DT_S)
                    Error += "結束的時間應該在起始的時間之後;";

                if (Error == "")
                {
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_TimeSpan.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_TimeSpan.FirstOrDefaultAsync(q => q.DTSID == N.DTSID && !q.DeleteFlag);
                if (N.CreUID == 0)
                    Error += "缺少更新者ID,無法更新;";

                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.HourS < 0 || N.HourS > 23)
                    Error += "請輸入起始的小時;";
                if (N.MinuteS < 0 || N.MinuteS > 59)
                    Error += "請輸入起始的分鐘數;";
                if (N.HourE < 0 || N.HourE > 23)
                    Error += "請輸入結束的小時;";
                if (N.MinuteE < 0 || N.MinuteE > 59)
                    Error += "請輸入結束的分鐘數;";

                DateTime DT_S = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourS + ":" + N.MinuteS + ":00");
                DateTime DT_E = Convert.ToDateTime(DT.Year + "/" + DT.Month + "/" + DT.Day + " " + N.HourE + ":" + N.MinuteE + ":00");
                if (DT_E <= DT_S)
                    Error += "結束的時間應該在起始的時間之後;";

                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.DTSID = N.DTSID;
                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.HourS = N.HourS;
                    N_.MinuteS = N.MinuteS;
                    N_.HourE = N.HourE;
                    N_.MinuteE = N.MinuteE;
                    
                    N_.ActiveFlag = N.ActiveFlag;
                    N_.DeleteFlag = N.DeleteFlag;
                    N_.UpdUID = N.UpdUID;
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}