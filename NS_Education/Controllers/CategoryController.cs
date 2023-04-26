using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models;
using NS_Education.Models.APIItems.Category;
using NS_Education.Models.Entities;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Tools.Filters.ResponsePrivilegeWrapper;

namespace NS_Education.Controllers
{
    public class CategoryController : PublicClass
    {
        //取得分類的類別列表
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetTypeList()
        {
            List<cSelectItem> TIs = new List<cSelectItem>();
            await Task.Run(() =>
                TIs.AddRange(sCategoryTypes.Select((t, i) => new cSelectItem { ID = i, Title = t })));
            return ChangeJson(TIs);
        }

        //取得分類列表
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(string KeyWord = "", int CategoryType = -1, int NowPage = 1, int CutPage = 10)
        {
            var Ns = DC.B_Category.Where(q => !q.DeleteFlag);
            if (CategoryType >= 0)
                Ns = Ns.Where(q => q.CategoryType == CategoryType);
            if (KeyWord != "")
                Ns = Ns.Where(q => q.TitleC.Contains(KeyWord) || q.TitleC.Contains(KeyWord) || q.Code.Contains(KeyWord));

            B_Category_List ListData = new B_Category_List();
            ListData.Items = new List<B_Category_APIItem>();
            ListData.NowPage = NowPage;
            ListData.CutPage = CutPage;

            if (NowPage == 0)
                Ns = Ns.Where(q => q.ActiveFlag).OrderBy(q => q.SortNo);
            else
                Ns = Ns.OrderBy(q => q.SortNo).Skip((NowPage - 1) * CutPage).Take(CutPage);

            var NsList = await Ns.ToListAsync();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                B_Category BC_P = null;
                if (N.ParentID > 0)
                {
                    BC_P = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == N.ParentID && !q.DeleteFlag);
                }
                ListData.Items.Add(new B_Category_APIItem()
                {
                    BCID = N.BCID,
                    iCategoryType = N.CategoryType,
                    sCategoryType = sCategoryTypes[N.CategoryType],
                    CategoryTypeList = null,
                    ParentID = N.ParentID,
                    ParentList = null,
                    ParentTitleC = (BC_P != null ? BC_P.TitleC : ""),
                    ParentTitleE = (BC_P != null ? BC_P.TitleE : ""),
                    Code = N.Code == null ? "" : N.Code,
                    TitleC = N.TitleC == null ? "" : N.TitleC,
                    TitleE = N.TitleE == null ? "" : N.TitleE,
                    SortNo = N.SortNo,
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
        //取得分類的內容
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        [ResponsePrivilegeWrapperFilter]
        public async Task<string> GetInfoByID(int ID = 0)
        {
            var N = await DC.B_Category.FirstOrDefaultAsync(q => !q.DeleteFlag && q.BCID == ID);
            List<cSelectItem> TIs = new List<cSelectItem>();
            for (int i = 0; i < sCategoryTypes.Length; i++)
                TIs.Add(new cSelectItem { ID = i, Title = sCategoryTypes[i], SelectFlag = (N == null ? i == 0 : i == N.CategoryType) });
            B_Category_APIItem Item = new B_Category_APIItem();
            if (N != null)
            {
                B_Category BC_P = null;
                List<cSelectItem> BC_Ps = null;
                if (N.ParentID > 0)
                {
                    BC_P = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == N.ParentID && !q.DeleteFlag);
                    if (BC_P != null)
                    {
                        BC_Ps = new List<cSelectItem>();
                        var _BC_Ps = DC.B_Category.Where(q => q.ParentID == BC_P.ParentID && !q.DeleteFlag).OrderBy(q => q.SortNo);
                        foreach (var _BC in await _BC_Ps.ToListAsync())
                            BC_Ps.Add(new cSelectItem { ID = _BC.BCID, Title = _BC.TitleC, SelectFlag = _BC.BCID == BC_P.BCID });
                    }
                }
                Item = new B_Category_APIItem()
                {
                    BCID = N.BCID,
                    iCategoryType = N.CategoryType,
                    sCategoryType = sCategoryTypes[N.CategoryType],
                    CategoryTypeList = TIs,
                    ParentID = N.ParentID,
                    ParentList = BC_Ps,
                    ParentTitleC = (BC_P != null ? BC_P.TitleC : ""),
                    ParentTitleE = (BC_P != null ? BC_P.TitleE : ""),
                    Code = N.Code,
                    TitleC = N.TitleC,
                    TitleE = N.TitleE,
                    SortNo = N.SortNo,
                    ActiveFlag = N.ActiveFlag,
                    CreDate = N.CreDate.ToString(DateTimeFormat),
                    CreUser = await GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? await GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                };
            }
            else
            {
                Item.CategoryTypeList = TIs;
            }
            return ChangeJson(Item);
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int ID, bool ActiveFlag)
        {
            Error = "";
            var N_ = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == ID && !q.DeleteFlag);
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(int ID)
        {
            Error = "";
            var N_ = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == ID);
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddOrEdit, null, nameof(B_Category.BCID))]
        public async Task<string> Submit(B_Category N)
        {
            string Error = "";
            if (N.BCID == 0)//新增
            {
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    var BCs = await DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == N.CategoryType).ToListAsync();
                    if (BCs.Any())
                        N.SortNo = BCs.Max(q => q.SortNo) + 1;
                    N.CreUID = GetUid();
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    await DC.B_Category.AddAsync(N);
                    await DC.SaveChangesAsync();
                }
            }
            else//更新
            {
                var N_ = await DC.B_Category.FirstOrDefaultAsync(q => q.BCID == N.BCID && !q.DeleteFlag);
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新;";
                if (Error == "")
                {
                    N_.ParentID = N.ParentID;
                    N_.CategoryType = N.CategoryType;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
                    N_.ActiveFlag = N.ActiveFlag;
                    N_.UpdUID = GetUid();
                    N_.UpdDate = DT;
                    await DC.SaveChangesAsync();
                }
            }
            return ChangeJson(GetMsgClass(Error));
        }
    }
}