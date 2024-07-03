using System.Security.Claims;
using System.Web.Configuration;

namespace NS_Education.Variables
{
    public class JwtConstants
    {
        public static readonly string Secret = WebConfigurationManager.AppSettings["JwtSecret"];

        public static string UidClaimType => ClaimTypes.Actor;
    }
}