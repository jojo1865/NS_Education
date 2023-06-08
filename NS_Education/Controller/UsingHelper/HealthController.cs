using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Controller.UsingHelper
{
    public class HealthController : PublicClass
    {
        [HttpGet]
        public Task<string> Ping()
        {
            return Task.FromResult(GetResponseJson());
        }
    }
}