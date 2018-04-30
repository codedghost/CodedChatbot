using Microsoft.AspNetCore.Mvc;

using CoreCodedChatbot.Helpers;

using CoreCodedChatbot.Web.Models;


namespace CoreCodedChatbot.Web.Controllers
{
    public class StreamListController : Controller
    {
        private readonly PlaylistHelper playlistHelper;

        public StreamListController(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public ActionResult Index()
        {
            var browserSourceModel = new PlaylistBrowserSource
            {
                Songs = playlistHelper.GetTopSongs()
        };

            return View(browserSourceModel);
        }
    }
}