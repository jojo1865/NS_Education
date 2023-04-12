using NS_Education.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Category;
using NS_Education.Models.Entities;
using NS_Education.Tools.Filters;


namespace NS_Education.Controllers
{
    public class CategoryController : PublicClass
    {
        //取得分類的類別列表
        [HttpGet]
        public string GetTypeList()
        {
            List<cSelectItem> TIs = new List<cSelectItem>();
            for (int i = 0; i < sCategoryTypes.Length; i++)
                TIs.Add(new cSelectItem { ID = i, Title = sCategoryTypes[i] });
            return ChangeJson(TIs);
        }

        //取得分類列表
        [HttpGet]
        public string GetList(string KeyWord = "", int CategoryType = -1, int NowPage = 1, int CutPage = 10)
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

            var NsList = Ns.ToList();
            ListData.SuccessFlag = NsList.Any();
            ListData.Message = ListData.SuccessFlag ? "" : "查無資料";
            ListData.AllItemCt = NsList.Count;
            ListData.AllPageCt = NowPage == 0 ? 0 : (ListData.AllItemCt % CutPage == 0 ? ListData.AllItemCt / CutPage : (ListData.AllItemCt / CutPage) + 1);
            
            foreach (var N in NsList)
            {
                B_Category BC_P = null;
                if (N.ParentID > 0)
                {
                    BC_P = DC.B_Category.FirstOrDefault(q => q.BCID == N.ParentID && !q.DeleteFlag);
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
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
                    UpdUID = (N.CreDate != N.UpdDate ? N.UpdUID : 0)
                });
            }
            return ChangeJson(ListData);
        }
        //取得分類的內容
        [HttpGet]
        [JwtAuthFilter]
        public string GetInfoByID(int ID = 0)
        {
            var N = DC.B_Category.FirstOrDefault(q => !q.DeleteFlag && q.BCID == ID);
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
                    BC_P = DC.B_Category.FirstOrDefault(q => q.BCID == N.ParentID && !q.DeleteFlag);
                    if (BC_P != null)
                    {
                        BC_Ps = new List<cSelectItem>();
                        var _BC_Ps = DC.B_Category.Where(q => q.ParentID == BC_P.ParentID && !q.DeleteFlag).OrderBy(q => q.SortNo);
                        foreach (var _BC in _BC_Ps)
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
                    CreUser = GetUserNameByID(N.CreUID),
                    CreUID = N.CreUID,
                    UpdDate = (N.CreDate != N.UpdDate ? N.UpdDate.ToString(DateTimeFormat) : ""),
                    UpdUser = (N.CreDate != N.UpdDate ? GetUserNameByID(N.UpdUID) : ""),
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
        public string ChangeActive(int ID, bool ActiveFlag, int UID)
        {
            Error = "";
            if (UID == 0)
                Error += "缺少更新者ID,無法更新;";
            else
            {
                var N_ = DC.B_Category.FirstOrDefault(q => q.BCID == ID && !q.DeleteFlag);
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
                var N_ = DC.B_Category.FirstOrDefault(q => q.BCID == ID);
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
        public string Submit(B_Category N)
        {
            string Error = "";
            if (N.BCID == 0)//新增
            {
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (Error == "")
                {
                    var BCs = DC.B_Category.Where(q => !q.DeleteFlag && q.CategoryType == N.CategoryType);
                    if (BCs.Any())
                        N.SortNo = BCs.Max(q => q.SortNo) + 1;
                    N.UpdDate = N.CreDate = DT;
                    N.UpdUID = 0;
                    DC.B_Category.Add(N);
                    DC.SaveChanges();
                }
            }
            else//更新
            {
                var N_ = DC.B_Category.FirstOrDefault(q => q.BCID == N.BCID && !q.DeleteFlag);
                if (N.TitleC == "")
                    Error += "名稱必須輸入;";
                if (N_ == null)
                    Error += "查無資料,無法更新;";
                if (Error == "")
                {
                    N_.CategoryType = N.CategoryType;
                    N_.Code = N.Code;
                    N_.TitleC = N.TitleC;
                    N_.TitleE = N.TitleE;
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