using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using CoreCodedChatbot.Helpers;

using CoreCodedChatbot.Web.Models;

namespace CoreCodedChatbot.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
