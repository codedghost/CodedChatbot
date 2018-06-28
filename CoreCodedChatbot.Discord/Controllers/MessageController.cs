using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Discord.Services;
using CoreCodedChatbot.Discord.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Discord.Controllers
{
    [Produces("application/json")]
    [Route("api/Message")]
    public class MessageController : Controller
    {
        private IDiscordService discordService { get; set; }

        public MessageController(IDiscordService discordService)
        {
            this.discordService = discordService;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            discordService.SendMessage("Hello, someone made me say this!").Wait();
            return new string[] { "value1", "value2" };
        }
    }
}