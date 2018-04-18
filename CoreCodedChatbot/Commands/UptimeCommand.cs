using System;
using System.Threading;

using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;
using TwitchLib.Api;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "uptime", "live" }, false)]
    public class UptimeCommand : ICommand
    {
        private readonly TwitchAPI api;

        private readonly ConfigModel config = ConfigHelper.GetConfig();

        public UptimeCommand(TwitchAPI api)
        {
            this.api = api;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var config = ConfigHelper.GetConfig();
            var channel = await api.Channels.v5.GetChannelAsync(config.ChatbotAccessToken);

            var Stream = await api.Streams.v5.GetStreamByUserAsync(channel.Id);
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
