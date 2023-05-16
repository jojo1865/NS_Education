using System.Collections.Generic;
using NS_Education.Tools.Filters.ResponsePrivilegeWrapper;

namespace NS_Education.Models
{
    public class FinalizedResponse
    {
        public int Status { get; set; }
        public string StatusMessage { get; set; }
        public object ApiResponse { get; set; }
        public IEnumerable<Privilege> Privileges { get; set; }
    }
}