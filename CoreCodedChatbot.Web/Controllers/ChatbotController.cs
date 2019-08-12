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
        private readonly IUserService userService;

        private readonly IChatterService chatterService;

        public ChatbotController(IPlaylistService playlistService, IVipService vipService,
            IChatterService chatterService, IUserService userService)
        {
            this.playlistService = playlistService;
            this.vipService = vipService;
            this.userService = userService;

            this.chatterService = chatterService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var chattersModel = chatterService.GetCurrentChatters();
            LoggedInTwitchUser twitchUser = null;

            if (User.Identity.IsAuthenticated)
            {
                var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                    ?.Value;
                twitchUser = new LoggedInTwitchUser
                {
                    Username = username,
                    IsMod = chattersModel?.IsUserMod(username) ?? false
                };

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
            }

            var playlistModel = playlistService.GetAllSongs(twitchUser);
            playlistModel.RegularList =
                playlistModel.RegularList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();
            playlistModel.VipList =
                playlistModel.VipList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();

            ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

            ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;

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

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                    {
                        Username = username,
                        IsMod = chattersModel?.IsUserMod(username) ?? false
                    };

                    ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                    ViewBag.Username = twitchUser?.Username ?? string.Empty;
                }

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;

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

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                        {
                            Username = username,
                            IsMod = chattersModel?.IsUserMod(username) ?? false
                        };

                    ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                    ViewBag.Username = twitchUser?.Username ?? string.Empty;
                }

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;
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
            CheckAndSetUserModStatus();
            if (!User.Identity.IsAuthenticated) return BadRequest();

            var requestToDelete = playlistService.GetRequestById(songId);
            
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
            if (!CheckAndSetUserModStatus()) return BadRequest();

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
                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                        {
                            Username = username,
                            IsMod = chattersModel?.IsUserMod(username) ?? false
                        };
                    ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                }

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;
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
            CheckAndSetUserModStatus();
            if (!User.Identity.IsAuthenticated) return BadRequest();

            try
            {
                var requestViewModel =
                    playlistService.GetEditRequestSongViewModel(User.Identity.Name.ToLower(), songId,
                        ViewBag.UserIsMod ?? false);

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
            CheckAndSetUserModStatus();
            if (!User.Identity.IsAuthenticated) return BadRequest();

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

                if ((chattersModel?.IsUserMod(User.Identity.Name) ?? false) ||
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

                if (chattersModel?.IsUserMod(User.Identity.Name) ?? false)
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

                var editRequestResult =
                    playlistService.EditWebRequest(requestData, User.Identity.Name.ToLower(),
                        chatters?.IsUserMod(User.Identity.Name.ToLower()) ?? false);

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
            if (!CheckAndSetUserModStatus()) return BadRequest();

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

        public IActionResult RenderConvertBytesModal()
        {
            try
            {
                var twitchUser = GetTwitchUser();

                if (twitchUser == null) return BadRequest();

                return PartialView("Partials/List/ConvertBytesModal", twitchUser);
            }
            catch (Exception)
            {
                return Json(new {Success = false, message = "Encountered an error"});
            }
        }

        public IActionResult AddSongToDrive([FromBody] int songId)
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

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
            if (!CheckAndSetUserModStatus()) return BadRequest();

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
            if (!CheckAndSetUserModStatus()) return BadRequest();

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

        public IActionResult RenderOpenPlaylistModal()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                return PartialView("Partials/List/OpenPlaylistModal");
            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        public IActionResult RenderVeryClosePlaylistModal()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                return PartialView("Partials/List/VeryClosePlaylistModal");
            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        public IActionResult OpenPlaylist()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                if (playlistService.OpenPlaylistWeb())
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        public IActionResult VeryClosePlaylist()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                if (playlistService.VeryClosePlaylistWeb())
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        public IActionResult ConvertBytes([FromBody] int bytesToConvert)
        {
            try
            {
                var twitchUser = GetTwitchUser();

                if (twitchUser == null) BadRequest();

                // convert bytes

                return Ok();

            }
            catch (Exception e)
            {
                Console.Write($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        private bool CheckAndSetUserModStatus()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return false;
            }

            var chattersModel = chatterService.GetCurrentChatters();

            if (User.Identity.IsAuthenticated)
            {
                var username = User?.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                                   ?.Value ?? string.Empty;
                var twitchUser = new LoggedInTwitchUser
                {
                    Username = username,
                    IsMod = chattersModel.IsUserMod(username)
                };

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
            }

            return ViewBag.UserIsMod;
        }

        private LoggedInTwitchUser GetTwitchUser()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var chattersModel = chatterService.GetCurrentChatters();
            var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)?.Value;
            var userBalance = userService.GetUserVipByteBalance(username);

            var twitchUser = new LoggedInTwitchUser
            {
                Username = username,
                IsMod = chattersModel?.chatters?.moderators?.Any(mod =>
                            string.Equals(mod, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                        false,
                Vips = userBalance.Vips,
                Bytes = userBalance.Bytes
            };

            return twitchUser;
        }
    }
}
