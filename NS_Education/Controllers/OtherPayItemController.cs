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
    public class OtherPayItemController : PublicClass
    {
        [HttpGet]
        public string GetList(string KeyWord = "", int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_OtherPayItem.Where(q => !q.DeleteFlag);
            
            if (KeyWord != "")
                Ns = Ns.Where(q => q.Title.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_OtherPayItem_List ListData = new D_OtherPayItem_List();
            ListData.Items = new List<D_OtherPayItem_APIItem>();
            ListData.SuccessFlag = Ns.Count() > 0;
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;
            ListData.AllItemCt = Ns.Count();
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);

            if (NowPage == 0)
                Ns = Ns.Where(q => q.ActiveFlag).OrderBy(q => q.Title);
            else
                Ns = Ns.OrderBy(q => q.Title).Skip((NowPage - 1) * CutPage).Take(CutPage);

            foreach (var N in Ns)
            {
                ListData.Items.Add(new D_OtherPayItem_APIItem
                {
                    DOPIID = N.DOPIID,
                    Code = N.Code,
                    Title = N.Title,

                    Ct = N.Ct,
                    UnitPrice = N.UnitPrice,
                    InPrice = N.InPrice,
                    OutPrice = N.OutPrice,

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
        public D_OtherPayItem_APIItem GetInfoByID(int ID = 0)
        {
            var N = DC.D_OtherPayItem.FirstOrDefault(q => q.DOPIID == ID && !q.DeleteFlag);
            D_OtherPayItem_APIItem Item = null;
            if (N != null)
            {
                Item = new D_OtherPayItem_APIItem
                {
                    DOPIID = N.DOPIID,
                    Code = N.Code,
                    Title = N.Title,

                    Ct = N.Ct,
                    UnitPrice = N.UnitPrice,
                    InPrice = N.InPrice,
                    OutPrice = N.OutPrice,

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }

            return Item;
        }
        [HttpGet]
        public string ChangeActive(int ID, bool ActiveFlag, int UID)
        {
            Error = "";
            if (UID == 0)
                Error += "缺少更新者ID,無法更新;";
            else
            {
                var N_ = DC.D_OtherPayItem.FirstOrDefault(q => q.DOPIID == ID && !q.DeleteFlag);
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
                var N_ = DC.D_OtherPayItem.FirstOrDefault(q => q.DOPIID == ID);
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
        public string Submit(D_OtherPayItem N)
        {
            Error = "";
            if (N.DOPIID == 0)
            {
                if (N.CreUID == 0)
                    Error += "缺少建立者ID,無法更新;";
               
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.UnitPrice < 0)
                    Error += "請輸入成本的數字;";
                if (N.InPrice < 0)
                    Error += "請輸入內部價格的數字;";
                if (N.OutPrice < 0)
                    Error += "請輸入外部價格的數字;";
                if (Error == "")
                {
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    DC.D_OtherPayItem.InsertOnSubmit(N);
                    DC.SubmitChanges();
                }
            }
            else
            {
                var N_ = DC.D_OtherPayItem.FirstOrDefault(q => q.DOPIID == N.DOPIID && !q.DeleteFlag);
                if (N.CreUID == 0)
                    Error += "缺少更新者ID,無法更新;";
                
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.UnitPrice < 0)
                    Error += "請輸入成本的數字;";
                if (N.InPrice < 0)
                    Error += "請輸入內部價格的數字;";
                if (N.OutPrice < 0)
                    Error += "請輸入外部價格的數字;";

                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    
                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.Ct = N.Ct;
                    N_.UnitPrice = N.UnitPrice;
                    N_.InPrice = N.InPrice;
                    N_.OutPrice = N.OutPrice;
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