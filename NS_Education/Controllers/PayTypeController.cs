using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.PayType;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers
{
    public class PayTypeController : PublicClass
    {
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(string KeyWord = "", int BCID = 0, int NowPage = 1, int CutPage = 10)
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

            var NsList = await Ns.ToListAsync();
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
                    BC_List = null,
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
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                }); ; ;
            }

            return ChangeJson(ListData);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == ID && !q.DeleteFlag);
            D_PayType_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Cats = DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == 8).OrderBy(q => q.SortNo);
                foreach (var Cat in await Cats.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Cat.BCID, Title = Cat.TitleC, SelectFlag = N.BCID == Cat.BCID });
                Item = new D_PayType_APIItem
                {
                    DPTID = N.DPTID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    BC_List = SIs,
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, null, null)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag, null, null)]
        public async Task<string> DeleteItem(int ID)
        {
            Error = "";
            var N_ = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == ID);
            if (N_ != null)
            {
                N_.DeleteFlag = true;
                N_.UpdDate = DT;
                N_.UpdUID = GetUid();
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_PayType.DPTID))]
        public async Task<string> Submit(D_PayType N)
        {
            Error = "";
            if (N.DPTID == 0)
            {
                if (N.BCID <= 0)
                    Error += "請選擇付款方式所屬分類;";
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_PayType.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_PayType.FirstOrDefaultAsync(q => q.DPTID == N.DPTID && !q.DeleteFlag);
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
                    N_.UpdUID = GetUid();
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}