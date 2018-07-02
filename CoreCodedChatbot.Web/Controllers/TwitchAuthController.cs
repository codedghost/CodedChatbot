using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class TwitchAuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}