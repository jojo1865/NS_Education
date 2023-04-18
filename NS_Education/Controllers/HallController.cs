using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models;
using NS_Education.Models.APIItems.Hall;
using NS_Education.Models.Entities;
using NS_Education.Tools.Filters;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers
{
    public class HallController : PublicClass
    {
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(string KeyWord = "", int DDID = 0, int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_Hall.Where(q => !q.DeleteFlag);
            if (DDID > 0)
                Ns = Ns.Where(q => q.DDID == DDID);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.TitleC.Contains(KeyWord) || q.TitleC.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_Hall_List ListData = new D_Hall_List();
            ListData.Items = new List<D_Hall_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q => q.ActiveFlag).OrderBy(q => q.TitleC);
            else
                Ns = Ns.OrderBy(q => q.TitleC).Skip((NowPage - 1) * CutPage).Take(CutPage);

            Ns.Include(h => h.DD);
            
            var NsList = await Ns.ToListAsync();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                ListData.Items.Add(new D_Hall_APIItem
                {
                    DHID = N.DHID,
                    DDID = N.DDID,
                    DD_TitleC = N.DD.TitleC,
                    DD_TitleE = N.DD.TitleE,
                    DD_List = null,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,

                    DiscountFlag = N.DiscountFlag,
                    CheckoutNowFlag = N.CheckoutNowFlag,
                    PrintCheckFlag = N.PrintCheckFlag,
                    Invoice3Flag = N.Invoice3Flag,
                    CheckType = N.CheckType,
                    BusinessTaxRate = N.BusinessTaxRate,

                    DeviceCt = N.B_Device.Count,
                    SiteCt = N.B_SiteData.Count,
                    PartnerItemCt = N.B_PartnerItem.Count,

                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                });
            }

            return ChangeJson(ListData);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Hall.Include(q => q.DD).FirstOrDefaultAsync(q => q.DDID == ID && !q.DeleteFlag);
            D_Hall_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Deps = DC.D_Department.Where(q => !q.DeleteFlag).OrderBy(q => q.TitleC);
                
                foreach (var Dep in await Deps.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Dep.DDID, Title = Dep.TitleC, SelectFlag = N.DDID == Dep.DDID });
                Item = new D_Hall_APIItem
                {
                    DDID = N.DDID,
                    DHID = N.DHID,
                    DD_TitleC = N.DD.TitleC,
                    DD_TitleE = N.DD.TitleE,
                    DD_List = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,

                    DiscountFlag = N.DiscountFlag,
                    CheckoutNowFlag = N.CheckoutNowFlag,
                    PrintCheckFlag = N.PrintCheckFlag,
                    Invoice3Flag = N.Invoice3Flag,
                    CheckType = N.CheckType,
                    BusinessTaxRate = N.BusinessTaxRate,

                    DeviceCt = N.B_Device.Count,
                    SiteCt = N.B_SiteData.Count,
                    PartnerItemCt = N.B_PartnerItem.Count,

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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.D_Hall.FirstOrDefaultAsync(q => q.DHID == ID && !q.DeleteFlag);
            if (N_ != null)
            {
                N_.ActiveFlag = ActiveFlag;
                N_.UpdDate = DT;
                N_.UpdUID = FilterStaticTools.GetUidInRequestInt(Request);
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int ID)
        {
            Error = "";
            var N_ = await DC.D_Hall.FirstOrDefaultAsync(q => q.DHID == ID);
            if (N_ != null)
            {
                N_.DeleteFlag = true;
                N_.UpdDate = DT;
                N_.UpdUID = FilterStaticTools.GetUidInRequestInt(Request);
                await DC.SaveChangesAsync();
            }
            else
                Error += "查無資料,無法更新;";

            return ChangeJson(GetMsgClass(Error));
        }

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, "DHID")]
        public async Task<string> Submit(D_Hall N)
        {
            Error = "";
            if (N.DHID == 0)
            {
                if (N.DDID <= 0)
                    Error += "請選擇廳別所屬部門;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.CreUID = FilterStaticTools.GetUidInRequestInt(Request);
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Hall.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Hall.FirstOrDefaultAsync(q => q.DHID == N.DHID && !q.DeleteFlag);
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
                    N_.UpdUID = FilterStaticTools.GetUidInRequestInt(Request);
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}