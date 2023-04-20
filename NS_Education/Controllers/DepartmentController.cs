using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models;
using NS_Education.Models.APIItems.Department;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers
{
    public class DepartmentController : PublicClass
    {
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(string KeyWord = "", int DCID = 0, int NowPage = 1, int CutPage = 10)
        {

            var Ns = DC.D_Department.Where(q => !q.DeleteFlag);
            if (DCID > 0)
                Ns = Ns.Where(q => q.DCID == DCID);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.TitleC.Contains(KeyWord) || q.TitleC.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_Department_List ListData = new D_Department_List();
            ListData.Items = new List<D_Department_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q=>q.ActiveFlag).OrderBy(q => q.TitleC);
            else
                Ns = Ns.OrderBy(q => q.TitleC).Skip((NowPage - 1) * CutPage).Take(CutPage);

            Ns = Ns.Include(q => q.DC);
            
            var NsList = await Ns.ToListAsync();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                ListData.Items.Add(new D_Department_APIItem
                {
                    DDID = N.DDID,
                    DCID = N.DCID,
                    DC_TitleC = N.DC.TitleC,
                    DC_TitleE = N.DC.TitleE,
                    DC_List = null,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    PeopleCt = N.PeopleCt,
                    HallCt = N.D_Hall.Count,
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_Department.Include(q => q.DC).FirstOrDefaultAsync(q => q.DDID == ID && !q.DeleteFlag);
            D_Department_APIItem Item = null;
            if (N != null)
            {
                List<cSelectItem> SIs = new List<cSelectItem>();
                var Coms = DC.D_Company.Where(q => !q.DeleteFlag).OrderBy(q => q.TitleC);
                foreach (var Com in await Coms.ToListAsync())
                    SIs.Add(new cSelectItem { ID = Com.DCID, Title = Com.TitleC, SelectFlag = N.DCID == Com.DCID });
                Item = new D_Department_APIItem
                {
                    DDID = N.DDID,
                    DCID = N.DCID,
                    DC_TitleC = N.DC.TitleC,
                    DC_TitleE = N.DC.TitleE,
                    DC_List = SIs,
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    PeopleCt = N.PeopleCt,
                    HallCt = N.D_Hall.Count,
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
            var N_ = await DC.D_Department.FirstOrDefaultAsync(q => q.DDID == ID && !q.DeleteFlag);
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
            var N_ = await DC.D_Department.FirstOrDefaultAsync(q => q.DDID == ID);
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_Department.DDID))]
        public async Task<string> Submit(D_Department N)
        {
            Error = "";
            if (N.DDID == 0)
            {
                if(N.DCID<=0)
                    Error += "請選擇部門所屬公司;";
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if(Error=="")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_Department.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_Department.FirstOrDefaultAsync(q => q.DDID == N.DDID && !q.DeleteFlag);
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
                    N_.UpdUID = GetUid();
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}