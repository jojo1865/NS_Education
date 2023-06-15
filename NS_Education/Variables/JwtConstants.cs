using System.Security.Claims;

namespace NS_Education.Variables
{
    public class JwtConstants
    {
        // TODO: key 應該做保護
        public const string Secret = "uNU-<}k>Ui~xz\"UcgtY'$wa37bB2.B>T!uN!+>:XXbxYPH/G-C`g~I1G*nf1lo9";

        public const int ExpireMinutes = 720;

        public const int AdminGid = 1;

        public static string UidClaimType => ClaimTypes.Actor;
    }
}