using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class MediaController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}