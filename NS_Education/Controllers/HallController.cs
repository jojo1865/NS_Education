using NS_Education.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace NS_Education.Controllers
{
    public class HallController : PublicClass
    {
        [HttpGet]
        public string GetList(string KeyWord = "", int DDID = 0, int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_Hall.Where(q => !q.DeleteFlag);
            if (DDID > 0)
                Ns = Ns.Where(q => q.DDID == DDID);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.TitleC.Contains(KeyWord) || q.TitleC.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_Hall_List ListData = new D_Hall_List();
            ListData.Items = new List<D_Hall_APIItem>();
            ListData.SuccessFlag = Ns.Count() > 0;
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;
            ListData.AllItemCt = Ns.Count();
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);

            if (NowPage == 0)
                Ns = Ns.Where(q => q.ActiveFlag).OrderBy(q => q.TitleC);
            else
                Ns = Ns.OrderBy(q => q.TitleC).Skip((NowPage - 1) * CutPage).Take(CutPage);

            foreach (var N in Ns)
            {
                ListData.Items.Add(new D_Hall_APIItem
                {
                    DHID = N.DHID,
                    DDID = N.DDID,
                    DD_TitleC = N.D_Department.TitleC,
                    DD_TitleE = N.D_Department.TitleE,
                    DepartmentList = null,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,

                    DiscountFlag = N.DiscountFlag,
                    CheckoutNowFlag = N.CheckoutNowFlag,
                    PrintCheckFlag = N.PrintCheckFlag,
                    Invoice3Flag = N.Invoice3Flag,
                    CheckType = N.CheckType,
                    BusinessTaxRate = N.BusinessTaxRate,

                    DeviceCt = N.B_Device.Count(),
                    SiteCt = N.B_SiteData.Count(),
                    PartnerItemCt = N.B_PartnerItem.Count(),

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                });
            }

            return ChangeJson(ListData);
        }

        [HttpGet]
        public string GetInfoByID(int ID = 0)
        {
            var N = DC.D_Hall.FirstOrDefault(q => q.DDID == ID && !q.DeleteFlag);
            D_Hall_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Deps = DC.D_Department.Where(q => !q.DeleteFlag).OrderBy(q => q.TitleC);
                foreach (var Dep in Deps)
                    SIs.Add(new cSelectItem { ID = Dep.DDID, Title = Dep.TitleC, SelectFlag = N.DDID == Dep.DDID });
                Item = new D_Hall_APIItem
                {
                    DDID = N.DDID,
                    DHID = N.DHID,
                    DD_TitleC = N.D_Department.TitleC,
                    DD_TitleE = N.D_Department.TitleE,
                    DepartmentList = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,

                    DiscountFlag = N.DiscountFlag,
                    CheckoutNowFlag = N.CheckoutNowFlag,
                    PrintCheckFlag = N.PrintCheckFlag,
                    Invoice3Flag = N.Invoice3Flag,
                    CheckType = N.CheckType,
                    BusinessTaxRate = N.BusinessTaxRate,

                    DeviceCt = N.B_Device.Count(),
                    SiteCt = N.B_SiteData.Count(),
                    PartnerItemCt = N.B_PartnerItem.Count(),

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
                var N_ = DC.D_Hall.FirstOrDefault(q => q.DHID == ID && !q.DeleteFlag);
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
                var N_ = DC.D_Hall.FirstOrDefault(q => q.DHID == ID);
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
        public string Submit(D_Hall N)
        {
            Error = "";
            if (N.DHID == 0)
            {
                if (N.CreUID == 0)
                    Error += "缺少建立者ID,無法更新;";
                if (N.DDID <= 0)
                    Error += "請選擇廳別所屬部門;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    DC.D_Hall.InsertOnSubmit(N);
                    DC.SubmitChanges();
                }
            }
            else
            {
                var N_ = DC.D_Hall.FirstOrDefault(q => q.DHID == N.DHID && !q.DeleteFlag);
                if (N.CreUID == 0)
                    Error += "缺少更新者ID,無法更新;";
                if (N.DDID <= 0)
                    Error += "請選擇廳別所屬部門;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.DDID = N.DDID;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
                    N_.DiscountFlag = N.DiscountFlag;
                    N_.CheckoutNowFlag = N.CheckoutNowFlag;
                    N_.PrintCheckFlag = N.PrintCheckFlag;
                    N_.Invoice3Flag = N.Invoice3Flag;
                    N_.CheckType = N.CheckType;
                    N_.BusinessTaxRate = N.BusinessTaxRate;
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