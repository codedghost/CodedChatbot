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
using CoreCodedChatbot.Library.Models.Enums;
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
        private readonly IVipService vipService;

        private readonly IChatterService chatterService;

        public ChatbotController(IPlaylistService playlistService, IVipService vipService,
            IChatterService chatterService)
        {
            this.playlistService = playlistService;
            this.vipService = vipService;

            this.chatterService = chatterService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            var playlistModel = playlistService.GetAllSongs(twitchUser);
            playlistModel.RegularList =
                playlistModel.RegularList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();
            playlistModel.VipList =
                playlistModel.VipList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();

            ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

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
        public IActionResult RenderRegularList([FromBody] PlaylistItem[] data)
        {
            try
            {
                var chattersModel = chatterService.GetCurrentChatters();

                var twitchUser = User.Identity.IsAuthenticated
                    ? new LoggedInTwitchUser
                    {
                        Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                            ?.Value,
                        IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                    string.Equals(mod, User.Identity.Name,
                                        StringComparison.CurrentCultureIgnoreCase)) ?? false
                    }
                    : null;

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
                return PartialView("Partials/List/RegularList", data);
            }
            catch (Exception)
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
                        IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                    string.Equals(mod, User.Identity.Name,
                                        StringComparison.CurrentCultureIgnoreCase)) ?? false
                    }
                    : null;

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
                return PartialView("Partials/List/VipList", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderRemoveSongModal([FromBody] int songId)
        {
            var requestToDelete = playlistService.GetRequestById(songId);

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            try
            {
                return PartialView("Partials/List/DeleteModal", requestToDelete);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderRemoveCurrentSongModal([FromBody] int songId)
        {
            var requestToDelete = playlistService.GetRequestById(songId);

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

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

                var twitchUser = User.Identity.IsAuthenticated
                    ? new LoggedInTwitchUser
                    {
                        Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                            ?.Value,
                        IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                    string.Equals(mod, User.Identity.Name,
                                        StringComparison.CurrentCultureIgnoreCase)) ?? false
                    }
                    : null;

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                return PartialView("Partials/List/CurrentSong", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderRequestModal()
        {
            try
            {
                var requestViewModel = playlistService.GetNewRequestSongViewModel(User.Identity.Name.ToLower());

                return PartialView("Partials/List/RequestModal", requestViewModel);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderEditRequestModal([FromBody] int songId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new {Success = false, Message = "It looks like you aren't logged in!"});
            }

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            try
            {
                var requestViewModel =
                    playlistService.GetEditRequestSongViewModel(User.Identity.Name.ToLower(), songId,
                        twitchUser?.IsMod ?? false);

                return PartialView("Partials/List/RequestModal", requestViewModel);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderPromoteSongModal([FromBody] int songId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new {Success = false, Message = "It looks like you aren't logged in!"});
            }

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            try
            {
                var requestToPromote = playlistService.GetRequestById(songId);

                return PartialView("Partials/List/PromoteSongModal", requestToPromote);
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

                if ((chattersModel?.chatters?.moderators?.Any(mod =>
                         string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ?? false) ||
                    string.Equals(User.Identity.Name, request.songRequester))
                {
                    if (playlistService.ArchiveRequestById(songId))
                        return Ok();
                }
            }

            return Json(new {Success = false, Message = "Encountered an error, are you certain you're logged in?"});
        }

        [HttpPost]
        public IActionResult RemoveCurrentSong([FromBody] int songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();

                if (chattersModel?.chatters?.moderators?.Any(mod =>
                        string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ?? false)
                {
                    try
                    {
                        playlistService.ArchiveCurrentRequest(songId);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return Json(new
                            {Success = false, Message = "Encountered an error, or you are not a moderator"});
                    }
                }
            }

            return Json(new {Success = false, Message = "Encountered an error, or you are not a moderator"});
        }

        [HttpPost]
        public IActionResult RequestSong([FromBody] RequestSongViewModel requestData)
        {
            if (User.Identity.IsAuthenticated)
            {
                var requestResult = playlistService.AddWebRequest(requestData, User.Identity.Name);

                switch (requestResult)
                {
                    case AddRequestResult.Success:
                        return Ok();
                    case AddRequestResult.NoMultipleRequests:
                        return BadRequest(new
                        {
                            Message =
                                $"You cannot have more than {playlistService.GetMaxUserRequests()} regular request{(playlistService.GetMaxUserRequests() > 1 ? "s" : "")}"
                        });
                    case AddRequestResult.PlaylistClosed:
                        return BadRequest(new
                        {
                            Message =
                                "The playlist is currently closed, you can still use a VIP token to request though!"
                        });
                    case AddRequestResult.PlaylistVeryClosed:
                        return BadRequest(new
                        {
                            Message =
                                "The playlist is completely closed, please wait until the playlist opens to request a song"
                        });
                    case AddRequestResult.UnSuccessful:
                        return BadRequest(new
                        {
                            Message = "An error occurred, please wait until the issue is resolved"
                        });
                    case AddRequestResult.NoRequestEntered:
                        return BadRequest(new
                        {
                            Message = "You haven't entered a request. Please enter a Song Name and/or Artist"
                        });
                }
            }

            return BadRequest(new {Message = "It looks like you're not logged in, log in and try again"});
        }

        [HttpPost]
        public IActionResult EditSong([FromBody] RequestSongViewModel requestData)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chatters = chatterService.GetCurrentChatters();
                var userIsMod = chatters?.chatters?.moderators?.Any(m => m == User.Identity.Name.ToLower()) ?? false;

                var editRequestResult =
                    playlistService.EditWebRequest(requestData, User.Identity.Name.ToLower(), userIsMod);

                switch (editRequestResult)
                {
                    case EditRequestResult.Success:
                        return Ok();
                    case EditRequestResult.NoRequestEntered:
                        return BadRequest(new
                        {
                            Message = "You haven't entered a request. Please enter a Song name and/or Artist"
                        });
                    case EditRequestResult.NotYourRequest:
                        return BadRequest(new
                        {
                            Message = "This doesn't seem to be your request. Please try again"
                        });
                    case EditRequestResult.RequestAlreadyRemoved:
                        return BadRequest(new
                        {
                            Message = "It seems like this song has been played or removed from the list"
                        });
                    default:
                        return BadRequest(new
                        {
                            Message = "An error occurred, please wait until the issue is resolved"
                        });
                }
            }

            return BadRequest(new {Message = "It looks like you're not logged in, log in and try again"});
        }

        [HttpPost]
        public IActionResult PromoteRequest([FromBody] int songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var promoteRequestResult = playlistService.PromoteWebRequest(songId, User.Identity.Name.ToLower());

                switch (promoteRequestResult)
                {
                    case PromoteRequestResult.NotYourRequest:
                        return BadRequest(new
                        {
                            Message = "This is not your request. Please try again"
                        });
                    case PromoteRequestResult.AlreadyVip:
                        return BadRequest(new
                        {
                            Message = "This request has already been promoted! Congratulations"
                        });
                    case PromoteRequestResult.NoVipAvailable:
                        return BadRequest(new
                        {
                            Message = "Sorry but you don't seem to have a VIP token"
                        });
                    case PromoteRequestResult.Successful:
                        return Ok();
                    default:
                        return BadRequest(new
                        {
                            Message = "An error occurred, please wait until the issue is resolved"
                        });
                }
            }

            return BadRequest(new {Message = "It looks like you're not logged in, log in and try again"});
        }

        public IActionResult RenderAddToDriveModal([FromBody] int songId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new {Success = false, Message = "It looks like you aren't logged in!"});
            }

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            try
            {
                var requestToAddToDrive = playlistService.GetRequestById(songId);

                return PartialView("Partials/List/AddToDriveModal", requestToAddToDrive);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        public IActionResult AddSongToDrive([FromBody] int songId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { Success = false, Message = "It looks like you aren't logged in!" });
            }

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            try
            {
                playlistService.AddSongToDrive(songId);
                return Ok();
            }
            catch (Exception e)
            {
                Console.Error.Write($"{e} - {e.InnerException}");
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        public IActionResult RenderEmptyPlaylistModal()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new {Success = false, Message = "It looks like you aren't logged in!"});
            }

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            if (!ViewBag.UserIsMod) return BadRequest();

            try
            {
                return PartialView("Partials/List/EmptyPlaylistModal");
            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        public IActionResult EmptyPlaylist()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { Success = false, Message = "It looks like you aren't logged in!" });
            }

            var chattersModel = chatterService.GetCurrentChatters();

            var twitchUser = User.Identity.IsAuthenticated
                ? new LoggedInTwitchUser
                {
                    Username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value,
                    IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                                string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                            false
                }
                : null;

            ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

            if (!ViewBag.UserIsMod) return BadRequest();

            try
            {
                playlistService.ClearRockRequests();
                return Ok();
            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }
    }
}
