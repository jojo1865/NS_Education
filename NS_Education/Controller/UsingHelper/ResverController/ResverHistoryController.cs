using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Controller.Resver.GetHistory;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.ResverController
{
    public class ResverHistoryController : PublicClass,
        IGetListAll<Resver_Head_Log, Resver_GetHistory_Input_APIItem, Resver_GetHistory_Output_Row_APIItem>
    {
        #region Initialization

        private readonly IGetListAllHelper<Resver_GetHistory_Input_APIItem> _getListAllHelper;

        public ResverHistoryController()
        {
            _getListAllHelper =
                new GetListAllHelper<ResverHistoryController, Resver_Head_Log, Resver_GetHistory_Input_APIItem,
                    Resver_GetHistory_Output_Row_APIItem>(this);
        }

        #endregion

        #region GetHistory

        /// <summary>
        /// 取得單筆預約單的操作歷史紀錄。
        /// </summary>
        /// <param name="input"><see cref="Resver_GetHistory_Input_APIItem"/></param>
        /// <returns><see cref="Resver_GetHistory_Output_Row_APIItem"/> 的集合。</returns>
        /// <remarks>實際 Route 請參照 RouteConfig</remarks>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(Resver_GetHistory_Input_APIItem input)
        {
            return await _getListAllHelper.GetAllList(input);
        }

        public async Task<bool> GetListAllValidateInput(Resver_GetHistory_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.RHID.IsAboveZero(),
                    () => AddError(WrongFormat("預約單 ID", nameof(input.RHID))))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IOrderedQueryable<Resver_Head_Log> GetListAllOrderedQuery(Resver_GetHistory_Input_APIItem input)
        {
            var query = DC.Resver_Head_Log
                .Include(rhl => rhl.UserData);

            query = query.Where(rhl => rhl.RHID == input.RHID);

            return query.OrderByDescending(rhl => rhl.CreDate);
        }

        public Task<Resver_GetHistory_Output_Row_APIItem> GetListAllEntityToRow(Resver_Head_Log entity)
        {
            return Task.FromResult(new Resver_GetHistory_Output_Row_APIItem
            {
                TypeName = ((ResverHistoryType?)entity.Type).GetChineseName(),
                CreDate = entity.CreDate.ToFormattedStringDateTime(),
                CreUser =
                    $"{entity.CreUID} {entity.UserData?.UserName ?? entity.UserData?.LoginAccount ?? String.Empty}"
            });
        }

        #endregion
    }
}