﻿using System.Collections.Generic;
using System.Linq;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"shoutout", "so"}, true)]
    public class ShoutoutCommand : ICommand
    {
        private readonly ITwitchApiFactory _twitchApiFactory;

        public ShoutoutCommand(ITwitchApiFactory twitchApiFactory)
        {
            _twitchApiFactory = twitchApiFactory;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandSplit = commandText.Split(" ");

            if (!commandSplit.Any() || commandSplit.Length != 1 || !commandSplit[0].Contains("@"))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you need to @ someone to shout them out!");
                return;
            }

            var twitchApi = _twitchApiFactory.Get();

            // Query twitch api to ensure that this user exists.
            var apiResult = await twitchApi.Helix.Users.GetUsersAsync(logins: new List<string> {commandSplit[0].Trim('@')});

            var verifiedUser = apiResult.Users.FirstOrDefault();
            if (verifiedUser == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I think you may have typed that username wrong! :O");
                return;
            }

            client.SendMessage(joinedChannel, $"Go check out the awesome {verifiedUser.DisplayName} over at https://twitch.tv/{verifiedUser.Login} Make sure to drop a follow and check them out when they're next live!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will shoutout the user you @ mention! Usage: !so @<username>");
        }
    }
}
