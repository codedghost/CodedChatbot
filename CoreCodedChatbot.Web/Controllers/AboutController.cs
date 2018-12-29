using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
