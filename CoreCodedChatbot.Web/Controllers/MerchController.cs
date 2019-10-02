using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class MerchController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View("Merch");
        }
    }
}