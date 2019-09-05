using System;
using System.Collections.Generic;
using System.Linq;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"followage"}, false)]
    public class FollowageCommand : ICommand
    {
        private ConfigModel config;
        private TwitchAPI twitchApi;

        public FollowageCommand(ConfigModel config, TwitchAPI twitchApi)
        {
            this.config = config;
            this.twitchApi = twitchApi;
        }
        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            try
            {
                var users = await twitchApi.Helix.Users.GetUsersAsync(logins: new List<string>(new[] {username}));
                var userId = users.Users[0].Id;

                if (userId == null) return;

                var follows = await twitchApi.Helix.Users.GetUsersFollowsAsync(fromId:userId, toId:config.ChannelId);

                var followedChannel = follows?.Follows?.SingleOrDefault();
                if (followedChannel == null) return;

                var monthsFollowed = Math.Abs(12 * (followedChannel.FollowedAt.Year - DateTime.UtcNow.Year) +
                                              followedChannel.FollowedAt.Month - DateTime.UtcNow.Month);

                client.SendMessage(joinedChannel,
                    $"Hey @{username}, you have followed {config.StreamerChannel} for {monthsFollowed} months!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how long you've followed {config.StreamerChannel}!");
        }
    }
}
