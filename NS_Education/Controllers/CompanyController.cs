using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models;
using NS_Education.Models.APIItems.Company;
using NS_Education.Models.Entities;

namespace NS_Education.Controllers
{
    public class CompanyController : PublicClass
    {
        [HttpGet]
        public async Task<string> GetList(string KeyWord = "", int BCID = 0, int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_Company.Where(q => !q.DeleteFlag);
            if (BCID > 0)
                Ns = Ns.Where(q => q.BCID == BCID);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.TitleC.Contains(KeyWord) || q.TitleC.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_Company_List ListData = new D_Company_List();
            ListData.Items = new List<D_Company_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q=>q.ActiveFlag).OrderBy(q => q.TitleC);
            else
                Ns = Ns.OrderBy(q => q.TitleC).Skip((NowPage - 1) * CutPage).Take(CutPage);

            Ns = Ns.Include(q => q.BC);
            
            var NsList = await Ns.ToListAsync();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                ListData.Items.Add(new D_Company_APIItem
                {
                    DCID = N.DCID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    CategoryList = null,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    DepartmentCt = N.D_Department.Count,
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
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Company.Include(q => q.BC).FirstOrDefaultAsync(q => q.DCID == ID && !q.DeleteFlag);
            D_Company_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Cats = DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == 1).OrderBy(q => q.SortNo);
                foreach (var Cat in await Cats.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Cat.BCID, Title = Cat.TitleC, SelectFlag = N.BCID == Cat.BCID });
                Item = new D_Company_APIItem
                {
                    DCID = N.DCID,
                    BCID = N.BCID,
                    BC_TitleC = N.BC.TitleC,
                    BC_TitleE = N.BC.TitleE,
                    CategoryList = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    DepartmentCt = N.D_Department.Count,
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
                var N_ = await DC.D_Company.FirstOrDefaultAsync(q => q.DCID == ID && !q.DeleteFlag);
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
                var N_ = await DC.D_Company.FirstOrDefaultAsync(q => q.DCID == ID);
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
        public async Task<string> Submit(D_Company N)
        {
            Error = "";
            if (N.DCID == 0)
            {
                if (N.CreUID == 0)
                    Error += "缺少建立者ID,無法更新;";
                if (N.BCID <= 0)
                    Error += "請選擇公司所屬分類;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Company.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Company.FirstOrDefaultAsync(q => q.DCID == N.DCID && !q.DeleteFlag);
                if (N.CreUID == 0)
                    Error += "缺少更新者ID,無法更新;";
                if (N.BCID <= 0)
                    Error += "請選擇公司所屬分類;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    N_.BCID = N.BCID;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
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