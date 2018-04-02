using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class PlaylistController : Controller
    {
        public ActionResult Index()
        {
            var playlistModel = new PlaylistBrowserSource
            {
                Songs = PlaylistHelper.GetAllSongs()
        };

            return View(playlistModel);
        }
    }
}
