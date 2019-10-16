using System;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "uptime", "live" }, false)]
    public class UptimeCommand : ICommand
    {
        private readonly ConfigModel config;
        private readonly TwitchAPI api;

        public UptimeCommand(TwitchAPI api, IConfigService configService)
        {
            this.api = api;
            this.config = configService.GetConfig();
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var Stream = await api.V5.Streams.GetStreamByUserAsync(config.ChannelId);
            var streamGoLiveTime = Stream?.Stream?.CreatedAt;

            if (streamGoLiveTime != null)
            {
                var timeLiveFor = DateTime.UtcNow.Subtract(streamGoLiveTime.Value.ToUniversalTime());

                client.SendMessage(joinedChannel, $"Hey @{username}, {config.StreamerChannel} has been live for: {timeLiveFor.Hours} hours and {timeLiveFor.Minutes} minutes.");
            }
            else
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, {config.StreamerChannel} seems to be offline right now");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs how long the stream has been live!");
        }
    }
}
