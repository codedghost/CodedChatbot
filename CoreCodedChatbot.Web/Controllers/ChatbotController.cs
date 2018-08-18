using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.View;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TwitchLib.Api;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly PlaylistHelper playlistHelper;
        private readonly ConfigModel config;

        public ChatbotController(PlaylistHelper playlistHelper, IConfigHelper configHelper)
        {
            this.playlistHelper = playlistHelper;
            this.config = configHelper.GetConfig();
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> List()
        {
            var httpClient = new HttpClient();
            var currentChattersJson = await httpClient.GetStringAsync($"https://tmi.twitch.tv/group/user/{config.StreamerChannel}/chatters");
            // process json into username list.
            var chattersModel = JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson);

            var twitchUser = User.Identity.IsAuthenticated ? new LoggedInTwitchUser
            {
                Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)?.Value,
                IsMod = chattersModel.chatters.moderators.Any(mod => string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
            } : null;

            var playlistModel = new PlaylistBrowserSource
            {
                Songs = playlistHelper.GetAllSongs(),
                TwitchUser = twitchUser
            };

            return View(playlistModel);
        }

        public IActionResult RenderList(PlaylistItem[] data)
        {
            return PartialView("Partials/List/Playlist", data);
        }
    }
}
