using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class AboutController : Controller
    {
        public AboutController()
        {

        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
