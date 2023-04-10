using Antlr.Runtime.Tree;
using NS_Education.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace NS_Education.Controllers
{
    public class DepartmentController : PublicClass
    {
        [HttpGet]
        public string GetList(string KeyWord = "", int DCID = 0, int NowPage = 1, int CutPage = 10)
        {

            var Ns = DC.D_Department.Where(q => !q.DeleteFlag);
            if (DCID > 0)
                Ns = Ns.Where(q => q.DCID == DCID);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.TitleC.Contains(KeyWord) || q.TitleC.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_Department_List ListData = new D_Department_List();
            ListData.Items = new List<D_Department_APIItem>();
            ListData.SuccessFlag = Ns.Count() > 0;
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;
            ListData.AllItemCt = Ns.Count();
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);

            if (NowPage == 0)
                Ns = Ns.Where(q=>q.ActiveFlag).OrderBy(q => q.TitleC);
            else
                Ns = Ns.OrderBy(q => q.TitleC).Skip((NowPage - 1) * CutPage).Take(CutPage);
           
            foreach (var N in Ns)
            {
                ListData.Items.Add(new D_Department_APIItem
                {
                    DDID = N.DDID,
                    DCID = N.DCID,
                    DC_TitleC = N.D_Company.TitleC,
                    DC_TitleE = N.D_Company.TitleE,
                    CompanyList = null,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    PeopleCt = N.PeopleCt,
                    HallCt = N.D_Hall.Count(),
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                }); ;
            }

            return ChangeJson(ListData);
        }

        [HttpGet]
        public string GetInfoByID(int ID = 0)
        {
            var N = DC.D_Department.FirstOrDefault(q => q.DDID == ID && !q.DeleteFlag);
            D_Department_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Coms = DC.D_Company.Where(q => !q.DeleteFlag).OrderBy(q => q.TitleC);
                foreach (var Com in Coms)
                    SIs.Add(new cSelectItem { ID = Com.DCID, Title = Com.TitleC, SelectFlag = N.DCID == Com.DCID });
                Item = new D_Department_APIItem
                {
                    DDID = N.DDID,
                    DCID = N.DCID,
                    DC_TitleC = N.D_Company.TitleC,
                    DC_TitleE = N.D_Company.TitleE,
                    CompanyList = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    PeopleCt = N.PeopleCt,
                    HallCt = N.D_Hall.Count(),
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }

            return ChangeJson(Item);
        }
        [HttpGet]
        public string ChangeActive(int ID, bool ActiveFlag, int UID)
        {
            Error = "";
            if (UID == 0)
                Error += "缺少更新者ID,無法更新;";
            else
            {
                var N_ = DC.D_Department.FirstOrDefault(q => q.DDID == ID && !q.DeleteFlag);
                if (N_ != null)
                {
                    N_.ActiveFlag = ActiveFlag;
                    N_.UpdDate = DT;
                    N_.UpdUID = UID;
                    DC.SubmitChanges();
                }
                else
                    Error += "查無資料,無法更新;";
            }

            return ChangeJson(GetMsgClass(Error));
        }
        [HttpGet]
        public string DeleteItem(int ID, int UID)
        {
            Error = "";
            if (UID == 0)
                Error += "缺少更新者ID,無法更新;";
            else
            {
                var N_ = DC.D_Department.FirstOrDefault(q => q.DDID == ID);
                if (N_ != null)
                {
                    N_.DeleteFlag = true;
                    N_.UpdDate = DT;
                    N_.UpdUID = UID;
                    DC.SubmitChanges();
                }
                else
                    Error += "查無資料,無法更新;";
            }

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpPost]
        public string Submit(D_Department N)
        {
            Error = "";
            if (N.DDID == 0)
            {
                if (N.CreUID == 0)
                    Error += "缺少建立者ID,無法更新;";
                if(N.DCID<=0)
                    Error += "請選擇部門所屬公司;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if(Error=="")
                {
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    DC.D_Department.InsertOnSubmit(N);
                    DC.SubmitChanges();
                }
            }
            else
            {
                var N_ = DC.D_Department.FirstOrDefault(q => q.DDID == N.DDID && !q.DeleteFlag);
                if (N.CreUID == 0)
                    Error += "缺少更新者ID,無法更新;";
                if (N.DCID <= 0)
                    Error += "請選擇部門所屬公司;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.DCID = N.DCID;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
                    N_.PeopleCt = N.PeopleCt;
                    N_.ActiveFlag = N.ActiveFlag;
                    N_.DeleteFlag = N.DeleteFlag;
                    N_.UpdUID = N.UpdUID;
                    N_.UpdDate = DT;
                    DC.SubmitChanges();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}