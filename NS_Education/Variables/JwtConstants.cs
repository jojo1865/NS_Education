using System.Security.Claims;
using System.Web.Configuration;

namespace NS_Education.Variables
{
    public class JwtConstants
    {
        public const int ExpireMinutes = 720;

        public const int AdminGid = 1;
        public static readonly string Secret = WebConfigurationManager.AppSettings["JwtSecret"];

        public static string UidClaimType => ClaimTypes.Actor;
    }
}