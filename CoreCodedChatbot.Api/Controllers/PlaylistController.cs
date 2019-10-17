﻿using System;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Api.Controllers
{
    [Route("Playlist/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        public IActionResult TestEndpoint()
        {
            return new JsonResult(new {Message = "Authorized!"});
        }

        [HttpPost]
        public IActionResult EditRequest([FromBody] EditSongRequest editSongRequest)
        {
            var success = _playlistService.EditRequest(editSongRequest.username, editSongRequest.commandText, editSongRequest.isMod, 
                out string songRequestText, out bool syntaxError);

            if (success)
            {
                var editResult = new EditRequestResponse
                {
                    SongRequestText = songRequestText,
                    SyntaxError = syntaxError
                };
                return new JsonResult(editResult);
            }
            else
                return BadRequest();
        }

        [HttpPost]
        public IActionResult GetUserRequests([FromBody] string username)
        {
            var requests = _playlistService.GetUserRequests(username);

            var requestsResult = new GetUserRequestsResponse
            {
                UserRequests = requests
            };

            return new JsonResult(requestsResult);
        }

        public IActionResult OpenPlaylist()
        {
            if (_playlistService.OpenPlaylist())
                return Ok();

            return BadRequest();
        }

        public IActionResult VeryClosePlaylist()
        {
            if (_playlistService.VeryClosePlaylist())
                return Ok();

            return BadRequest();
        }

        public IActionResult ClosePlaylist()
        {
            if (_playlistService.ClosePlaylist())
                return Ok();

            return BadRequest();
        }

        public IActionResult IsPlaylistOpen()
        {
            return Json(_playlistService.GetPlaylistState());
        }

        public IActionResult ArchiveCurrentRequest()
        {
            try
            {
                _playlistService.ArchiveCurrentRequest();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult RemoveRockRequests([FromBody] RemoveSongRequest removeSongRequest)
        {
            if (_playlistService.RemoveRockRequests(removeSongRequest.username, removeSongRequest.commandText, removeSongRequest.isMod))
                return Ok();

            return BadRequest();
        }

        [HttpPost]
        public IActionResult RemoveSuperVip([FromBody] RemoveSuperVipRequest requestModel)
        {
            if (_playlistService.RemoveSuperRequest(requestModel.username)) return Ok();

            return BadRequest();
        }

        [HttpPost]
        public IActionResult AddRequest([FromBody] AddSongRequest requestModel)
        {
            var addRequestResult = _playlistService.AddRequest(requestModel.username, requestModel.commandText, requestModel.isVipRequest);
            return new JsonResult(new AddRequestResponse
            {
                Result = addRequestResult.Item1,
                PlaylistPosition = addRequestResult.Item2
            });
        }

        [HttpPost]
        public IActionResult AddSuperRequest([FromBody] AddSuperVipRequest requestModel)
        {
            var addSuperVipResult = _playlistService.AddSuperVipRequest(requestModel.username, requestModel.commandText);

            return new JsonResult(new AddRequestResponse
            {
                Result = addSuperVipResult
            });
        }

        [HttpPost]
        public IActionResult EditSuperVipRequest([FromBody] EditSuperVipRequest requestModel)
        {
            var editSuperVipResult =
                _playlistService.EditSuperVipRequest(requestModel.username, requestModel.commandText);

            return new JsonResult(new EditRequestResponse
            {
                SongRequestText = editSuperVipResult,
                SyntaxError = string.IsNullOrWhiteSpace(editSuperVipResult)
            });
        }

        [HttpPost]
        public IActionResult PromoteRequest([FromBody] PromoteSongRequest promoteSongRequest)
        {
            return new JsonResult(_playlistService.PromoteRequest(promoteSongRequest.username));
        }

        [HttpGet]
        public IActionResult ClearRequests()
        {
            try
            {
                _playlistService.ClearRockRequests();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
