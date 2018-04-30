using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly PlaylistHelper playlistHelper;

        public PlaylistController(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public ActionResult Index()
        {
            var playlistModel = new PlaylistBrowserSource
            {
                Songs = playlistHelper.GetAllSongs()
        };

            return View(playlistModel);
        }
    }
}
