using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.View;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TwitchLib.Api;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly IChatterService chatterService;

        public ChatbotController(PlaylistHelper playlistHelper, IChatterService chatterService)
        {
            this.playlistHelper = playlistHelper;

            this.chatterService = chatterService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated ? new LoggedInTwitchUser
            {
                Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)?.Value,
                IsMod = chattersModel.chatters.moderators.Any(mod => string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
            } : null;

            var playlistModel = playlistHelper.GetAllSongs(twitchUser);

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            return View(playlistModel);
        }

        [HttpPost]
        public IActionResult RenderRegularList([FromBody]PlaylistItem[] data)
        {
            try
            {
                var chattersModel = chatterService.GetCurrentChatters();

                var twitchUser = User.Identity.IsAuthenticated ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)?.Value,
                    IsMod = chattersModel.chatters.moderators.Any(mod => string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
                } : null;

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                return PartialView("Partials/List/RegularList", data);
            }
            catch (Exception e)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderVipList([FromBody] PlaylistItem[] data)
        {
            try
            {
                var chattersModel = chatterService.GetCurrentChatters();

                var twitchUser = User.Identity.IsAuthenticated
                    ? new LoggedInTwitchUser
                    {
                        Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                            ?.Value,
                        IsMod = chattersModel.chatters.moderators.Any(mod =>
                            string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
                    }
                    : null;

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                return PartialView("Partials/List/VipList", data);
            }
            catch (Exception e)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderModal([FromBody] int songId)
        {
            var requestToDelete = playlistHelper.GetRequestById(songId);

            try
            {
                return PartialView("Partials/List/DeleteModal", requestToDelete);
            }
            catch (Exception e)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSong([FromBody] int songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();

                if (chattersModel.chatters.moderators.Any(mod =>
                    string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (playlistHelper.ArchiveRequestById(songId))
                        return Ok();
                }
            }

            return Json(new {Success = false, Message = "Encountered an error, or you are not a moderator" });
        }
    }
}
