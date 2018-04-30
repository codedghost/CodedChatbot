using System;

using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Api;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "uptime", "live" }, false)]
    public class UptimeCommand : ICommand
    {
        private readonly ConfigModel config;
        private readonly TwitchAPI api;

        public UptimeCommand(TwitchAPI api, IConfigHelper configHelper)
        {
            this.api = api;
            this.config = configHelper.GetConfig();
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var Stream = await api.Streams.v5.GetStreamByUserAsync(config.ChannelId);
            var streamGoLiveTime = Stream.Stream.CreatedAt.ToUniversalTime();

            var timeLiveFor = DateTime.Now.ToUniversalTime().Subtract(streamGoLiveTime);

            client.SendMessage(config.StreamerChannel, $"Hey @{username}, {config.StreamerChannel} has been live for: {timeLiveFor.Hours} hours and {timeLiveFor.Minutes} minutes.");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs how long the stream has been live!");
        }
    }
}
