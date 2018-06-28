using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodedChatbot.Discord.Services.Interfaces
{
    public interface IDiscordService
    {
        Task Initialise();
        Task SendMessage(string message);
    }
}
