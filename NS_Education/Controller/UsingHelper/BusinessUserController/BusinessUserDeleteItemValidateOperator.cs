using System.Collections.Generic;
using System.Linq;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;

namespace NS_Education.Controller.UsingHelper.BusinessUserController
{
    public class BusinessUserDeleteItemValidateOperator : IDeleteItemValidateReservation<Resver_Head>
    {
        /// <inheritdoc />
        public IQueryable<Resver_Head> SupplyQueryWithInputIdCondition(IQueryable<Resver_Head> basicQuery,
            HashSet<int> uniqueDeleteId)
        {
            return basicQuery
                .Where(rh => uniqueDeleteId.Contains(rh.OP_BUID));
        }

        /// <inheritdoc />
        public object GetInputId(Resver_Head cantDelete)
        {
            return cantDelete.OP_BUID;
        }

        /// <inheritdoc />
        public int GetHeadId(Resver_Head cantDelete)
        {
            return cantDelete.RHID;
        }
    }
}