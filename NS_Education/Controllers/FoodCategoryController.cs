using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models.APIItems.FoodCategory;
using NS_Education.Models.Entities;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controllers
{
    public class FoodCategoryController : PublicClass
    {
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetList(string KeyWord = "", int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.D_FoodCategory.Where(q => !q.DeleteFlag);
            
            if (KeyWord != "")
                Ns = Ns.Where(q => q.Title.Contains(KeyWord) || q.Code.Contains(KeyWord));

            D_FoodCategory_List ListData = new D_FoodCategory_List();
            ListData.Items = new List<D_FoodCategory_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q => q.ActiveFlag).OrderBy(q => q.Title);
            else
                Ns = Ns.OrderBy(q => q.Title).Skip((NowPage - 1) * CutPage).Take(CutPage);

            var NsList = await Ns.ToListAsync();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                ListData.Items.Add(new D_FoodCategory_APIItem
                {
                    DFCID = N.DFCID,
                    Code = N.Code,
                    Title = N.Title,

                    UnitPrice = N.UnitPrice,
                    Price = N.Price,
                    
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, null, null)]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == ID && !q.DeleteFlag);
            D_FoodCategory_APIItem Item = null;
            if (N != null)
            {
                Item = new D_FoodCategory_APIItem
                {
                    DFCID = N.DFCID,
                    Code = N.Code,
                    Title = N.Title,

                    UnitPrice = N.UnitPrice,
                    Price = N.Price,

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
            var N_ = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == ID && !q.DeleteFlag);
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
            var N_ = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == ID);
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(D_FoodCategory.DFCID))]
        public async Task<string> Submit(D_FoodCategory N)
        {
            Error = "";
            if (N.DFCID == 0)
            {
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.UnitPrice < 0)
                    Error += "請輸入成本的數字;";
                if (N.Price < 0)
                    Error += "請輸入價格的數字;";
                if (Error == "")
                {
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.D_FoodCategory.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else
            {
                var N_ = await DC.D_FoodCategory.FirstOrDefaultAsync(q => q.DFCID == N.DFCID && !q.DeleteFlag);
                if (N.Title == "")
                    Error += "名稱必須輸入;";
                if (N.UnitPrice < 0)
                    Error += "請輸入成本的數字;";
                if (N.Price < 0)
                    Error += "請輸入價格的數字;";

                if (N_ == null)
                    Error += "查無資料,無法更新";
                if (Error == "")
                {
                    
                    N_.Code = N.Code;
                    N_.Title = N.Title;
                    N_.UnitPrice = N.UnitPrice;
                    N_.Price = N.Price;
                    
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