using System;

using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers.Interfaces;

using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "uptime", "live" }, false)]
    public class UptimeCommand : ICommand
    {
        private readonly IConfigHelper configHelper;
        private readonly TwitchAPI api;

        public UptimeCommand(TwitchAPI api, IConfigHelper configHelper)
        {
            this.api = api;
            this.configHelper = configHelper;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var config = configHelper.GetConfig();
            var channel = await api.Channels.v5.GetChannelAsync(config.ChatbotAccessToken);

            var Stream = await api.Streams.v5.GetStreamByUserAsync(channel.Id);
            var streamGoLiveTime = Stream.Stream.CreatedAt.ToUniversalTime();

            var timeLiveFor = DateTime.Now.ToUniversalTime().Subtract(streamGoLiveTime);

            client.SendMessage($"Hey @{username}, {config.StreamerChannel} has been live for: {timeLiveFor.Hours} hours and {timeLiveFor.Minutes} minutes.");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command outputs how long the stream has been live!");
        }
    }
}
