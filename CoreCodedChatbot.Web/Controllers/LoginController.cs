using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly ConfigModel config;

        public LoginController(IConfigService configService)
        {
            this.config = configService.GetConfig();
        }

        public IActionResult Index()
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = "/" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return LocalRedirect("/");
        }
    }
}