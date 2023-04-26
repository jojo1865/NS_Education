using System.Web.Mvc;

namespace NS_Education.Controller
{
    public class HomeController : System.Web.Mvc.Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
