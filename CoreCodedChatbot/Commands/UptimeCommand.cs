using System;
using System.Collections.Generic;
using System.Linq;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "uptime", "live" }, false)]
    public class UptimeCommand : ICommand
    {
        private readonly ITwitchApiFactory _twitchApiFactory;
        private readonly IConfigService _configService;

        public UptimeCommand(ITwitchApiFactory twitchApiFactory, IConfigService configService)
        {
            _twitchApiFactory = twitchApiFactory;
            _configService = configService;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var twitchApi = _twitchApiFactory.Get();
            var userIds = new List<string>
            {
                _configService.Get<string>("ChannelId")
            };

            var stream = await twitchApi.Helix.Streams.GetStreamsAsync(userIds: userIds);
            var streamGoLiveTime = stream?.Streams.FirstOrDefault()?.StartedAt;

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
