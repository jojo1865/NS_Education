using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Controllers
{
    /// <summary>
    /// 入帳代號 B_OrderCode 的 Controller。
    /// </summary>
    public class OrderCodeController : PublicClass,
        IGetTypeList<B_OrderCode>
    {
        #region Initialization
        
        private readonly IGetTypeListHelper _getTypeListHelper;
        
        /// <summary>
        /// 靜態參數類別名稱對照表。<br/>
        /// 內容在建構式 populate。<br/>
        /// 在 ASP.NET 中，端點每次被呼叫都會是新的 Controller，所以沒有需要 refresh 的問題。
        /// </summary>
        private readonly Dictionary<string, B_OrderCode> OrderCodeTypes;
        
        public OrderCodeController()
        {
            OrderCodeTypes = DC.B_OrderCode
                .Where(sc => sc.ActiveFlag && !sc.DeleteFlag)
                .Where(sc => sc.CodeType == 0)
                .OrderBy(sc => sc.Code)
                .ThenBy(sc => sc.SortNo)
                // EF 不支援 GroupBy，所以回到本地在記憶體做
                .AsEnumerable()
                // CodeType 和 Code 並不是 PK，有可能有多筆同樣 CodeType Code 的資料，所以這裡各種 Code 只取一筆，以免重複 Key
                .GroupBy(sc => sc.Code)
                .ToDictionary(group => group.Key, group => group.First());
            
            _getTypeListHelper = new GetTypeListHelper<OrderCodeController, B_OrderCode>(this);
        }
        
        #endregion

        #region GetTypeList

        public async Task<string> GetTypeList()
        {
            return await _getTypeListHelper.GetTypeList();
        }

        public IOrderedQueryable<B_OrderCode> GetTypeListQuery()
        {
            return DC.B_OrderCode
                .Where(oc => oc.ActiveFlag && oc.CodeType == 0)
                .OrderBy(oc => oc.SortNo);
        }

        public async Task<BaseResponseRowForType> GetTypeListEntityToRow(B_OrderCode entity)
        {
            return await Task.Run(() => new BaseResponseRowForType
            {
                ID = int.Parse(entity.Code),
                Title = entity.Title
            });
        }

        #endregion

    }
}