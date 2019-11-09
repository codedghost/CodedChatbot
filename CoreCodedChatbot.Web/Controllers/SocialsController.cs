using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class SocialsController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}