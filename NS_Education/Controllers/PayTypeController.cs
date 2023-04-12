using NS_Education.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.Entities;

namespace NS_Education.Controllers
{
    public class PayTypeController : PublicClass
    {
        [HttpGet]
        public string GetList(string KeyWord = "", int BCID = 0, int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_PayType.Where(q => !q.DeleteFlag);
            if (BCID > 0)
                Ns = Ns.Where(q => q.BCID == BCID);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.Title.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_PayType_List ListData = new D_PayType_List();
            ListData.Items = new List<D_PayType_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q=>q.ActiveFlag).OrderBy(q => q.Title);
            else
                Ns = Ns.OrderBy(q => q.Title).Skip((NowPage - 1) * CutPage).Take(CutPage);

            Ns = Ns.Include(q => q.BC);

            var NsList = Ns.ToList();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                ListData.Items.Add(new D_PayType_APIItem
                {
                    DPTID = N.DPTID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    CategoryList = null,
                    Code = N.Code,
                    Title = N.Title,

                    AccountingNo = N.AccountingNo,
                    CustormerNo = N.CustormerNo,
                    InvoiceFlag = N.InvoiceFlag,
                    DepositFlag = N.DepositFlag,
                    RestaurantFlag = N.RestaurantFlag,
                    SimpleCheckoutFlag = N.SimpleCheckoutFlag,
                    SimpleDepositFlag = N.SimpleDepositFlag,

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                }); ; ;
            }

            return ChangeJson(ListData);
        }

        [HttpGet]
        public string GetInfoByID(int ID = 0)
        {
            var N = DC.D_PayType.FirstOrDefault(q => q.DPTID == ID && !q.DeleteFlag);
            D_PayType_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Cats = DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == 8).OrderBy(q => q.SortNo);
                foreach (var Cat in Cats)
                    SIs.Add(new cSelectItem { ID = Cat.BCID, Title = Cat.TitleC, SelectFlag = N.BCID == Cat.BCID });
                Item = new D_PayType_APIItem
                {
                    DPTID = N.DPTID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    CategoryList = SIs,
                    Code = N.Code,
                    Title = N.Title,

                    AccountingNo = N.AccountingNo,
                    CustormerNo = N.CustormerNo,
                    InvoiceFlag = N.InvoiceFlag,
                    DepositFlag = N.DepositFlag,
                    RestaurantFlag = N.RestaurantFlag,
                    SimpleCheckoutFlag = N.SimpleCheckoutFlag,
                    SimpleDepositFlag = N.SimpleDepositFlag,

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
                var N_ = DC.D_PayType.FirstOrDefault(q => q.DPTID == ID && !q.DeleteFlag);
                if (N_ != null)
                {
                    N_.ActiveFlag = ActiveFlag;
                    N_.UpdDate = DT;
                    N_.UpdUID = UID;
                    DC.SaveChanges();
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
                var N_ = DC.D_PayType.FirstOrDefault(q => q.DPTID == ID);
                if (N_ != null)
                {
                    N_.DeleteFlag = true;
                    N_.UpdDate = DT;
                    N_.UpdUID = UID;
                    DC.SaveChanges();
                }
                else
                    Error += "查無資料,無法更新;";
            }

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpPost]
        public string Submit(D_PayType N)
        {
            Error = "";
            if (N.DPTID == 0)
            {
                if (N.CreUID == 0)
                    Error += "缺少建立者ID,無法更新;";
                if (N.BCID <= 0)
                    Error += "請選擇付款方式所屬分類;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    DC.D_PayType.Add(N);
                    DC.SaveChanges();
                }
            }
            else
            {
                var N_ = DC.D_PayType.FirstOrDefault(q => q.DPTID == N.DPTID && !q.DeleteFlag);
                if (N.CreUID == 0)
                    Error += "缺少更新者ID,無法更新;";
                if (N.BCID <= 0)
                    Error += "請選擇付款方式所屬分類;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.BCID = N.BCID;
                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.AccountingNo = N.AccountingNo;
                    N_.CustormerNo = N.CustormerNo;
                    N_.InvoiceFlag = N.InvoiceFlag;
                    N_.DepositFlag = N.DepositFlag;
                    N_.RestaurantFlag = N.RestaurantFlag;
                    N_.SimpleCheckoutFlag = N.SimpleCheckoutFlag;
                    N_.SimpleDepositFlag = N.SimpleDepositFlag;
                    N_.ActiveFlag = N.ActiveFlag;
                    N_.DeleteFlag = N.DeleteFlag;
                    N_.UpdUID = N.UpdUID;
                    N_.UpdDate = DT;
                    DC.SaveChanges();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}