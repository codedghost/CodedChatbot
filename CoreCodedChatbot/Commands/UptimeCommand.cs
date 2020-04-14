using System;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "uptime", "live" }, false)]
    public class UptimeCommand : ICommand
    {
        private readonly TwitchAPI api;
        private readonly IConfigService _configService;

        public UptimeCommand(TwitchAPI api, IConfigService configService)
        {
            this.api = api;
            _configService = configService;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var Stream = await api.V5.Streams.GetStreamByUserAsync(_configService.Get<string>("ChannelId"));
            var streamGoLiveTime = Stream?.Stream?.CreatedAt;

            if (streamGoLiveTime != null)
            {
                var timeLiveFor = DateTime.UtcNow.Subtract(streamGoLiveTime.Value.ToUniversalTime());

                client.SendMessage(joinedChannel, $"Hey @{username}, {_configService.Get<string>("StreamerChannel")} has been live for: {timeLiveFor.Hours} hours and {timeLiveFor.Minutes} minutes.");
            }
            else
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, {_configService.Get<string>("StreamerChannel")} seems to be offline right now");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs how long the stream has been live!");
        }
    }
}
