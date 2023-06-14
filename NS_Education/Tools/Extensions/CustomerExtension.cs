using System.Linq;
using NS_Education.Models.Entities;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    public static class CustomerExtension
    {
        public static int GetDealtReservationCount(this Customer customer)
        {
            return customer.Resver_Head.Count(rh =>
                !rh.DeleteFlag && rh.B_StaticCode.Code != ReserveHeadState.Draft);
        }
    }
}