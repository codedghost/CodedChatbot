using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.ChatInfo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatInfoApiController : Controller
    {
        private readonly IChatInfoService _chatInfoService;

        public ChatInfoApiController(IChatInfoService chatInfoService)
        {
            _chatInfoService = chatInfoService;
        }

        [HttpPost]
        public IActionResult RemoveInfo([FromBody] RemoveInfoRequestModel model)
        {
            try
            {
                return Ok(_chatInfoService.RemoveInfo(model.Aliases));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"RemoveInfo\nException:\n{e}\nInner:\n{e.InnerException}");
                return BadRequest();
            }
        }
    }
}