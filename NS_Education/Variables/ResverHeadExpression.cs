using System;
using System.Linq.Expressions;
using NS_Education.Models.Entities;

namespace NS_Education.Variables
{
    public static class ResverHeadExpression
    {
        /// <summary>
        /// 檢查一筆預約單是否處於有效狀態。
        /// </summary>
        public static readonly Expression<Func<Resver_Head, bool>> IsDealtExpression =
            rh => !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft;
    }
}