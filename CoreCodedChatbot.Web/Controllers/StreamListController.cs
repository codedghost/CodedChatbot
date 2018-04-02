using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using CoreCodedChatbot.Helpers;

using CoreCodedChatbot.Web.Models;


namespace CoreCodedChatbot.Web.Controllers
{
    public class StreamListController : Controller
    {
        public ActionResult Index(string accessKey)
        {
            var browserSourceModel = new PlaylistBrowserSource
            {
                Songs = PlaylistHelper.GetTopSongs()
        };

            return View(browserSourceModel);
        }
    }
}