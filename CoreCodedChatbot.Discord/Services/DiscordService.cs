using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Discord.Helpers.Interfaces;
using CoreCodedChatbot.Discord.Models.DataModels;
using CoreCodedChatbot.Discord.Services.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CoreCodedChatbot.Discord.Services
{
    public class DiscordService : IDiscordService
    {
        private DiscordClient discord { get; set; }
        private DiscordChannel channelToRespond { get; set; }

        private Config config { get; set; }

        public DiscordService(IConfigHelper configHelper)
        {
            this.config = configHelper.GetConfig();
        }

        public async Task Initialise()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = config.DiscordBotToken,
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += async e =>
            {
                if (e.MentionedChannels.Any())
                    channelToRespond = e.MentionedChannels[0];

                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }

        public async Task SendMessage(string message)
        {
            await channelToRespond.SendMessageAsync(message);
        }
    }
}
