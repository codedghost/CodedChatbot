using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.SongLibrary;
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
        private readonly IPlaylistService playlistService;

        private readonly IChatterService chatterService;

        public ChatbotController(IPlaylistService playlistService, IChatterService chatterService)
        {
            this.playlistService = playlistService;

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

            var playlistModel = playlistService.GetAllSongs(twitchUser);
            playlistModel.RegularList =
                playlistModel.RegularList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();
            playlistModel.VipList =
                playlistModel.VipList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
            ViewBag.Username = twitchUser?.Username ?? string.Empty;
            return View(playlistModel);
        }

        [HttpGet]
        public IActionResult Library()
        {
            using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "SongsMasterGrid.json")))
            {
                return View(JsonConvert.DeserializeObject<SongLibraryRecords>(sr.ReadToEnd()));
            }
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
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
                return PartialView("Partials/List/RegularList", data);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Encountered an error" });
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
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
                return PartialView("Partials/List/VipList", data);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Encountered an error" });
            }
        }

        [HttpPost]
        public IActionResult RenderRemoveSongModal([FromBody] int songId)
        {
            var requestToDelete = playlistService.GetRequestById(songId);

            try
            {
                return PartialView("Partials/List/DeleteModal", requestToDelete);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Encountered an error" });
            }
        }

        [HttpPost]
        public IActionResult RenderRemoveCurrentSongModal([FromBody] int songId)
        {
            var requestToDelete = playlistService.GetRequestById(songId);
            try
            {
                return PartialView("Partials/List/RemoveCurrentModal", requestToDelete);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderCurrentSong([FromBody] PlaylistItem data)
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
                return PartialView("Partials/List/CurrentSong", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RemoveSong([FromBody] int songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();
                var request = playlistService.GetRequestById(songId);

                if (chattersModel.chatters.moderators.Any(mod =>
                    string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ||
                    string.Equals(User.Identity.Name, request.songRequester))
                {
                    if (playlistService.ArchiveRequestById(songId))
                        return Ok();
                }
            }

            return Json(new { Success = false, Message = "Encountered an error, are you certain you're logged in?" });
        }

        [HttpPost]
        public IActionResult RemoveCurrentSong([FromBody] int songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();

                if (chattersModel.chatters.moderators.Any(mod =>
                    string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    try
                    {
                        playlistService.ArchiveCurrentRequest(songId);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return Json(new { Success = false, Message = "Encountered an error, or you are not a moderator" });
                    }
                }
            }

            return Json(new { Success = false, Message = "Encountered an error, or you are not a moderator" });
        }
    }
}
