using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"followage"}, false)]
    public class FollowageCommand : ICommand
    {
        private readonly IConfigService _configService;
        private readonly ITwitchApiFactory _twitchApiFactory;
        private readonly ILogger<FollowageCommand> _logger;

        public FollowageCommand(
            IConfigService configService, 
            ITwitchApiFactory twitchApiFactory,
            ILogger<FollowageCommand> logger)
        {
            _configService = configService;
            _twitchApiFactory = twitchApiFactory;
            _logger = logger;
        }
        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            try
            {
                var twitchApi = _twitchApiFactory.Get();
                var users = await twitchApi.Helix.Users.GetUsersAsync(logins: new List<string>(new[] {username}));
                var userId = users.Users[0].Id;

                if (userId == null) return;

                var follows = await twitchApi.Helix.Users.GetUsersFollowsAsync(fromId:userId, toId: _configService.Get<string>("ChannelId"));

                var followedChannel = follows?.Follows?.SingleOrDefault();
                if (followedChannel == null) return;

                var monthsFollowed = Math.Abs(12 * (followedChannel.FollowedAt.Year - DateTime.UtcNow.Year) +
                                              followedChannel.FollowedAt.Month - DateTime.UtcNow.Month);

                client.SendMessage(joinedChannel,
                    $"Hey @{username}, you have followed {_configService.Get<string>("StreamerChannel")} for {monthsFollowed} months!");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in FollowageCommand");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how long you've followed {_configService.Get<string>("StreamerChannel")}!");
        }
    }
}
