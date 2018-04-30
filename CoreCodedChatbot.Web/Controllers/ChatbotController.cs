using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly PlaylistHelper playlistHelper;

        public ChatbotController(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            var playlistModel = new PlaylistBrowserSource
            {
                Songs = playlistHelper.GetAllSongs()
            };

            return View(playlistModel);
        }
    }
}
