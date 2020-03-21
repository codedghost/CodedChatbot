using System;
using CoreCodedChatbot.Interfaces;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "kitty" }, false)]
    public class KittyCommand : ICommand
    {
        private IRedditHelper _redditHelper;
        private readonly ILogger<KittyCommand> _logger;

        public KittyCommand(
            IRedditHelper redditHelper,
            ILogger<KittyCommand> logger)
        {
            _redditHelper = redditHelper;
            _logger = logger;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            try
            {
                var postInfo = await _redditHelper.GetRandomPost("cats");
                if (postInfo == null)
                {
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, I can't seem to talk to Reddit right now, try again in a few minutes :(");
                    return;
                }
                
                // Put post link in chat (should attribute reddit and poster rather than posting media directly)
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, {postInfo.Title} - {postInfo.Url}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in KittyCommand");
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I'm having some trouble talking to reddit right now, sorry :(");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, This command will give you a picture of a friendly feline!");
        }
    }
}
